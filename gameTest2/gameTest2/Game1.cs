using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace gameTest2
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Add player texture and position
        private Texture2D _playerTexture;
        private Vector2 _playerPosition;
        private float _playerSpeed = 200f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Initialize player position to center of screen
            _playerPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a temporary texture for the player
            _playerTexture = new Texture2D(GraphicsDevice, 64, 64);
            Color[] colorData = new Color[64 * 64];
            for (int i = 0; i < colorData.Length; i++)
            {
                // Create a simple rectangle with a border
                int x = i % 64;
                int y = i / 64;
                if (x < 5 || x > 58 || y < 5 || y > 58)
                    colorData[i] = Color.Red;
                else
                    colorData[i] = Color.Blue;
            }
            _playerTexture.SetData(colorData);
            
            // Comment this out until we fix the content pipeline
            _playerTexture = Content.Load<Texture2D>("textures/hero/hero");

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Get keyboard state
            KeyboardState keyboardState = Keyboard.GetState();

            // Calculate movement
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 movement = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                movement.Y -= _playerSpeed * deltaTime;
            if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                movement.Y += _playerSpeed * deltaTime;
            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                movement.X -= _playerSpeed * deltaTime;
            if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                movement.X += _playerSpeed * deltaTime;

            // Update player position
            _playerPosition += movement;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // Draw the player sprite
            _spriteBatch.Draw(
                _playerTexture,
                _playerPosition,
                null,
                Color.White,
                0f,
                new Vector2(_playerTexture.Width / 2, _playerTexture.Height / 2), // Center origin
                Vector2.One,
                SpriteEffects.None,
                0f);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}