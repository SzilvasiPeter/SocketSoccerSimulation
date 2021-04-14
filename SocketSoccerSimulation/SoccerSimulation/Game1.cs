using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoccerSimulation.Modell;

namespace SoccerSimulation
{
    public class Game1 : Game
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _backgroundTexture;
        private Texture2D _ballTexture;
        private Texture2D _goalkeeperTexture;

        private int _switchGoalKeeperSideMoving = 0;

        private double _leftGate;
        private double _rightGate;
        private double _goalLine;
        private int _screenWidth;
        private Rectangle _ballRectangle;
        private Rectangle _goalkeeperRectangle;
        private Vector2 _ballPosition;
        private Vector2 _initialBallPosition;

        private Socket _socket;
        private readonly object _lockObject = new object();
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private readonly float[] _coordinates = new float[2] {400f, 0.01f };

        public Game1()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            ResetWindowSize();
            Window.ClientSizeChanged += (s, e) => ResetWindowSize();

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 5500);
            _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(endPoint);
            _socket.Listen(100);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _backgroundTexture = Content.Load<Texture2D>("soccer_field");
            _ballTexture = Content.Load<Texture2D>("ball");
            _goalkeeperTexture = Content.Load<Texture2D>("goal_keeper");
        }

        protected override void Update(GameTime gameTime)
        {
            _resetEvent.Reset();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Shoot the ball

            _ballPosition.X = (int)MathHelper.LerpPrecise(_ballPosition.X, _coordinates[0], _coordinates[1]);
            _ballPosition.Y = (int)MathHelper.LerpPrecise(_ballPosition.Y, 0, _coordinates[1]);

            UpdateGoalKeeper();
            IsGoalKeeperDefended();
            IsGoalLanded();

            // Update ball position
            _ballRectangle.X = (int)_ballPosition.X;
            _ballRectangle.Y = (int)_ballPosition.Y;

            base.Update(gameTime);
        }

        protected override void EndRun()
        {
            _socket.Dispose();
            base.EndRun();
        }

        private void ResetWindowSize()
        {
            int screenWidth = Window.ClientBounds.Width;
            int screenHeight = Window.ClientBounds.Height;
            _screenWidth = screenWidth;
            // Init Gate value
            _leftGate = screenWidth * 0.375;
            _rightGate = screenWidth * 0.623;
            _goalLine = screenHeight * 0.05;

            // Init ball
            var ballDimension =
                (screenWidth > screenHeight) ? (int)(screenWidth * 0.02) : (int)(screenHeight * 0.035);
            _initialBallPosition = new Vector2(screenWidth / 2.0f, screenHeight * 0.8f);
            _ballPosition = new Vector2(_initialBallPosition.X, _initialBallPosition.Y);
            _ballRectangle = new Rectangle((int)_initialBallPosition.X, (int)_initialBallPosition.Y, ballDimension,
                ballDimension);

            // Init Goal keeper
            int goalKeeperWidth = (int)_goalLine;
            int goalKeeperHeight = (int)(screenWidth * 0.015);
            int goalkeeperPositionX = (screenWidth - goalKeeperWidth) / 2;
            int goalkeeperPositionY = (int)(screenHeight * 0.12);
            _goalkeeperRectangle =
                new Rectangle(goalkeeperPositionX, goalkeeperPositionY, goalKeeperWidth, goalKeeperHeight);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Green);

            var rectangle = new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
            _spriteBatch.Begin();
            _spriteBatch.Draw(_backgroundTexture, rectangle, Color.White);
            _spriteBatch.Draw(_ballTexture, _ballRectangle, Color.White);
            _spriteBatch.Draw(_goalkeeperTexture, _goalkeeperRectangle, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private bool IsGoalLanded()
        {
            bool isGoal = false;
            if (_ballPosition.Y < _goalLine)
            {
                _resetEvent.Set();
                isGoal = (_ballPosition.X > _leftGate) && (_ballPosition.X < _rightGate);
                using (StreamWriter sw = File.AppendText("goal.txt"))
                {
                    sw.WriteLine("Coordinates velocities (x, y): [{0}, {1}] is goal: {2}", _coordinates[0],
                        _coordinates[1], isGoal);
                }

                _ballPosition = new Vector2(_initialBallPosition.X, _initialBallPosition.Y);

                BeginUpdatingVelocities();
            }

            return isGoal;
        }

        private bool IsGoalKeeperDefended()
        {
            bool isGoal = true;
            if (_goalkeeperRectangle.Intersects(_ballRectangle))
            {
                _resetEvent.Set();
                isGoal = false;
                using (StreamWriter sw = File.AppendText("goal.txt"))
                {
                    sw.WriteLine("Coordinate velocities (x, y): [{0}, {1}] is goal: {2}", _coordinates[0],
                        _coordinates[1], isGoal);
                }

                _ballPosition = new Vector2(_initialBallPosition.X, _initialBallPosition.Y);

                BeginUpdatingVelocities();
            }

            return isGoal;
        }

        //itt a kapus
        private void UpdateGoalKeeper()
        {
            _goalkeeperRectangle.X =(int)MathHelper.Lerp(_goalkeeperRectangle.X, _switchGoalKeeperSideMoving, 0.1f);
        }

        private void BeginUpdatingVelocities()
        {
            _socket.BeginAccept(new AsyncCallback(UpdateVelocities), _socket);
            lock (_lockObject)
            {
            }
        }

        private void UpdateVelocities(IAsyncResult result)
        {
           
            //Itt a kliens küld event
            lock (_lockObject)
            {
                _resetEvent.WaitOne();
                Socket listener = (Socket)result.AsyncState;
                Socket acceptedSocket = listener.EndAccept(result);

                int xCoordinate;
                int yCoordinate;

                using (NetworkStream stream = new NetworkStream(acceptedSocket, true))
                {
                    xCoordinate = stream.ReadByte();
                    yCoordinate = stream.ReadByte();
                }
                Task.Run(() =>
                {
                    GoalKeeper(new Shot() {X = xCoordinate, Y = yCoordinate, Strength = 0, ScreenWidth = _screenWidth});
                });
    
                Random rand = new Random();
                float shootDirection = 1;
                if (rand.Next(100) <= 50)
                {
                    shootDirection = -1;
                }

                double minimum = _screenWidth * 0.375;
                double maximum = _screenWidth * 0.623;
                double middle = (minimum + maximum) / 2;
                var range = (maximum - minimum);
                var oneUnitPixelrange = range / 20;
                var shotPixelRange = oneUnitPixelrange * xCoordinate * shootDirection;
                var shotPixelRangeReal = shotPixelRange + middle;

                _coordinates[0] = (float)shotPixelRangeReal;
                _coordinates[1] = (float)yCoordinate /1000;
             
            }

        }

        private void GoalKeeper(Shot shoot)
        {
            try
            {
                //init
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(IPAddress.Loopback, 8001);
                Stream GoalKeeperStream = tcpClient.GetStream();

                //send
                byte[] MessageSend = System.Text.Encoding.Default.GetBytes(JsonSerializer.Serialize(shoot));
                GoalKeeperStream.Write(MessageSend, 0, MessageSend.Length);

                //recive
                byte[] MessageRecive = new byte[1000];
                GoalKeeperStream.Read(MessageRecive, 0, MessageRecive.Length);
                var MessageReciveString =
                    System.Text.Encoding.Default.GetString(MessageRecive).Split("\0").FirstOrDefault();
                var goalkeeperRequestedPosition = JsonSerializer.Deserialize<GoalkeeperRequestedPosition>(MessageReciveString);
                 _switchGoalKeeperSideMoving = goalkeeperRequestedPosition.X;
         
                //close
                GoalKeeperStream.Close();
            }

            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }

    }


}