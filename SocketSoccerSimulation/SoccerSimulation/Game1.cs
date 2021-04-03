using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SoccerSimulation
{
    public class Game1 : Game
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _backgroundTexture;
        private Texture2D _ballTexture;
        private Texture2D _goalkeeperTexture;

        private int _switchGoalKeeperSideMoving = 1;

        private double _leftGate;
        private double _rightGate;
        private double _goalLine;

        private Rectangle _ballRectangle;
        private Rectangle _goalkeeperRectangle;
        private Vector2 _ballPosition;
        private Vector2 _initialBallPosition;

        private Socket _socket;
        private readonly object _lockObject = new object();
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private readonly float[] _coordinates = new float[2] { -1f, 5f };

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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Shoot the ball
            _ballPosition.X -= _coordinates[0];
            _ballPosition.Y -= _coordinates[1];

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

            // Init Gate value
            _leftGate = screenWidth * 0.375;
            _rightGate = screenWidth * 0.623;
            _goalLine = screenHeight * 0.05;
            
            // Init ball
            var ballDimension = (screenWidth > screenHeight) ? (int)(screenWidth * 0.02) : (int)(screenHeight * 0.035);
            _initialBallPosition = new Vector2(screenWidth / 2.0f, screenHeight * 0.8f);
            _ballPosition = new Vector2(_initialBallPosition.X, _initialBallPosition.Y);
            _ballRectangle = new Rectangle((int)_initialBallPosition.X, (int)_initialBallPosition.Y, ballDimension, ballDimension);

            // Init Goal keeper
            int goalKeeperWidth = (int)_goalLine;
            int goalKeeperHeight = (int)(screenWidth * 0.015);
            int goalkeeperPositionX = (screenWidth - goalKeeperWidth) / 2;
            int goalkeeperPositionY = (int)(screenHeight * 0.12);
            _goalkeeperRectangle = new Rectangle(goalkeeperPositionX, goalkeeperPositionY,goalKeeperWidth, goalKeeperHeight);
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
                    sw.WriteLine("Coordinates velocities (x, y): [{0}, {1}] is goal: {2}", _coordinates[0], _coordinates[1], isGoal);
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
                    sw.WriteLine("Coordinate velocities (x, y): [{0}, {1}] is goal: {2}", _coordinates[0], _coordinates[1], isGoal);
                }

                _ballPosition = new Vector2(_initialBallPosition.X, _initialBallPosition.Y);

                BeginUpdatingVelocities();
            }

            return isGoal;
        }

        private void UpdateGoalKeeper()
        {
            if (_goalkeeperRectangle.X <= _leftGate)
            {
                _switchGoalKeeperSideMoving = 1;
            }
            else if (_goalkeeperRectangle.X >= _rightGate)
            {
                _switchGoalKeeperSideMoving = -1;
            }

            _goalkeeperRectangle.X += 1 * _switchGoalKeeperSideMoving;
        }

        private void BeginUpdatingVelocities()
        {
            _socket.BeginAccept(new AsyncCallback(UpdateVelocities), _socket);
            lock (_lockObject) { }
        }

        private void UpdateVelocities(IAsyncResult result)
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

            int[] newVelocities = new int[] { xCoordinate, yCoordinate };

            float normalizedXVelocity = newVelocities[0] / 10;
            float normalizedYVelocity = newVelocities[1] / 10;

            Random rand = new Random();
            float shootDirection = 1;
            if (rand.Next(1) <= 0.5)
            {
                shootDirection = -1;
            }

            _coordinates[0] = normalizedXVelocity * shootDirection;
            _coordinates[1] = normalizedYVelocity;
        }
    }
}
