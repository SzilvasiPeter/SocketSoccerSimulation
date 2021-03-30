using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SoccerSimulation
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _backgroundTexture;
        private Texture2D _ballTexture;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _backgroundTexture = Content.Load<Texture2D>("soccer_field");
            _ballTexture = Content.Load<Texture2D>("ball");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here
            GraphicsDevice.Clear(Color.Green);

            // Set the position for the background    
            var screenWidth = Window.ClientBounds.Width;
            var screenHeight = Window.ClientBounds.Height;
            var rectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            // Begin a sprite batch    
            _spriteBatch.Begin();
            // Draw the background    
            _spriteBatch.Draw(_backgroundTexture, rectangle, Color.White);
            // Draw the ball
            var initialBallPositionX = screenWidth / 2;
            var ínitialBallPositionY = (int)(screenHeight * 0.8);
            var ballDimension = (screenWidth > screenHeight) ?
                (int)(screenWidth * 0.02) :
                (int)(screenHeight * 0.035);
            var ballRectangle = new Rectangle(initialBallPositionX, ínitialBallPositionY,
                ballDimension, ballDimension);
            _spriteBatch.Draw(_ballTexture, ballRectangle, Color.White);
            // End the sprite batch    
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
