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

        private readonly Random _random = new();
        private float _totalPlayTime = 0f;

        // Score & difficulty scaling
        private double _score = 0.0;
        private int _scoreInt = 0;
        private SpriteFont _scoreFont;

        private const double ScorePerSecond = 100.0;
        private const double EnemyKillScore = 300.0;
        private const float BaseEnemySpawnInterval = 5f;
        private const float MinEnemySpawnInterval = 1f;
        private const int ScoreStepForDifficulty = 1000;
        private const float ScoreIntervalReductionPerStep = 0.2f;

        // Enemy shooting difficulty
        private const float EnemyBaseShotMin = 2.0f;
        private const float EnemyBaseShotMax = 5.2f;
        private const float EnemyShotDifficultyFactor = 0.00025f;

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

        // UI / Menu
        private enum GameState { MainMenu, Playing }
        private GameState _state = GameState.MainMenu;
        private Texture2D _uiPixel;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1000,
                PreferredBackBufferHeight = 900
            };
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
            base.Initialize();
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

        protected override void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();

            // Quitter via Échap depuis n'importe quel écran du menu
            if (_state == GameState.MainMenu)
            {
                // Background anim continue
                float dtMenu = (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_bgFrames.Count > 1)
                {
                    _bgFrameTimer += dtMenu;
                    while (_bgFrameTimer >= BgFrameDuration)
                    {
                        _bgFrameTimer -= BgFrameDuration;
                        _bgFrameIndex = (_bgFrameIndex + 1) % _bgFrames.Count;
                    }
                }

                if (keyboard.IsKeyDown(Keys.Escape) ||
                    GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                {
                    Exit();
                }

                // Layout et interactions
                GetMenuLayout(out var startRect, out var quitRect);
                bool click = mouse.LeftButton == ButtonState.Pressed &&
                             _previousMouseState.LeftButton == ButtonState.Released;

                if (keyboard.IsKeyDown(Keys.Enter) || keyboard.IsKeyDown(Keys.Space) ||
                    (click && startRect.Contains(mouse.Position)))
                {
                    _state = GameState.Playing;
                    ResetGame();
                }
                else if (click && quitRect.Contains(mouse.Position))
                {
                    Exit();
                }

                _previousMouseState = mouse;
                base.Update(gameTime);
                return;
            }

            // Ici: état Playing
            // Retour au menu depuis l'écran Game Over
            if (_isPlayerDead)
            {
                if (keyboard.IsKeyDown(Keys.Enter) || keyboard.IsKeyDown(Keys.Space) ||
                    (mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released))
                {
                    _state = GameState.MainMenu;
                }
                _previousMouseState = mouse;
                base.Update(gameTime);
                return;
            }

            if (keyboard.IsKeyDown(Keys.Escape) ||
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _spawnedEnemyThisUpdate = false;

            // Background anim
            if (_bgFrames.Count > 1)
            {
                _bgFrameTimer += dt;
                while (_bgFrameTimer >= BgFrameDuration)
                {
                    _bgFrameTimer -= BgFrameDuration;
                    _bgFrameIndex = (_bgFrameIndex + 1) % _bgFrames.Count;
                }
            }

            // Score et temps de jeu
            _totalPlayTime += dt;
            _score += dt * ScorePerSecond;
            _scoreInt = (int)_score;

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

            // Shooting
            if (mouse.LeftButton == ButtonState.Pressed &&
                _previousMouseState.LeftButton == ButtonState.Released &&
                _shootCooldownTimer <= 0f)
            {
                SpawnLaserTowards(new Vector2(mouse.X, mouse.Y), false);
                _shootCooldownTimer = ShootCooldownSeconds;
            }

            // Enemy spawn
            int steps = _scoreInt / ScoreStepForDifficulty;
            float enemyInterval = BaseEnemySpawnInterval - steps * ScoreIntervalReductionPerStep;
            if (enemyInterval < MinEnemySpawnInterval) enemyInterval = MinEnemySpawnInterval;
            _enemySpawnTimer += dt;
            if (_enemySpawnTimer >= enemyInterval && !_spawnedEnemyThisUpdate)
            {
                _enemySpawnTimer -= enemyInterval;
                SpawnEnemy();
                _spawnedEnemyThisUpdate = true;
            }

            // Asteroid spawn
            float asteroidInterval = AsteroidSpawnBaseInterval - (float)(_score * AsteroidSpawnDifficultyFactor);
            if (asteroidInterval < AsteroidSpawnMinInterval) asteroidInterval = AsteroidSpawnMinInterval;
            _asteroidSpawnTimer += dt;
            if (_asteroidSpawnTimer >= asteroidInterval)
            {
                _asteroidSpawnTimer -= asteroidInterval;
                SpawnAsteroid();
            }

            UpdateEnemies(dt);
            UpdateAsteroids(dt);
            UpdateLasers(dt);

            _previousMouseState = mouse;
            base.Update(gameTime);
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

            if (_state == GameState.MainMenu)
            {
                // Assombrir le fond
                var overlay = new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
                _spriteBatch.Draw(_uiPixel, overlay, new XnaColor(0, 0, 0, 160));

                // Boutons
                GetMenuLayout(out var startRect, out var quitRect);
                var mouse = Mouse.GetState();
                bool hoverStart = startRect.Contains(mouse.Position);
                bool hoverQuit = quitRect.Contains(mouse.Position);

                var startColor = hoverStart ? XnaColor.Lerp(XnaColor.DodgerBlue, XnaColor.White, 0.25f) : XnaColor.DodgerBlue;
                var quitColor = hoverQuit ? XnaColor.Lerp(XnaColor.DimGray, XnaColor.White, 0.25f) : XnaColor.DimGray;

                _spriteBatch.Draw(_uiPixel, startRect, startColor);
                _spriteBatch.Draw(_uiPixel, quitRect, quitColor);

                // Titres et libellés
                if (_scoreFont != null)
                {
                    string title = "Menu Principal";
                    var tSize = _scoreFont.MeasureString(title);
                    var tPos = new Vector2((_graphics.PreferredBackBufferWidth - tSize.X) / 2f, _graphics.PreferredBackBufferHeight * 0.18f);
                    _spriteBatch.DrawString(_scoreFont, title, tPos, XnaColor.White);

                    string startText = "Démarrer";
                    string quitText = "Quitter";

                    var sSize = _scoreFont.MeasureString(startText);
                    var qSize = _scoreFont.MeasureString(quitText);

                    var sPos = new Vector2(startRect.X + (startRect.Width - sSize.X) / 2f, startRect.Y + (startRect.Height - sSize.Y) / 2f);
                    var qPos = new Vector2(quitRect.X + (quitRect.Width - qSize.X) / 2f, quitRect.Y + (quitRect.Height - qSize.Y) / 2f);

                    _spriteBatch.DrawString(_scoreFont, startText, sPos, XnaColor.White);
                    _spriteBatch.DrawString(_scoreFont, quitText, qPos, XnaColor.White);

                    string hint = "Entrée/Espace ou clic pour démarrer";
                    var hSize = _scoreFont.MeasureString(hint);
                    var hPos = new Vector2((_graphics.PreferredBackBufferWidth - hSize.X) / 2f, quitRect.Bottom + 24);
                    _spriteBatch.DrawString(_scoreFont, hint, hPos, XnaColor.LightGray);
                }

                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

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

            // Score + HP
            if (_scoreFont != null)
            {
                string text = $"SCORE: {_scoreInt:N0}  HP: {_playerHp}";
                var size = _scoreFont.MeasureString(text);
                var pos = new Vector2(_graphics.PreferredBackBufferWidth - 10 - size.X, 10);
                _spriteBatch.DrawString(_scoreFont, text, pos, XnaColor.White);
            }

            // Overlay Game Over
            if (_isPlayerDead)
            {
                var overlay = new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
                _spriteBatch.Draw(_uiPixel, overlay, new XnaColor(0, 0, 0, 160));
                if (_scoreFont != null)
                {
                    string over = "Game Over";
                    string back = "Entrée/Espace ou clic pour retourner au menu";
                    var oSize = _scoreFont.MeasureString(over);
                    var bSize = _scoreFont.MeasureString(back);
                    var oPos = new Vector2((_graphics.PreferredBackBufferWidth - oSize.X) / 2f, _graphics.PreferredBackBufferHeight * 0.4f);
                    var bPos = new Vector2((_graphics.PreferredBackBufferWidth - bSize.X) / 2f, oPos.Y + oSize.Y + 16f);
                    _spriteBatch.DrawString(_scoreFont, over, oPos, XnaColor.White);
                    _spriteBatch.DrawString(_scoreFont, back, bPos, XnaColor.LightGray);
                }
            }

            _spriteBatch.End();
            base.Draw(gameTime);
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
                    float scale = 1f + MathF.Min(_scoreInt / 3000f, 1.5f);
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
                    if (_playerHp <= 0) _isPlayerDead = true;
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
                    float diff = (float)Math.Clamp(1.0 - (_scoreInt * EnemyShotDifficultyFactor), 0.4, 1.0);
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
                    if (_playerHp <= 0) _isPlayerDead = true;
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
                            _score += EnemyKillScore; _scoreInt = (int)_score;
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
                        if (_playerHp <= 0) _isPlayerDead = true;
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

            float diff = (float)Math.Clamp(1.0 - (_scoreInt * EnemyShotDifficultyFactor), 0.4, 1.0);
            float minShot = EnemyBaseShotMin * diff;
            float maxShot = EnemyBaseShotMax * diff;

            float baseHoriz = (float)(_random.NextDouble() * (EnemyHorizontalSpeedMax - EnemyHorizontalSpeedMin) + EnemyHorizontalSpeedMin);
            float speedScale = 1f + MathF.Min(_scoreInt / 5000f, 1.2f);
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

        private bool IsOffScreen(Vector2 pos)
        {
            int w = _graphics.PreferredBackBufferWidth;
            int h = _graphics.PreferredBackBufferHeight;
            const int margin = 32;
            return pos.X < -margin || pos.X > w + margin || pos.Y < -margin || pos.Y > h + margin;
        }

        // Menu helpers
        private void GetMenuLayout(out Rectangle startRect, out Rectangle quitRect)
        {
            int w = _graphics.PreferredBackBufferWidth;
            int h = _graphics.PreferredBackBufferHeight;

            int bw = Math.Min(360, w - 160);
            int bh = 64;
            int x = (w - bw) / 2;
            int yStart = (int)(h * 0.45f);
            int spacing = 18;

            startRect = new Rectangle(x, yStart, bw, bh);
            quitRect = new Rectangle(x, yStart + bh + spacing, bw, bh);
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

            _score = 0.0;
            _scoreInt = 0;
            _totalPlayTime = 0f;

            _enemySpawnTimer = 0f;
            _asteroidSpawnTimer = 0f;
            _shootCooldownTimer = 0f;

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