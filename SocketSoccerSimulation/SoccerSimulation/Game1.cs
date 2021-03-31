using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.IO;

namespace SoccerSimulation
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _backgroundTexture;
        private Texture2D _ballTexture;
        private Texture2D _goalkeeperTexture;

        private int _screenWidth;
        private int _screenHeight;
        private double _goalLinePosition;

        private int _goalkeeperPositionX;
        private int _goalkeeperPositionY;
        private int _goalKeeperWidth;
        private int _goalKeeperHeight;
        private int _switchGoalKeeperSideMoving;

        private Rectangle _ballRectangle;
        private Rectangle _goalkeeperRectangle;
        private Vector2 _ballPosition;
        private Vector2 _initialBallPosition;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            ResetWindowSize();
            Window.ClientSizeChanged += (s, e) => ResetWindowSize();
            TouchPanel.EnabledGestures = GestureType.Flick;
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Shoot the ball
            _ballPosition.X -= 0.5f;
            _ballPosition.Y -= 3;

            // Update goal keeper
            if (_goalkeeperRectangle.X <= _screenWidth * 0.375)
            {
                _switchGoalKeeperSideMoving = 1;
            }
            else if(_goalkeeperRectangle.X >= _screenWidth * 0.623)
            {
                _switchGoalKeeperSideMoving = -1;
            }

            _goalkeeperRectangle.X += 1 * _switchGoalKeeperSideMoving;

            if (_goalkeeperRectangle.Intersects(_ballRectangle))
            {
                _ballPosition = new Vector2(_initialBallPosition.X, _initialBallPosition.Y);
            }

            // Check for goal
            if (_ballPosition.Y < _goalLinePosition)
            {
                bool isGoal = (_ballPosition.X > _screenWidth * 0.375) && (_ballPosition.X < _screenWidth * 0.623);
                using (StreamWriter sw = File.AppendText("goal.txt"))
                {
                    sw.WriteLine(isGoal);
                }

                _ballPosition = new Vector2(_initialBallPosition.X, _initialBallPosition.Y);
            }

            // Update ball position
            _ballRectangle.X = (int)_ballPosition.X;
            _ballRectangle.Y = (int)_ballPosition.Y;

            base.Update(gameTime);
        }

        private void ResetWindowSize()
        {
            _screenWidth = Window.ClientBounds.Width;
            _screenHeight = Window.ClientBounds.Height;

            _initialBallPosition = new Vector2(_screenWidth / 2.0f, _screenHeight * 0.8f);
            var ballDimension = (_screenWidth > _screenHeight) ? (int)(_screenWidth * 0.02) : (int)(_screenHeight * 0.035);
            _ballPosition = new Vector2(_initialBallPosition.X, _initialBallPosition.Y);
            _goalLinePosition = _screenHeight * 0.05;
            _ballRectangle = new Rectangle((int)_initialBallPosition.X, (int)_initialBallPosition.Y,
                ballDimension, ballDimension);

            _goalkeeperPositionX = (_screenWidth - _goalKeeperWidth) / 2;
            _goalkeeperPositionY = (int)(_screenHeight * 0.12);
            _goalKeeperWidth = (int)(_screenWidth * 0.05);
            _goalKeeperHeight = (int)(_screenWidth * 0.015);

            _goalkeeperRectangle = new Rectangle(_goalkeeperPositionX, _goalkeeperPositionY,
                    _goalKeeperWidth, _goalKeeperHeight);
            _switchGoalKeeperSideMoving = -1;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Green);

            var rectangle = new Rectangle(0, 0, _screenWidth, _screenHeight);
 
            _spriteBatch.Begin();
            _spriteBatch.Draw(_backgroundTexture, rectangle, Color.White);
            _spriteBatch.Draw(_ballTexture, _ballRectangle, Color.White);
            _spriteBatch.Draw(_goalkeeperTexture, _goalkeeperRectangle, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
