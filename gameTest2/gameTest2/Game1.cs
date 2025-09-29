using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace gameTest2
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
       

        // Background animation
        private readonly List<Texture2D> _bgFrames = new();
        private int _bgFrameIndex = 0;
        private float _bgFrameTimer = 0f;
        private const float BgFrameDuration = 0.03f;
        private Texture2D _bgFallback;

        // Player
        private Texture2D _playerTexture;
        private Vector2 _playerPosition;
        private float _playerSpeed = 300f;
        private Vector2 _facing = new(0, -1);

        // Shooting cooldown
        private const float ShootCooldownSeconds = 0.4f;
        private float _shootCooldownTimer = 0f;

        // Lasers
        private Texture2D _laserTexture;
        private readonly List<Laser> _lasers = new();
        private float _laserSpeedMultiplier = 3f;
        private const float EnemyLaserSpeedFactor = 0.85f;

        // Enemies
        private Texture2D[] _enemyTextures = new Texture2D[12];
        private readonly List<Enemy> _enemies = new();
        private float _enemySpawnTimer = 0f;
        private bool _spawnedEnemyThisUpdate = false;

        // Asteroids
        private Texture2D[] _asteroidTextures = new Texture2D[2];
        private readonly List<Asteroid> _asteroids = new();
        private float _asteroidSpawnTimer = 0f;
        private const float AsteroidSpawnBaseInterval = 12f;
        private const float AsteroidSpawnMinInterval = 4f;
        private const float AsteroidSpawnDifficultyFactor = 0.00025f;
        private const int AsteroidMaxHp = 2;
        private const int AsteroidCollisionDamage = 3;

        // Boss
        private Texture2D _bossTexture;
        private bool _bossActive = false;
        private Vector2 _bossPosition;
        private float _bossHp = 10;  // Reduced from 30 to 10 HP
        private const float BossMaxHp = 10f;  // Reduced from 30 to 10 HP
        private const float BossSpeed = 120f;  // Increased from 80 to 120 for speed boost
        private float _bossHorizontalSpeed = 180f;  // Increased from 120 to 180 for speed boost
        private float _bossHorizontalDir = 1f;  // 1 for right, -1 for left
        private float _bossMovementTimer = 0f;
        private const float BossMovementChangeInterval = 1.2f; // Reduced from 1.5 to 1.2 for faster direction changes
        private bool _bossSpawned = false; // Track if boss has been spawned for this threshold

        private readonly Random _random = new();
        private float _totalPlayTime = 0f;

        // Score & difficulty scaling - RESTORED SCORE SYSTEM
        private int _enemiesDestroyed = 0;  // Keep for boss spawn condition
        private double _score = 0.0;  // Restored score system
        private int _scoreInt = 0;  // Display score as integer
        private SpriteFont _scoreFont;

        // Restored score constants
        private const double ScorePerSecond = 100.0;
        private const double EnemyKillScore = 300.0;
        
        // Keep these for difficulty scaling but base on score again:
        private const float BaseEnemySpawnInterval = 5f;
        private const float MinEnemySpawnInterval = 1f;
        private const int ScoreDifficultyStep = 3000;  // Every 3000 points, increase difficulty
        private const float ScoreIntervalReductionPerStep = 0.2f;

        // Enemy shooting difficulty
        private const float EnemyBaseShotMin = 2.0f;
        private const float EnemyBaseShotMax = 5.2f;
        private const float EnemyShotDifficultyFactor = 0.025f;  // Increased since we're using smaller numbers

        // Enemy horizontal movement
        private const float EnemyHorizontalSpeedMin = 10f;
        private const float EnemyHorizontalSpeedMax = 60f;
        private const float EnemyHorizontalIntervalMin = 0.8f;
        private const float EnemyHorizontalIntervalMax = 3.0f;
        private const float EnemyHorizontalEdgeMargin = 24f;

        // Player state
        private int _playerHp = 5;
        private bool _isPlayerDead = false;
        private const int EnemyCollisionDamage = 2;

        // Input
        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;

        // UI / Menu
        private enum GameState { MainMenu, UsernameInput, Playing, GameOver, Leaderboard }
        private GameState _state = GameState.MainMenu;
        private Texture2D _uiPixel;

        // Username input
        private string _playerName = "";
        private bool _showCursor = true;
        private float _cursorBlinkTimer = 0f;
        private const float CursorBlinkInterval = 0.5f;
        private const int MaxUsernameLength = 15;

        // Database
        private ScoreDatabase _scoreDatabase;

        // Leaderboard
        private List<ScoreEntry> _topScores = new();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            
            // Set fullscreen mode
            _graphics.IsFullScreen = true;
            
            // Optional: Set preferred resolution for fullscreen (uses desktop resolution by default)
            // _graphics.PreferredBackBufferWidth = 1920;
            // _graphics.PreferredBackBufferHeight = 1080;
            
            // If you want to use desktop resolution, get it from GraphicsAdapter
            var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            _graphics.PreferredBackBufferWidth = displayMode.Width;
            _graphics.PreferredBackBufferHeight = displayMode.Height;
            
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _playerPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2f,
                _graphics.PreferredBackBufferHeight / 2f);

            _previousMouseState = Mouse.GetState();
            _previousKeyboardState = Keyboard.GetState();
            
            _scoreDatabase = new ScoreDatabase();
            LoadTopScores();
            
            base.Initialize();
        }

        private void LoadTopScores()
        {
            _topScores = _scoreDatabase.GetTopScores(10);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            LoadBackgroundFrames();

            // UI pixel (1x1) pour rectangles/boutons
            _uiPixel = new Texture2D(GraphicsDevice, 1, 1);
            _uiPixel.SetData(new[] { XnaColor.White });

            // Player temp texture
            _playerTexture = new Texture2D(GraphicsDevice, 64, 64);
            var colorData = new XnaColor[64 * 64];
            for (int i = 0; i < colorData.Length; i++)
            {
                int x = i % 64;
                int y = i / 64;
                colorData[i] = (x < 5 || x > 58 || y < 5 || y > 58)
                    ? XnaColor.Red
                    : XnaColor.Blue;
            }
            _playerTexture.SetData(colorData);
            try { _playerTexture = Content.Load<Texture2D>("textures/hero/hero"); } catch { }

            // Laser
            try
            {
                _laserTexture = Content.Load<Texture2D>("textures/lasers/laserViolet");
            }
            catch
            {
                int lw = 6, lh = 18;
                _laserTexture = new Texture2D(GraphicsDevice, lw, lh);
                var arr = Enumerable.Repeat(XnaColor.FromNonPremultiplied(148, 0, 211, 255), lw * lh).ToArray();
                _laserTexture.SetData(arr);
            }

            // Enemies
            for (int i = 0; i < 12; i++)
            {
                string path = $"textures/ennemis/ennemi{i + 1}";
                try
                {
                    _enemyTextures[i] = Content.Load<Texture2D>(path);
                }
                catch
                {
                    int size = 32;
                    var tex = new Texture2D(GraphicsDevice, size, size);
                    var pixels = Enumerable.Repeat(
                        (i % 3) switch
                        {
                            0 => XnaColor.DarkRed,
                            1 => XnaColor.DarkGreen,
                            _ => XnaColor.DarkSlateBlue
                        }, size * size).ToArray();
                    tex.SetData(pixels);
                    _enemyTextures[i] = tex;
                }
            }

            // Asteroids
            for (int i = 0; i < 2; i++)
            {
                string path = $"textures/asteroide/asteroide{i + 1}";
                try
                {
                    _asteroidTextures[i] = Content.Load<Texture2D>(path);
                }
                catch
                {
                    int size = 48;
                    var tex = new Texture2D(GraphicsDevice, size, size);
                    var pixels = new XnaColor[size * size];
                    for (int p = 0; p < pixels.Length; p++)
                    {
                        int x = p % size;
                        int y = p / size;
                        float dx = x - size / 2f;
                        float dy = y - size / 2f;
                        float d = MathF.Sqrt(dx * dx + dy * dy);
                        pixels[p] = d < size * 0.45f ? XnaColor.Gray : XnaColor.Transparent;
                    }
                    tex.SetData(pixels);
                    _asteroidTextures[i] = tex;
                }
            }

            try { _scoreFont = Content.Load<SpriteFont>("fonts/score"); } catch { _scoreFont = null; }

            // Boss - with fallback texture creation
            try 
            { 
                _bossTexture = Content.Load<Texture2D>("textures/boss/boss3"); // Changed from "textures/boss/boss" to "textures/boss/boss3"
            } 
            catch 
            {
                // Create fallback boss texture if loading fails
                int bossSize = 96; // Larger than regular enemies
                _bossTexture = new Texture2D(GraphicsDevice, bossSize, bossSize);
                var bossPixels = new XnaColor[bossSize * bossSize];
                for (int p = 0; p < bossPixels.Length; p++)
                {
                    int x = p % bossSize;
                    int y = p / bossSize;
                    
                    // Create a distinctive boss pattern (red with black border)
                    if (x < 4 || x > bossSize - 5 || y < 4 || y > bossSize - 5)
                        bossPixels[p] = XnaColor.Black;
                    else if (x < 8 || x > bossSize - 9 || y < 8 || y > bossSize - 9)
                        bossPixels[p] = XnaColor.DarkRed;
                    else
                        bossPixels[p] = XnaColor.Red;
                }
                _bossTexture.SetData(bossPixels);
            }
        }

        private void LoadBackgroundFrames()
        {
            for (int i = 0; i <= 32; i++)
            {
                string name = $"textures/background/frame{i:00}";
                try { _bgFrames.Add(Content.Load<Texture2D>(name)); } catch { }
            }
            if (_bgFrames.Count == 0)
            {
                _bgFallback = new Texture2D(GraphicsDevice, 1, 1);
                _bgFallback.SetData(new[] { XnaColor.Black });
            }
        }

        private void ToggleFullscreen()
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            
            if (_graphics.IsFullScreen)
            {
                // Switch to fullscreen - use desktop resolution
                var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                _graphics.PreferredBackBufferWidth = displayMode.Width;
                _graphics.PreferredBackBufferHeight = displayMode.Height;
            }
            else
            {
                // Switch to windowed mode - use smaller resolution
                _graphics.PreferredBackBufferWidth = 1000;
                _graphics.PreferredBackBufferHeight = 900;
            }
            
            _graphics.ApplyChanges();
        }

        protected override void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Global fullscreen toggle (Alt+Enter or F11)
            if ((keyboard.IsKeyDown(Keys.LeftAlt) && keyboard.IsKeyDown(Keys.Enter) && 
                 !(_previousKeyboardState.IsKeyDown(Keys.LeftAlt) && _previousKeyboardState.IsKeyDown(Keys.Enter))) ||
                (keyboard.IsKeyDown(Keys.F11) && !_previousKeyboardState.IsKeyDown(Keys.F11)))
            {
                ToggleFullscreen();
            }

            // Background animation for all states
            if (_bgFrames.Count > 1)
            {
                _bgFrameTimer += dt;
                while (_bgFrameTimer >= BgFrameDuration)
                {
                    _bgFrameTimer -= BgFrameDuration;
                    _bgFrameIndex = (_bgFrameIndex + 1) % _bgFrames.Count;
                }
            }

            switch (_state)
            {
                case GameState.MainMenu:
                    UpdateMainMenu(keyboard, mouse);
                    break;
                case GameState.UsernameInput:
                    UpdateUsernameInput(keyboard, mouse, dt);
                    break;
                case GameState.Playing:
                    UpdatePlaying(keyboard, mouse, dt);
                    break;
                case GameState.GameOver:
                    UpdateGameOver(keyboard, mouse);
                    break;
                case GameState.Leaderboard:
                    UpdateLeaderboard(keyboard, mouse);
                    break;
            }

            _previousMouseState = mouse;
            _previousKeyboardState = keyboard;
            base.Update(gameTime);
        }

        private void UpdateMainMenu(KeyboardState keyboard, MouseState mouse)
        {
            // Remove Escape key handling from main menu - it should do nothing
            // Only keep gamepad back button check for Xbox controller compatibility
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            GetMenuLayout(out var startRect, out var leaderboardRect, out var quitRect);
            bool click = mouse.LeftButton == ButtonState.Pressed &&
                         _previousMouseState.LeftButton == ButtonState.Released;

            if (keyboard.IsKeyDown(Keys.Enter) || keyboard.IsKeyDown(Keys.Space) ||
                (click && startRect.Contains(mouse.Position)))
            {
                _state = GameState.UsernameInput;
                _playerName = "";
            }
            else if (click && leaderboardRect.Contains(mouse.Position) ||
                     (keyboard.IsKeyDown(Keys.L) && !_previousKeyboardState.IsKeyDown(Keys.L)))
            {
                LoadTopScores();
                _state = GameState.Leaderboard;
            }
            else if (click && quitRect.Contains(mouse.Position))
            {
                Exit();
            }
        }

        private void UpdateUsernameInput(KeyboardState keyboard, MouseState mouse, float dt)
        {
            // Cursor blinking
            _cursorBlinkTimer += dt;
            if (_cursorBlinkTimer >= CursorBlinkInterval)
            {
                _cursorBlinkTimer -= CursorBlinkInterval;
                _showCursor = !_showCursor;
            }

            // Handle text input
            var pressedKeys = keyboard.GetPressedKeys();
            foreach (var key in pressedKeys)
            {
                if (!_previousKeyboardState.IsKeyDown(key))
                {
                    HandleTextInput(key);
                }
            }

            // Start game when Enter is pressed (if username is not empty)
            if (keyboard.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter))
            {
                if (!string.IsNullOrWhiteSpace(_playerName))
                {
                    _state = GameState.Playing;
                    ResetGame();
                }
            }

            // Back to main menu
            if (keyboard.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
            {
                _state = GameState.MainMenu;
            }
        }

        private void UpdateLeaderboard(KeyboardState keyboard, MouseState mouse)
        {
            if (keyboard.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape) ||
                keyboard.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter) ||
                keyboard.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space) ||
                (mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released))
            {
                _state = GameState.MainMenu;
            }
        }

        private void HandleTextInput(Keys key)
        {
            if (_playerName.Length >= MaxUsernameLength) return;

            // Handle letters
            if (key >= Keys.A && key <= Keys.Z)
            {
                char letter = (char)('a' + (key - Keys.A));
                _playerName += letter;
            }
            // Handle numbers
            else if (key >= Keys.D0 && key <= Keys.D9)
            {
                char number = (char)('0' + (key - Keys.D0));
                _playerName += number;
            }
            // Handle space
            else if (key == Keys.Space && _playerName.Length > 0 && !_playerName.EndsWith(" "))
            {
                _playerName += " ";
            }
            // Handle backspace
            else if (key == Keys.Back && _playerName.Length > 0)
            {
                _playerName = _playerName[..^1];
            }
        }

        private void UpdatePlaying(KeyboardState keyboard, MouseState mouse, float dt)
        {
            if (_isPlayerDead)
            {
                _state = GameState.GameOver;
                return;
            }

            // Change Escape behavior to go back to main menu instead of exiting
            if (keyboard.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
            {
                _state = GameState.MainMenu;
                return;
            }

            // Keep gamepad back button for Xbox controller compatibility
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                _state = GameState.MainMenu;
                return;
            }

            _spawnedEnemyThisUpdate = false;

            // Restored time-based scoring
            _totalPlayTime += dt;
            _score += ScorePerSecond * dt;
            _scoreInt = (int)Math.Round(_score);

            if (_shootCooldownTimer > 0f) _shootCooldownTimer -= dt;

            // Player movement
            Vector2 mv = Vector2.Zero;
            if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)) mv.Y -= 1;
            if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) mv.Y += 1;
            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) mv.X -= 1;
            if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) mv.X += 1;
            if (mv.LengthSquared() > 0)
            {
                mv = Vector2.Normalize(mv) * _playerSpeed * dt;
                _facing = Vector2.Normalize(mv);
            }
            _playerPosition += mv;

            // Keep player within screen bounds
            _playerPosition.X = MathHelper.Clamp(_playerPosition.X, _playerTexture.Width / 2f, _graphics.PreferredBackBufferWidth - _playerTexture.Width / 2f);
            _playerPosition.Y = MathHelper.Clamp(_playerPosition.Y, _playerTexture.Height / 2f, _graphics.PreferredBackBufferHeight - _playerTexture.Height / 2f);

            // Shooting
            if (mouse.LeftButton == ButtonState.Pressed &&
                _previousMouseState.LeftButton == ButtonState.Released &&
                _shootCooldownTimer <= 0f)
            {
                SpawnLaserTowards(new Vector2(mouse.X, mouse.Y), false);
                _shootCooldownTimer = ShootCooldownSeconds;
            }

            // Boss logic: spawn if exactly 10 enemies destroyed and boss hasn't been spawned yet
            if (!_bossActive && !_bossSpawned && _enemiesDestroyed >= 10)
            {
                _bossActive = true;
                _bossSpawned = true;
                _bossPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2f, -100);
                _bossHp = BossMaxHp;
                _bossHorizontalDir = _random.Next(0, 2) == 0 ? -1f : 1f; // Random initial direction
                _bossMovementTimer = 0f;
            }

            if (_bossActive)
            {
                UpdateBoss(dt);
            }

            // Only spawn enemies and asteroids if boss is not active
            if (!_bossActive)
            {
                // Enemy spawn - back to score-based difficulty scaling
                int steps = _scoreInt / ScoreDifficultyStep;
                float enemyInterval = BaseEnemySpawnInterval - steps * ScoreIntervalReductionPerStep;
                if (enemyInterval < MinEnemySpawnInterval) enemyInterval = MinEnemySpawnInterval;
                _enemySpawnTimer += dt;
                if (_enemySpawnTimer >= enemyInterval && !_spawnedEnemyThisUpdate)
                {
                    _enemySpawnTimer -= enemyInterval;
                    SpawnEnemy();
                    _spawnedEnemyThisUpdate = true;
                }

                // Asteroid spawn - back to score-based calculation
                float asteroidInterval = AsteroidSpawnBaseInterval - (_scoreInt * AsteroidSpawnDifficultyFactor);
                if (asteroidInterval < AsteroidSpawnMinInterval) asteroidInterval = AsteroidSpawnMinInterval;
                _asteroidSpawnTimer += dt;
                if (_asteroidSpawnTimer >= asteroidInterval)
                {
                    _asteroidSpawnTimer -= asteroidInterval;
                    SpawnAsteroid();
                }
            }

            UpdateEnemies(dt);
            UpdateAsteroids(dt);
            UpdateLasers(dt);
        }

        private void UpdateGameOver(KeyboardState keyboard, MouseState mouse)
        {
            if (keyboard.IsKeyDown(Keys.Enter) || keyboard.IsKeyDown(Keys.Space) ||
                (mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released))
            {
                LoadTopScores(); // Refresh leaderboard after game over
                _state = GameState.MainMenu;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(XnaColor.Black);
            _spriteBatch.Begin();

            // Background
            if (_bgFrames.Count > 0)
            {
                var dest = new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
                _spriteBatch.Draw(_bgFrames[_bgFrameIndex], dest, XnaColor.White);
            }
            else if (_bgFallback != null)
            {
                _spriteBatch.Draw(_bgFallback, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), XnaColor.Black);
            }

            switch (_state)
            {
                case GameState.MainMenu:
                    DrawMainMenu();
                    break;
                case GameState.UsernameInput:
                    DrawUsernameInput();
                    break;
                case GameState.Playing:
                    DrawPlaying();
                    break;
                case GameState.GameOver:
                    DrawGameOver();
                    break;
                case GameState.Leaderboard:
                    DrawLeaderboard();
                    break;
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawMainMenu()
        {
            // Assombrir le fond
            var overlay = new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _spriteBatch.Draw(_uiPixel, overlay, new XnaColor(0, 0, 0, 160));

            // Boutons
            GetMenuLayout(out var startRect, out var leaderboardRect, out var quitRect);
            var mouse = Mouse.GetState();
            bool hoverStart = startRect.Contains(mouse.Position);
            bool hoverLeaderboard = leaderboardRect.Contains(mouse.Position);
            bool hoverQuit = quitRect.Contains(mouse.Position);

            var startColor = hoverStart ? XnaColor.Lerp(XnaColor.DodgerBlue, XnaColor.White, 0.25f) : XnaColor.DodgerBlue;
            var leaderboardColor = hoverLeaderboard ? XnaColor.Lerp(XnaColor.Orange, XnaColor.White, 0.25f) : XnaColor.Orange;
            var quitColor = hoverQuit ? XnaColor.Lerp(XnaColor.DimGray, XnaColor.White, 0.25f) : XnaColor.DimGray;

            _spriteBatch.Draw(_uiPixel, startRect, startColor);
            _spriteBatch.Draw(_uiPixel, leaderboardRect, leaderboardColor);
            _spriteBatch.Draw(_uiPixel, quitRect, quitColor);

            // Titres et libellés
            if (_scoreFont != null)
            {
                string title = "Menu Principal";
                var tSize = _scoreFont.MeasureString(title);
                var tPos = new Vector2((_graphics.PreferredBackBufferWidth - tSize.X) / 2f, _graphics.PreferredBackBufferHeight * 0.15f);
                _spriteBatch.DrawString(_scoreFont, title, tPos, XnaColor.White);

                string startText = "Démarrer";
                string leaderboardText = "Classement";
                string quitText = "Quitter";

                var sSize = _scoreFont.MeasureString(startText);
                var lSize = _scoreFont.MeasureString(leaderboardText);
                var qSize = _scoreFont.MeasureString(quitText);

                var sPos = new Vector2(startRect.X + (startRect.Width - sSize.X) / 2f, startRect.Y + (startRect.Height - sSize.Y) / 2f);
                var lPos = new Vector2(leaderboardRect.X + (leaderboardRect.Width - lSize.X) / 2f, leaderboardRect.Y + (leaderboardRect.Height - lSize.Y) / 2f);
                var qPos = new Vector2(quitRect.X + (quitRect.Width - qSize.X) / 2f, quitRect.Y + (quitRect.Height - qSize.Y) / 2f);

                _spriteBatch.DrawString(_scoreFont, startText, sPos, XnaColor.White);
                _spriteBatch.DrawString(_scoreFont, leaderboardText, lPos, XnaColor.White);
                _spriteBatch.DrawString(_scoreFont, quitText, qPos, XnaColor.White);

                string hint = "Entrée/Espace: Démarrer | L: Classement | F11/Alt+Enter: Plein écran";
                var hSize = _scoreFont.MeasureString(hint);
                var hPos = new Vector2((_graphics.PreferredBackBufferWidth - hSize.X) / 2f, quitRect.Bottom + 24);
                _spriteBatch.DrawString(_scoreFont, hint, hPos, XnaColor.LightGray);
            }
        }

        private void DrawLeaderboard()
        {
            // Assombrir le fond
            var overlay = new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _spriteBatch.Draw(_uiPixel, overlay, new XnaColor(0, 0, 0, 180));

            if (_scoreFont != null)
            {
                // Titre
                string title = "Meilleurs Scores";
                var titleSize = _scoreFont.MeasureString(title);
                var titlePos = new Vector2((_graphics.PreferredBackBufferWidth - titleSize.X) / 2f, _graphics.PreferredBackBufferHeight * 0.15f);
                _spriteBatch.DrawString(_scoreFont, title, titlePos, XnaColor.White);

                // Headers - RESTORED SCORE HEADER
                float startY = titlePos.Y + titleSize.Y + 40f;
                string rankHeader = "Rang";
                string nameHeader = "Nom";
                string scoreHeader = "Score";  // Changed back to "Score"
                string dateHeader = "Date";

                float rankX = _graphics.PreferredBackBufferWidth * 0.15f;
                float nameX = _graphics.PreferredBackBufferWidth * 0.3f;
                float scoreX = _graphics.PreferredBackBufferWidth * 0.55f;
                float dateX = _graphics.PreferredBackBufferWidth * 0.75f;

                _spriteBatch.DrawString(_scoreFont, rankHeader, new Vector2(rankX, startY), XnaColor.Yellow);
                _spriteBatch.DrawString(_scoreFont, nameHeader, new Vector2(nameX, startY), XnaColor.Yellow);
                _spriteBatch.DrawString(_scoreFont, scoreHeader, new Vector2(scoreX, startY), XnaColor.Yellow);
                _spriteBatch.DrawString(_scoreFont, dateHeader, new Vector2(dateX, startY), XnaColor.Yellow);

                // Line separator
                float lineY = startY + 30f;
                var lineRect = new Rectangle((int)rankX, (int)lineY, (int)(dateX + 100 - rankX), 2);
                _spriteBatch.Draw(_uiPixel, lineRect, XnaColor.Gray);

                // Scores
                float currentY = lineY + 20f;
                for (int i = 0; i < Math.Min(_topScores.Count, 10); i++)
                {
                    var score = _topScores[i];
                    
                    // Alternate row colors
                    var rowColor = i % 2 == 0 ? XnaColor.White : XnaColor.LightGray;
                    
                    // Highlight top 3
                    if (i == 0) rowColor = XnaColor.Gold;
                    else if (i == 1) rowColor = XnaColor.Silver;
                    else if (i == 2) rowColor = XnaColor.FromNonPremultiplied(205, 127, 50, 255); // Bronze

                    string rank = $"{i + 1}.";
                    string name = score.PlayerName.Length > 12 ? score.PlayerName[..12] + "..." : score.PlayerName;
                    string scoreText = $"{score.Score:N0}";  // Formatted score display
                    string date = score.DateAchieved.ToString("dd/MM/yy");

                    _spriteBatch.DrawString(_scoreFont, rank, new Vector2(rankX, currentY), rowColor);
                    _spriteBatch.DrawString(_scoreFont, name, new Vector2(nameX, currentY), rowColor);
                    _spriteBatch.DrawString(_scoreFont, scoreText, new Vector2(scoreX, currentY), rowColor);
                    _spriteBatch.DrawString(_scoreFont, date, new Vector2(dateX, currentY), rowColor);

                    currentY += 35f;
                }

                // Empty leaderboard message
                if (_topScores.Count == 0)
                {
                    string emptyMessage = "Aucun score enregistré";
                    var emptySize = _scoreFont.MeasureString(emptyMessage);
                    var emptyPos = new Vector2((_graphics.PreferredBackBufferWidth - emptySize.X) / 2f, currentY + 40f);
                    _spriteBatch.DrawString(_scoreFont, emptyMessage, emptyPos, XnaColor.Gray);
                }

                // Instructions
                string instruction = "Appuyez sur n'importe quelle touche pour retourner au menu";
                var instrSize = _scoreFont.MeasureString(instruction);
                var instrPos = new Vector2((_graphics.PreferredBackBufferWidth - instrSize.X) / 2f, _graphics.PreferredBackBufferHeight * 0.85f);
                _spriteBatch.DrawString(_scoreFont, instruction, instrPos, XnaColor.LightGray);
            }
        }

        private void DrawUsernameInput()
        {
            // Assombrir le fond
            var overlay = new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _spriteBatch.Draw(_uiPixel, overlay, new XnaColor(0, 0, 0, 180));

            if (_scoreFont != null)
            {
                // Titre
                string title = "Entrez votre nom";
                var titleSize = _scoreFont.MeasureString(title);
                var titlePos = new Vector2((_graphics.PreferredBackBufferWidth - titleSize.X) / 2f, _graphics.PreferredBackBufferHeight * 0.3f);
                _spriteBatch.DrawString(_scoreFont, title, titlePos, XnaColor.White);

                // Champ de saisie
                int inputWidth = 400;
                int inputHeight = 50;
                var inputRect = new Rectangle(
                    (_graphics.PreferredBackBufferWidth - inputWidth) / 2,
                    (int)(titlePos.Y + titleSize.Y + 40),
                    inputWidth,
                    inputHeight);

                _spriteBatch.Draw(_uiPixel, inputRect, XnaColor.White);
                _spriteBatch.Draw(_uiPixel, new Rectangle(inputRect.X + 2, inputRect.Y + 2, inputRect.Width - 4, inputRect.Height - 4), XnaColor.Black);

                // Texte saisi
                string displayText = _playerName + (_showCursor ? "|" : "");
                var textSize = _scoreFont.MeasureString(displayText);
                var textPos = new Vector2(inputRect.X + 10, inputRect.Y + (inputRect.Height - textSize.Y) / 2f);
                _spriteBatch.DrawString(_scoreFont, displayText, textPos, XnaColor.White);

                // Instructions
                string instruction = "Tapez votre nom et appuyez sur Entrée pour commencer";
                var instrSize = _scoreFont.MeasureString(instruction);
                var instrPos = new Vector2((_graphics.PreferredBackBufferWidth - instrSize.X) / 2f, inputRect.Bottom + 30);
                _spriteBatch.DrawString(_scoreFont, instruction, instrPos, XnaColor.LightGray);

                string backInstruction = "Échap pour retourner au menu";
                var backSize = _scoreFont.MeasureString(backInstruction);
                var backPos = new Vector2((_graphics.PreferredBackBufferWidth - backSize.X) / 2f, instrPos.Y + instrSize.Y + 10);
                _spriteBatch.DrawString(_scoreFont, backInstruction, backPos, XnaColor.Gray);
            }
        }

        private void DrawPlaying()
        {
            // Asteroids (blink visible: ignorer tous ceux avec Visible = false)
            foreach (var a in _asteroids)
            {
                if (!a.Visible) continue;
                var tex = a.Texture;
                _spriteBatch.Draw(tex, a.Position, null, XnaColor.White, 0f,
                    new Vector2(tex.Width / 2f, tex.Height / 2f), 1f, SpriteEffects.None, 0f);
            }

            // Enemies
            foreach (var en in _enemies)
            {
                if (!en.Visible) continue;
                var tex = _enemyTextures[en.TextureIndex];
                _spriteBatch.Draw(tex, en.Position, null, XnaColor.White, 0f,
                    new Vector2(tex.Width / 2f, tex.Height / 2f), 1f, SpriteEffects.None, 0f);
            }

            // Lasers
            foreach (var l in _lasers)
            {
                float rotation = (float)Math.Atan2(l.Velocity.Y, l.Velocity.X) + MathHelper.PiOver2;
                var tint = l.IsEnemy ? XnaColor.OrangeRed : XnaColor.White;
                _spriteBatch.Draw(_laserTexture, l.Position, null, tint, rotation,
                    new Vector2(_laserTexture.Width / 2f, _laserTexture.Height / 2f), 1f,
                    SpriteEffects.None, 0f);
            }

            // Player
            _spriteBatch.Draw(_playerTexture, _playerPosition, null, XnaColor.White, 0f,
                new Vector2(_playerTexture.Width / 2f, _playerTexture.Height / 2f), 1f,
                SpriteEffects.None, 0f);

            // Boss - with proper null check
            if (_bossActive && _bossTexture != null)
            {
                _spriteBatch.Draw(_bossTexture, _bossPosition, null, XnaColor.White, 0f,
                    new Vector2(_bossTexture.Width / 2f, _bossTexture.Height / 2f), 1f,
                    SpriteEffects.None, 0f);
            }

            // Score + HP + Boss HP (if active) - RESTORED SCORE DISPLAY
            if (_scoreFont != null)
            {
                string text = $"SCORE: {_scoreInt}  HP: {_playerHp}  Joueur: {_playerName}";
                if (_bossActive)
                {
                    text += $"  BOSS HP: {_bossHp}/{BossMaxHp}";
                }
                var size = _scoreFont.MeasureString(text);
                var pos = new Vector2(_graphics.PreferredBackBufferWidth - 10 - size.X, 10);
                _spriteBatch.DrawString(_scoreFont, text, pos, XnaColor.White);
            }
        }

        private void DrawGameOver()
        {
            DrawPlaying(); // Draw the game state behind

            var overlay = new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _spriteBatch.Draw(_uiPixel, overlay, new XnaColor(0, 0, 0, 160));
            
            if (_scoreFont != null)
            {
                string over = "Game Over";
                string scoreText = $"Ennemis Détruits: {_enemiesDestroyed}";  // Changed from final score
                string playerText = $"Joueur: {_playerName}";
                string savedText = "Score sauvegardé!";
                string back = "Entrée/Espace ou clic pour retourner au menu";
                
                var oSize = _scoreFont.MeasureString(over);
                var sSize = _scoreFont.MeasureString(scoreText);
                var pSize = _scoreFont.MeasureString(playerText);
                var savedSize = _scoreFont.MeasureString(savedText);
                var bSize = _scoreFont.MeasureString(back);
                
                float startY = _graphics.PreferredBackBufferHeight * 0.3f;
                
                var oPos = new Vector2((_graphics.PreferredBackBufferWidth - oSize.X) / 2f, startY);
                var sPos = new Vector2((_graphics.PreferredBackBufferWidth - sSize.X) / 2f, oPos.Y + oSize.Y + 20f);
                var pPos = new Vector2((_graphics.PreferredBackBufferWidth - pSize.X) / 2f, sPos.Y + sSize.Y + 10f);
                var savedPos = new Vector2((_graphics.PreferredBackBufferWidth - savedSize.X) / 2f, pPos.Y + pSize.Y + 10f);
                var bPos = new Vector2((_graphics.PreferredBackBufferWidth - bSize.X) / 2f, savedPos.Y + savedSize.Y + 30f);
                
                _spriteBatch.DrawString(_scoreFont, over, oPos, XnaColor.White);
                _spriteBatch.DrawString(_scoreFont, scoreText, sPos, XnaColor.Yellow);
                _spriteBatch.DrawString(_scoreFont, playerText, pPos, XnaColor.Cyan);
                _spriteBatch.DrawString(_scoreFont, savedText, savedPos, XnaColor.LightGreen);
                _spriteBatch.DrawString(_scoreFont, back, bPos, XnaColor.LightGray);
            }
        }

        private void UpdateEnemies(float dt)
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var en = _enemies[i];
                en.Position.Y += en.Speed * dt;

                if (!en.IsDying)
                {
                    en.HorizontalSwitchTimer += dt;
                    if (en.HorizontalSwitchTimer >= en.HorizontalSwitchInterval)
                    {
                        en.HorizontalSwitchTimer -= en.HorizontalSwitchInterval;
                        en.HorizontalDir = -en.HorizontalDir;
                    }

                    float horiz = en.HorizontalSpeed;
                    float scale = 1f + MathF.Min(_scoreInt / 9000f, 1.5f);  // Scale based on score again
                    en.Position.X += en.HorizontalDir * horiz * scale * dt;

                    float left = EnemyHorizontalEdgeMargin;
                    float right = _graphics.PreferredBackBufferWidth - EnemyHorizontalEdgeMargin;
                    if (en.Position.X < left) { en.Position.X = left; en.HorizontalDir = 1; en.HorizontalSwitchTimer = 0f; }
                    else if (en.Position.X > right) { en.Position.X = right; en.HorizontalDir = -1; en.HorizontalSwitchTimer = 0f; }
                }

                if (!en.IsDying && CheckEnemyHitsPlayer(en))
                {
                    en.IsDying = true; en.Visible = false;
                    en.BlinkTimer = 0f; en.BlinkInterval = 0.15f; en.BlinkCount = 0; en.BlinkToggleTarget = 4;
                    _playerHp -= EnemyCollisionDamage;
                    if (_playerHp <= 0) 
                    {
                        _isPlayerDead = true;
                        _scoreDatabase.SaveScore(_playerName, _scoreInt);  // Save final score
                    }
                }

                if (en.IsDying)
                {
                    en.BlinkTimer += dt;
                    if (en.BlinkTimer >= en.BlinkInterval)
                    {
                        en.BlinkTimer -= en.BlinkInterval;
                        en.BlinkCount++;
                        en.Visible = !en.Visible;
                    }
                    if (en.BlinkCount >= en.BlinkToggleTarget)
                    {
                        _enemies.RemoveAt(i);
                        continue;
                    }
                    _enemies[i] = en;
                    continue;
                }

                if (en.Position.Y - _enemyTextures[en.TextureIndex].Height / 2f > _graphics.PreferredBackBufferHeight + 50)
                {
                    _enemies.RemoveAt(i);
                    continue;
                }

                if (_totalPlayTime >= en.NextShotTime)
                {
                    SpawnLaserTowards(_playerPosition, true, en.Position);
                    float diff = (float)Math.Clamp(1.0 - (_scoreInt * 0.000025), 0.4, 1.0);  // Back to score-based difficulty
                    float minShot = EnemyBaseShotMin * diff;
                    float maxShot = EnemyBaseShotMax * diff;
                    en.NextShotTime = _totalPlayTime + (float)(_random.NextDouble() * (maxShot - minShot) + minShot);
                }

                _enemies[i] = en;
            }
        }

        private void UpdateAsteroids(float dt)
        {
            for (int i = _asteroids.Count - 1; i >= 0; i--)
            {
                var a = _asteroids[i];
                a.Position += a.Velocity * dt;

                if (!a.IsDying && CheckAsteroidHitsPlayer(a))
                {
                    a.IsDying = true; a.Visible = false;
                    a.BlinkTimer = 0f; a.BlinkInterval = 0.18f; a.BlinkCount = 0; a.BlinkToggleTarget = 4;
                    _playerHp -= AsteroidCollisionDamage;
                    if (_playerHp <= 0) 
                    {
                        _isPlayerDead = true;
                        _scoreDatabase.SaveScore(_playerName, _scoreInt);  // Save final score
                    }
                }

                if (a.IsDying)
                {
                    a.BlinkTimer += dt;
                    if (a.BlinkTimer >= a.BlinkInterval)
                    {
                        a.BlinkTimer -= a.BlinkInterval;
                        a.BlinkCount++;
                        a.Visible = !a.Visible;
                    }
                    if (a.BlinkCount >= a.BlinkToggleTarget)
                    {
                        _asteroids.RemoveAt(i);
                        continue;
                    }
                    _asteroids[i] = a;
                    continue;
                }

                if (a.Position.Y - a.Texture.Width / 2f > _graphics.PreferredBackBufferHeight + 60)
                {
                    _asteroids.RemoveAt(i);
                    continue;
                }

                _asteroids[i] = a;
            }
        }

        private void UpdateLasers(float dt)
        {
            for (int i = _lasers.Count - 1; i >= 0; i--)
            {
                var l = _lasers[i];
                l.Position += l.Velocity * dt;
                _lasers[i] = l;
                bool removed = false;

                if (!l.IsEnemy)
                {
                    // Boss collision with player lasers
                    if (_bossActive && CheckLaserHitsBoss(l.Position))
                    {
                        _bossHp--;
                        _lasers.RemoveAt(i);
                        
                        // Check if boss is defeated
                        if (_bossHp <= 0)
                        {
                            _bossActive = false;
                            _enemiesDestroyed += 5; // Boss counts as 5 enemies destroyed (for future boss spawns)
                            _score += EnemyKillScore * 10; // Boss gives 10x enemy score bonus
                            _scoreInt = (int)Math.Round(_score);
                        }
                        continue;
                    }

                    // Enemies
                    for (int ei = 0; ei < _enemies.Count; ei++)
                    {
                        var en = _enemies[ei];
                        if (en.IsDying) continue;
                        var enemyRect = new Rectangle(
                            (int)(en.Position.X - _enemyTextures[en.TextureIndex].Width / 2f),
                            (int)(en.Position.Y - _enemyTextures[en.TextureIndex].Height / 2f),
                            _enemyTextures[en.TextureIndex].Width,
                            _enemyTextures[en.TextureIndex].Height);
                        var laserRect = new Rectangle(
                            (int)(l.Position.X - _laserTexture.Width / 2f),
                            (int)(l.Position.Y - _laserTexture.Height / 2f),
                            _laserTexture.Width,
                            _laserTexture.Height);
                        if (enemyRect.Intersects(laserRect))
                        {
                            en.IsDying = true; en.Visible = false;
                            en.BlinkTimer = 0f; en.BlinkInterval = 0.15f; en.BlinkCount = 0; en.BlinkToggleTarget = 4;
                            _enemies[ei] = en;
                            _enemiesDestroyed++;  // Keep enemy count for boss spawn
                            _score += EnemyKillScore;  // Add score for enemy kill
                            _scoreInt = (int)Math.Round(_score);
                            _lasers.RemoveAt(i);
                            removed = true;
                            break;
                        }
                    }
                    if (removed) continue;

                    // Asteroids
                    for (int ai = 0; ai < _asteroids.Count; ai++)
                    {
                        var a = _asteroids[ai];
                        if (a.IsDying) continue;
                        var tex = a.Texture;
                        var asteroidRect = new Rectangle(
                            (int)(a.Position.X - tex.Width / 2f),
                            (int)(a.Position.Y - tex.Height / 2f),
                            tex.Width,
                            tex.Height);
                        var laserRect = new Rectangle(
                            (int)(l.Position.X - _laserTexture.Width / 2f),
                            (int)(l.Position.Y - _laserTexture.Height / 2f),
                            _laserTexture.Width,
                            _laserTexture.Height);
                        if (asteroidRect.Intersects(laserRect))
                        {
                            a.Hp--;
                            if (a.Hp <= 0)
                            {
                                a.IsDying = true; a.Visible = false;
                                a.BlinkTimer = 0f; a.BlinkInterval = 0.18f; a.BlinkCount = 0; a.BlinkToggleTarget = 4;
                                _score += EnemyKillScore * 0.5; // Asteroids give half points
                                _scoreInt = (int)Math.Round(_score);
                            }
                            _asteroids[ai] = a;
                            _lasers.RemoveAt(i);
                            removed = true;
                            break;
                        }
                    }
                }
                else
                {
                    if (CheckLaserHitsPlayer(l.Position))
                    {
                        _lasers.RemoveAt(i);
                        _playerHp--;
                        if (_playerHp <= 0) 
                        {
                            _isPlayerDead = true;
                            _scoreDatabase.SaveScore(_playerName, _scoreInt);  // Save final score
                        }
                        continue;
                    }
                }

                if (removed) continue;
                if (IsOffScreen(l.Position))
                    _lasers.RemoveAt(i);
            }
        }

        private void SpawnLaserTowards(Vector2 target, bool isEnemy, Vector2? origin = null)
        {
            Vector2 src = origin ?? _playerPosition;
            Vector2 dir = target - src;
            if (dir.LengthSquared() < 0.0001f) dir = _facing; else dir.Normalize();
            float frontOffset = (_playerTexture.Height / 2f) + 8f;
            Vector2 spawnPos = src + dir * frontOffset;
            float speedMult = _playerSpeed * _laserSpeedMultiplier;
            if (isEnemy) speedMult *= EnemyLaserSpeedFactor;
            _lasers.Add(new Laser { Position = spawnPos, Velocity = dir * speedMult, IsEnemy = isEnemy });
        }

        private void SpawnEnemy()
        {
            int w = _graphics.PreferredBackBufferWidth;
            const int margin = 40;
            Vector2 pos = new(_random.Next(margin, w - margin), -margin);
            int texIndex = _random.Next(_enemyTextures.Length);

            float diff = (float)Math.Clamp(1.0 - (_scoreInt * 0.000025), 0.4, 1.0);  // Back to score-based difficulty
            float minShot = EnemyBaseShotMin * diff;
            float maxShot = EnemyBaseShotMax * diff;

            float baseHoriz = (float)(_random.NextDouble() * (EnemyHorizontalSpeedMax - EnemyHorizontalSpeedMin) + EnemyHorizontalSpeedMin);
            float speedScale = 1f + MathF.Min(_scoreInt / 15000f, 1.2f);  // Scale based on score
            float horizSpeed = baseHoriz * speedScale;

            Enemy en = new()
            {
                Position = pos,
                TextureIndex = texIndex,
                Speed = (float)(_random.NextDouble() * 80.0 + 40.0),
                NextShotTime = _totalPlayTime + (float)(_random.NextDouble() * (maxShot - minShot) + minShot),
                IsDying = false,
                Visible = true,
                BlinkTimer = 0f,
                BlinkInterval = 0.15f,
                BlinkCount = 0,
                BlinkToggleTarget = 4,
                HorizontalDir = _random.Next(0, 2) == 0 ? -1f : 1f,
                HorizontalSpeed = horizSpeed,
                HorizontalSwitchTimer = (float)(_random.NextDouble() * 0.5f),
                HorizontalSwitchInterval = (float)(_random.NextDouble() * (EnemyHorizontalIntervalMax - EnemyHorizontalIntervalMin) + EnemyHorizontalIntervalMin)
            };
            _enemies.Add(en);
        }

        private void SpawnAsteroid()
        {
            int w = _graphics.PreferredBackBufferWidth;
            const int margin = 50;
            Vector2 pos = new(_random.Next(margin, w - margin), -60);
            int texIndex = _random.Next(_asteroidTextures.Length);
            float vy = (float)(_random.NextDouble() * 70 + 90);
            float vx = (float)(_random.NextDouble() * 80 - 40);
            Asteroid a = new()
            {
                Texture = _asteroidTextures[texIndex],
                Position = pos,
                Velocity = new Vector2(vx, vy),
                Hp = AsteroidMaxHp,
                IsDying = false,
                Visible = true,
                BlinkTimer = 0f,
                BlinkInterval = 0.18f,
                BlinkCount = 0,
                BlinkToggleTarget = 4
            };
            _asteroids.Add(a);
        }

        private void UpdateBoss(float dt)
        {
            // Move boss down towards middle of screen, but stop at half screen height
            float halfScreenHeight = _graphics.PreferredBackBufferHeight / 2f;
            if (_bossPosition.Y < halfScreenHeight)
            {
                _bossPosition.Y += BossSpeed * dt;
                if (_bossPosition.Y > halfScreenHeight)
                    _bossPosition.Y = halfScreenHeight;
            }

            // Horizontal movement - keeps boss moving (faster now)
            _bossMovementTimer += dt;
            if (_bossMovementTimer >= BossMovementChangeInterval)
            {
                _bossMovementTimer -= BossMovementChangeInterval;
                _bossHorizontalDir = -_bossHorizontalDir; // Change direction
                
                // Increased speed range for more aggressive movement
                _bossHorizontalSpeed = (float)(_random.NextDouble() * 120 + 100); // Speed between 100-220 (was 60-140)
            }

            // Apply horizontal movement
            _bossPosition.X += _bossHorizontalDir * _bossHorizontalSpeed * dt;

            // Keep boss within screen bounds with some margin
            float margin = 100f;
            if (_bossPosition.X < margin)
            {
                _bossPosition.X = margin;
                _bossHorizontalDir = 1f; // Force right movement
                _bossMovementTimer = 0f; // Reset timer for immediate direction change
            }
            else if (_bossPosition.X > _graphics.PreferredBackBufferWidth - margin)
            {
                _bossPosition.X = _graphics.PreferredBackBufferWidth - margin;
                _bossHorizontalDir = -1f; // Force left movement
                _bossMovementTimer = 0f; // Reset timer for immediate direction change
            }

            // Boss collision with player
            if (CheckBossHitsPlayer())
            {
                _playerHp -= 5; // Boss does more damage than regular enemies
                if (_playerHp <= 0)
                {
                    _isPlayerDead = true;
                    _scoreDatabase.SaveScore(_playerName, _scoreInt);  // Save final score
                }
                
                // Push boss back up slightly after hitting player
                _bossPosition.Y = Math.Max(_bossPosition.Y - 50, halfScreenHeight - 100);
            }
        }

        private bool CheckEnemyHitsPlayer(Enemy en)
        {
            var tex = _enemyTextures[en.TextureIndex];
            var enemyRect = new Rectangle(
                (int)(en.Position.X - tex.Width / 2f),
                (int)(en.Position.Y - tex.Height / 2f),
                tex.Width,
                tex.Height);
            var playerRect = new Rectangle(
                (int)(_playerPosition.X - _playerTexture.Width / 2f),
                (int)(_playerPosition.Y - _playerTexture.Height / 2f),
                _playerTexture.Width,
                _playerTexture.Height);
            return enemyRect.Intersects(playerRect);
        }

        private bool CheckAsteroidHitsPlayer(Asteroid a)
        {
            var tex = a.Texture;
            var r = new Rectangle(
                (int)(a.Position.X - tex.Width / 2f),
                (int)(a.Position.Y - tex.Height / 2f),
                tex.Width,
                tex.Height);
            var playerRect = new Rectangle(
                (int)(_playerPosition.X - _playerTexture.Width / 2f),
                (int)(_playerPosition.Y - _playerTexture.Height / 2f),
                _playerTexture.Width,
                _playerTexture.Height);
            return r.Intersects(playerRect);
        }

        private bool CheckLaserHitsPlayer(Vector2 laserPos)
        {
            var playerRect = new Rectangle(
                (int)(_playerPosition.X - _playerTexture.Width / 2f),
                (int)(_playerPosition.Y - _playerTexture.Height / 2f),
                _playerTexture.Width,
                _playerTexture.Height);
            var laserRect = new Rectangle(
                (int)(laserPos.X - _laserTexture.Width / 2f),
                (int)(laserPos.Y - _laserTexture.Height / 2f),
                _laserTexture.Width,
                _laserTexture.Height);
            return playerRect.Intersects(laserRect);
        }

        private bool CheckBossHitsPlayer()
        {
            if (!_bossActive || _bossTexture == null) return false;
            
            var bossRect = new Rectangle(
                (int)(_bossPosition.X - _bossTexture.Width / 2f),
                (int)(_bossPosition.Y - _bossTexture.Height / 2f),
                _bossTexture.Width,
                _bossTexture.Height);
            var playerRect = new Rectangle(
                (int)(_playerPosition.X - _playerTexture.Width / 2f),
                (int)(_playerPosition.Y - _playerTexture.Height / 2f),
                _playerTexture.Width,
                _playerTexture.Height);
            return bossRect.Intersects(playerRect);
        }

        private bool CheckLaserHitsBoss(Vector2 laserPos)
        {
            if (!_bossActive || _bossTexture == null) return false;
            
            var bossRect = new Rectangle(
                (int)(_bossPosition.X - _bossTexture.Width / 2f),
                (int)(_bossPosition.Y - _bossTexture.Height / 2f),
                _bossTexture.Width,
                _bossTexture.Height);
            var laserRect = new Rectangle(
                (int)(laserPos.X - _laserTexture.Width / 2f),
                (int)(laserPos.Y - _laserTexture.Height / 2f),
                _laserTexture.Width,
                _laserTexture.Height);
            return bossRect.Intersects(laserRect);
        }

        private bool IsOffScreen(Vector2 pos)
        {
            int w = _graphics.PreferredBackBufferWidth;
            int h = _graphics.PreferredBackBufferHeight;
            const int margin = 32;
            return pos.X < -margin || pos.X > w + margin || pos.Y < -margin || pos.Y > h + margin;
        }

        // Menu helpers
        private void GetMenuLayout(out Rectangle startRect, out Rectangle leaderboardRect, out Rectangle quitRect)
        {
            int w = _graphics.PreferredBackBufferWidth;
            int h = _graphics.PreferredBackBufferHeight;

            int bw = Math.Min(360, w - 160);
            int bh = 64;
            int x = (w - bw) / 2;
            int yStart = (int)(h * 0.4f);
            int spacing = 18;

            startRect = new Rectangle(x, yStart, bw, bh);
            leaderboardRect = new Rectangle(x, yStart + bh + spacing, bw, bh);
            quitRect = new Rectangle(x, yStart + 2 * (bh + spacing), bw, bh);
        }

        private void ResetGame()
        {
            _enemies.Clear();
            _asteroids.Clear();
            _lasers.Clear();

            _playerHp = 5;
            _isPlayerDead = false;
            _playerPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2f, _graphics.PreferredBackBufferHeight / 2f);
            _facing = new Vector2(0, -1);

            _enemiesDestroyed = 0;  // Reset enemy counter for boss spawn
            _score = 0.0;  // Reset score
            _scoreInt = 0;  // Reset score display
            _totalPlayTime = 0f;

            _enemySpawnTimer = 0f;
            _asteroidSpawnTimer = 0f;
            _shootCooldownTimer = 0f;

            // Reset boss variables
            _bossActive = false;
            _bossSpawned = false;
            _bossHp = BossMaxHp;
            _bossMovementTimer = 0f;

            _bgFrameIndex = 0;
            _bgFrameTimer = 0f;

            _previousMouseState = Mouse.GetState();
        }

        // Structs
        private struct Laser
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public bool IsEnemy;
        }

        private struct Enemy
        {
            public Vector2 Position;
            public float NextShotTime;
            public float Speed;
            public int TextureIndex;
            public bool IsDying;
            public bool Visible;
            public float BlinkTimer;
            public float BlinkInterval;
            public int BlinkCount;
            public int BlinkToggleTarget;
            public float HorizontalDir;
            public float HorizontalSpeed;
            public float HorizontalSwitchTimer;
            public float HorizontalSwitchInterval;
        }

        private struct Asteroid
        {
            public Texture2D Texture;
            public Vector2 Position;
            public Vector2 Velocity;
            public int Hp;
            public bool IsDying;
            public bool Visible;
            public float BlinkTimer;
            public float BlinkInterval;
            public int BlinkCount;
            public int BlinkToggleTarget;
        }
    }
}