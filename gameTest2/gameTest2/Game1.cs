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
        private const float BgFrameDuration = 0.03f; // 30 ms par frame (~33 fps)
        private Texture2D _bgFallback; // en cas d'échec

        // Player
        private Texture2D _playerTexture;
        private Vector2 _playerPosition;
        private float _playerSpeed = 200f;
        private Vector2 _facing = new(0, -1);

        // Lasers
        private Texture2D _laserTexture;
        private readonly List<Laser> _lasers = new();
        private float _laserSpeedMultiplier = 3f;

        // Enemies
        private Texture2D[] _enemyTextures = new Texture2D[12];
        private readonly List<Enemy> _enemies = new();
        private float _enemySpawnTimer = 0f;
        private readonly Random _random = new();
        private float _totalPlayTime = 0f;

        // Difficulty / spawn
        private const float SecondsPerLevel = 20f;
        private const float BaseEnemySpawnInterval = 5f;
        private const float MinEnemySpawnInterval = 1f;

        // Input
        private MouseState _previousMouseState;

        // Player state
        private int _playerHp = 5;
        private bool _isPlayerDead = false;

        // Score
        private double _score = 0.0;
        private int _scoreInt = 0;
        private SpriteFont _scoreFont;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 800,
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

            // Score font
            try { _scoreFont = Content.Load<SpriteFont>("fonts/score"); } catch { _scoreFont = null; }
        }

        private void LoadBackgroundFrames()
        {
            // Attend frame00 à frame32
            for (int i = 0; i <= 32; i++)
            {
                string name = $"textures/background/frame{i:00}";
                try
                {
                    var tex = Content.Load<Texture2D>(name);
                    _bgFrames.Add(tex);
                }
                catch
                {
                    // ignore missing; continue
                }
            }

            if (_bgFrames.Count == 0)
            {
                _bgFallback = new Texture2D(GraphicsDevice, 1, 1);
                _bgFallback.SetData(new[] { XnaColor.Black });
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (_isPlayerDead)
            {
                base.Update(gameTime);
                return;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape) ||
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _totalPlayTime += dt;

            // Background animation
            if (_bgFrames.Count > 1)
            {
                _bgFrameTimer += dt;
                while (_bgFrameTimer >= BgFrameDuration)
                {
                    _bgFrameTimer -= BgFrameDuration;
                    _bgFrameIndex = (_bgFrameIndex + 1) % _bgFrames.Count;
                }
            }

            // Score
            _score += dt * 1000.0;
            _scoreInt = (int)_score;

            // Movement
            Vector2 move = Vector2.Zero;
            if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)) move.Y -= 1;
            if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) move.Y += 1;
            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) move.X -= 1;
            if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) move.X += 1;
            if (move.LengthSquared() > 0f)
            {
                move = Vector2.Normalize(move) * _playerSpeed * dt;
                _facing = Vector2.Normalize(move);
            }
            _playerPosition += move;

            // Shoot
            if (mouse.LeftButton == ButtonState.Pressed &&
                _previousMouseState.LeftButton == ButtonState.Released)
            {
                SpawnLaserTowards(new Vector2(mouse.X, mouse.Y), false);
            }

            // Enemy spawn
            int level = 1 + (int)(_totalPlayTime / SecondsPerLevel);
            float spawnInterval = Math.Max(MinEnemySpawnInterval,
                BaseEnemySpawnInterval - (level - 1) * 0.4f);
            _enemySpawnTimer += dt;
            if (_enemySpawnTimer >= spawnInterval)
            {
                _enemySpawnTimer = 0f;
                SpawnEnemy();
            }

            // Enemies update
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var en = _enemies[i];
                en.Position.Y += en.Speed * dt;

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

                if (en.Position.Y - _enemyTextures[en.TextureIndex].Height / 2f >
                    _graphics.PreferredBackBufferHeight + 50)
                {
                    _enemies.RemoveAt(i);
                    continue;
                }

                if (_totalPlayTime >= en.NextShotTime)
                {
                    SpawnLaserTowards(_playerPosition, true, en.Position);
                    en.NextShotTime = _totalPlayTime +
                        (float)(_random.NextDouble() * 5.0 + 3.0);
                }

                _enemies[i] = en;
            }

            // Lasers
            for (int i = _lasers.Count - 1; i >= 0; i--)
            {
                var l = _lasers[i];
                l.Position += l.Velocity * dt;
                _lasers[i] = l;

                bool removed = false;
                if (!l.IsEnemy)
                {
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
                            en.IsDying = true;
                            en.Visible = false;
                            en.BlinkTimer = 0f;
                            en.BlinkInterval = 0.15f;
                            en.BlinkCount = 0;
                            en.BlinkToggleTarget = 4;
                            _enemies[ei] = en;

                            _score += 3000;
                            _scoreInt = (int)_score;

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
                var dest = new Rectangle(0, 0,
                    _graphics.PreferredBackBufferWidth,
                    _graphics.PreferredBackBufferHeight);
                _spriteBatch.Draw(_bgFrames[_bgFrameIndex], dest, XnaColor.White);
            }
            else if (_bgFallback != null)
            {
                _spriteBatch.Draw(_bgFallback, new Rectangle(0, 0,
                    _graphics.PreferredBackBufferWidth,
                    _graphics.PreferredBackBufferHeight), XnaColor.Black);
            }

            // Enemies
            foreach (var en in _enemies)
            {
                if (!en.Visible) continue;
                var tex = _enemyTextures[en.TextureIndex];
                _spriteBatch.Draw(tex,
                    en.Position,
                    null,
                    XnaColor.White,
                    0f,
                    new Vector2(tex.Width / 2f, tex.Height / 2f),
                    1f,
                    SpriteEffects.None,
                    0f);
            }

            // Lasers
            foreach (var l in _lasers)
            {
                float rotation = (float)Math.Atan2(l.Velocity.Y, l.Velocity.X) + MathHelper.PiOver2;
                var tint = l.IsEnemy ? XnaColor.OrangeRed : XnaColor.White;
                _spriteBatch.Draw(_laserTexture,
                    l.Position,
                    null,
                    tint,
                    rotation,
                    new Vector2(_laserTexture.Width / 2f, _laserTexture.Height / 2f),
                    1f,
                    SpriteEffects.None,
                    0f);
            }

            // Player
            _spriteBatch.Draw(_playerTexture,
                _playerPosition,
                null,
                XnaColor.White,
                0f,
                new Vector2(_playerTexture.Width / 2f, _playerTexture.Height / 2f),
                1f,
                SpriteEffects.None,
                0f);

            // Score
            if (_scoreFont != null)
            {
                string text = $"SCORE: {_scoreInt:N0}";
                var size = _scoreFont.MeasureString(text);
                var pos = new Vector2(_graphics.PreferredBackBufferWidth - 10 - size.X, 10);
                _spriteBatch.DrawString(_scoreFont, text, pos, XnaColor.White);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void SpawnLaserTowards(Vector2 target, bool isEnemy, Vector2? origin = null)
        {
            Vector2 src = origin ?? _playerPosition;
            Vector2 dir = target - src;
            if (dir.LengthSquared() < 0.0001f) dir = _facing;
            else dir.Normalize();

            float frontOffset = (_playerTexture.Height / 2f) + 8f;
            Vector2 spawnPos = src + dir * frontOffset;
            Vector2 velocity = dir * (_playerSpeed * _laserSpeedMultiplier);

            _lasers.Add(new Laser { Position = spawnPos, Velocity = velocity, IsEnemy = isEnemy });
        }

        private void SpawnEnemy()
        {
            int w = _graphics.PreferredBackBufferWidth;
            const int margin = 40;
            Vector2 pos = new(_random.Next(margin, w - margin), -margin);
            int texIndex = _random.Next(_enemyTextures.Length);

            Enemy en = new()
            {
                Position = pos,
                TextureIndex = texIndex,
                Speed = (float)(_random.NextDouble() * 80.0 + 40.0),
                NextShotTime = _totalPlayTime + (float)(_random.NextDouble() * 5.0 + 3.0),
                IsDying = false,
                Visible = true,
                BlinkTimer = 0f,
                BlinkInterval = 0.15f,
                BlinkCount = 0,
                BlinkToggleTarget = 4
            };
            _enemies.Add(en);
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
        }
    }
}