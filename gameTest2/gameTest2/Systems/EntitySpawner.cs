using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using gameTest2.Models;
using gameTest2.Config;

namespace gameTest2.Systems
{
    /// <summary>
    /// Manages spawning of all game entities including enemies, asteroids, bosses, buffs, and lasers.
    /// Handles difficulty scaling and randomization for dynamic gameplay.
    /// </summary>
    public class EntitySpawner
    {
        private readonly Random _random;
        private readonly int _screenWidth;
        private readonly int _screenHeight;

        /// <summary>
        /// Initializes the entity spawner with screen dimensions.
        /// </summary>
        /// <param name="screenWidth">Width of the game screen</param>
        /// <param name="screenHeight">Height of the game screen</param>
        public EntitySpawner(int screenWidth, int screenHeight)
        {
            _random = new Random();
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        /// <summary>
        /// Spawns an enemy with randomized properties and difficulty-scaled attributes.
        /// </summary>
        /// <param name="enemyTextures">Array of available enemy textures</param>
        /// <param name="currentScore">Current player score (affects difficulty)</param>
        /// <param name="totalPlayTime">Total game time (for shooting timing)</param>
        /// <returns>A new Enemy instance</returns>
        public Enemy SpawnEnemy(Texture2D[] enemyTextures, int currentScore, float totalPlayTime)
        {
            const int margin = 40;
            Vector2 pos = new(_random.Next(margin, _screenWidth - margin), -margin);
            int texIndex = _random.Next(enemyTextures.Length);

            // Difficulty scaling for shooting frequency
            float diff = (float)Math.Clamp(1.0 - (currentScore * GameConstants.EnemyShotDifficultyFactor), 0.4, 1.0);
            float minShot = GameConstants.EnemyBaseShotMin * diff;
            float maxShot = GameConstants.EnemyBaseShotMax * diff;

            // Difficulty scaling for horizontal movement
            float baseHoriz = (float)(_random.NextDouble() * 
                (GameConstants.EnemyHorizontalSpeedMax - GameConstants.EnemyHorizontalSpeedMin) + 
                GameConstants.EnemyHorizontalSpeedMin);
            float speedScale = 1f + MathF.Min(currentScore / GameConstants.EnemyHorizontalSpeedScoreDivisor, 
                GameConstants.EnemyHorizontalMaxSpeedMultiplier);
            float horizSpeed = baseHoriz * speedScale;

            return new Enemy
            {
                Position = pos,
                TextureIndex = texIndex,
                Speed = (float)(_random.NextDouble() * 80.0 + 40.0),
                NextShotTime = totalPlayTime + (float)(_random.NextDouble() * (maxShot - minShot) + minShot),
                IsDying = false,
                Visible = true,
                BlinkTimer = 0f,
                BlinkInterval = GameConstants.EnemyBlinkInterval,
                BlinkCount = 0,
                BlinkToggleTarget = GameConstants.EnemyBlinkToggleTarget,
                HorizontalDir = _random.Next(0, 2) == 0 ? -1f : 1f,
                HorizontalSpeed = horizSpeed,
                HorizontalSwitchTimer = (float)(_random.NextDouble() * 0.5f),
                HorizontalSwitchInterval = (float)(_random.NextDouble() * 
                    (GameConstants.EnemyHorizontalIntervalMax - GameConstants.EnemyHorizontalIntervalMin) + 
                    GameConstants.EnemyHorizontalIntervalMin)
            };
        }

        /// <summary>
        /// Spawns an asteroid with randomized properties.
        /// </summary>
        /// <param name="asteroidTextures">Array of available asteroid textures</param>
        /// <returns>A new Asteroid instance</returns>
        public Asteroid SpawnAsteroid(Texture2D[] asteroidTextures)
        {
            const int margin = 50;
            Vector2 pos = new(_random.Next(margin, _screenWidth - margin), -60);
            int texIndex = _random.Next(asteroidTextures.Length);
            
            // Random velocity with vertical and horizontal components
            float vy = (float)(_random.NextDouble() * 70 + 90);
            float vx = (float)(_random.NextDouble() * 80 - 40);

            return new Asteroid
            {
                Texture = asteroidTextures[texIndex],
                Position = pos,
                Velocity = new Vector2(vx, vy),
                Hp = GameConstants.AsteroidMaxHp,
                IsDying = false,
                Visible = true,
                BlinkTimer = 0f,
                BlinkInterval = GameConstants.AsteroidBlinkInterval,
                BlinkCount = 0,
                BlinkToggleTarget = GameConstants.AsteroidBlinkToggleTarget
            };
        }

        /// <summary>
        /// Spawns a boss at the center top of the screen.
        /// </summary>
        /// <param name="textureIndex">Index of the boss texture to use</param>
        /// <returns>Boss spawn position and initial horizontal direction</returns>
        public (Vector2 position, float horizontalDir) SpawnBoss(int textureIndex)
        {
            Vector2 position = new Vector2(_screenWidth / 2f, -100);
            float horizontalDir = _random.Next(0, 2) == 0 ? -1f : 1f; // Random initial direction
            
            return (position, horizontalDir);
        }

        /// <summary>
        /// Attempts to spawn a buff drop at the specified position.
        /// Uses configured drop chance to determine if buff spawns.
        /// </summary>
        /// <param name="position">Position where the buff should spawn</param>
        /// <returns>A new BuffPickup if successful, null otherwise</returns>
        public BuffPickup? TrySpawnBuffDrop(Vector2 position)
        {
            if (_random.NextDouble() < GameConstants.BuffDropChance)
            {
                // Random buff type
                BuffType type = (BuffType)_random.Next(0, 5);

                return new BuffPickup
                {
                    Type = type,
                    Position = position,
                    Lifetime = GameConstants.BuffLifetime,
                    FloatOffset = 0f
                };
            }

            return null;
        }

        /// <summary>
        /// Spawns a laser projectile in a specific direction.
        /// </summary>
        /// <param name="origin">Starting position of the laser</param>
        /// <param name="direction">Direction vector (will be normalized)</param>
        /// <param name="facing">Default facing direction if direction is zero</param>
        /// <param name="isEnemy">Whether this is an enemy laser</param>
        /// <param name="playerSpeed">Base player speed for velocity calculation</param>
        /// <param name="laserSpeedMultiplier">Multiplier for laser speed</param>
        /// <param name="textureHeight">Height of the texture spawning the laser (for offset)</param>
        /// <returns>A new Laser instance</returns>
        public Laser SpawnLaserInDirection(
            Vector2 origin, 
            Vector2 direction, 
            Vector2 facing,
            bool isEnemy, 
            float playerSpeed, 
            float laserSpeedMultiplier,
            float textureHeight)
        {
            Vector2 dir = direction;
            if (dir.LengthSquared() < 0.0001f) 
                dir = facing;
            else 
                dir.Normalize();

            float frontOffset = (textureHeight / 2f) + GameConstants.LaserSpawnOffset;
            Vector2 spawnPos = origin + dir * frontOffset;
            
            float speedMult = playerSpeed * laserSpeedMultiplier;
            if (isEnemy) 
                speedMult *= GameConstants.EnemyLaserSpeedFactor;

            return new Laser 
            { 
                Position = spawnPos, 
                Velocity = dir * speedMult, 
                IsEnemy = isEnemy 
            };
        }

        /// <summary>
        /// Spawns a laser projectile towards a target position.
        /// </summary>
        /// <param name="origin">Starting position of the laser</param>
        /// <param name="target">Target position to aim towards</param>
        /// <param name="facing">Default facing direction if target is at origin</param>
        /// <param name="isEnemy">Whether this is an enemy laser</param>
        /// <param name="playerSpeed">Base player speed for velocity calculation</param>
        /// <param name="laserSpeedMultiplier">Multiplier for laser speed</param>
        /// <param name="textureHeight">Height of the texture spawning the laser (for offset)</param>
        /// <returns>A new Laser instance</returns>
        public Laser SpawnLaserTowards(
            Vector2 origin, 
            Vector2 target, 
            Vector2 facing,
            bool isEnemy, 
            float playerSpeed, 
            float laserSpeedMultiplier,
            float textureHeight)
        {
            Vector2 dir = target - origin;
            return SpawnLaserInDirection(origin, dir, facing, isEnemy, playerSpeed, laserSpeedMultiplier, textureHeight);
        }

        /// <summary>
        /// Spawns a boss laser with spread pattern towards the player.
        /// </summary>
        /// <param name="bossPosition">Position of the boss</param>
        /// <param name="playerPosition">Position of the player</param>
        /// <param name="playerSpeed">Base player speed for velocity calculation</param>
        /// <param name="laserSpeedMultiplier">Multiplier for laser speed</param>
        /// <param name="bossTextureHeight">Height of boss texture for spawn offset</param>
        /// <returns>A new enemy Laser instance with spread</returns>
        public Laser SpawnBossLaser(
            Vector2 bossPosition,
            Vector2 playerPosition,
            float playerSpeed,
            float laserSpeedMultiplier,
            float bossTextureHeight)
        {
            Vector2 directionToPlayer = playerPosition - bossPosition;
            directionToPlayer.Normalize();

            // Add random spread
            float spreadAngle = (float)(_random.NextDouble() - 0.5) * 0.5f;
            float currentAngle = (float)Math.Atan2(directionToPlayer.Y, directionToPlayer.X);
            float newAngle = currentAngle + spreadAngle;

            Vector2 shotDirection = new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle));

            float bossLaserSpeed = playerSpeed * laserSpeedMultiplier * GameConstants.BossLaserSpeedMultiplier;
            Vector2 spawnOffset = shotDirection * (bossTextureHeight / 2f + 10f);
            Vector2 laserPosition = bossPosition + spawnOffset;

            return new Laser
            {
                Position = laserPosition,
                Velocity = shotDirection * bossLaserSpeed,
                IsEnemy = true
            };
        }

        /// <summary>
        /// Calculates the enemy spawn interval based on current score.
        /// </summary>
        /// <param name="currentScore">Current player score</param>
        /// <returns>Spawn interval in seconds</returns>
        public float CalculateEnemySpawnInterval(int currentScore)
        {
            int steps = currentScore / GameConstants.ScoreDifficultyStep;
            float interval = GameConstants.BaseEnemySpawnInterval - steps * GameConstants.ScoreIntervalReductionPerStep;
            return Math.Max(interval, GameConstants.MinEnemySpawnInterval);
        }

        /// <summary>
        /// Calculates the asteroid spawn interval based on current score.
        /// </summary>
        /// <param name="currentScore">Current player score</param>
        /// <returns>Spawn interval in seconds</returns>
        public float CalculateAsteroidSpawnInterval(int currentScore)
        {
            float interval = GameConstants.AsteroidSpawnBaseInterval - (currentScore * GameConstants.AsteroidSpawnDifficultyFactor);
            return Math.Max(interval, GameConstants.AsteroidSpawnMinInterval);
        }

        /// <summary>
        /// Determines which boss texture should be used based on bosses defeated.
        /// </summary>
        /// <param name="bossesDefeated">Number of bosses already defeated</param>
        /// <returns>Index of the boss texture to use</returns>
        public int GetBossTextureIndex(int bossesDefeated)
        {
            if (bossesDefeated == 0)
            {
                return 2; // First boss uses texture index 2 (green)
            }
            else
            {
                // Cycle through boss textures: boss1, boss2, boss4
                int[] bossSequence = { 0, 1, 3 }; // indices for red, blue, purple
                return bossSequence[(bossesDefeated - 1) % bossSequence.Length];
            }
        }

        /// <summary>
        /// Checks if a boss should spawn based on enemies destroyed and bosses defeated.
        /// </summary>
        /// <param name="enemiesDestroyed">Total enemies destroyed</param>
        /// <param name="bossesDefeated">Total bosses defeated</param>
        /// <returns>True if a boss should spawn</returns>
        public bool ShouldSpawnBoss(int enemiesDestroyed, int bossesDefeated)
        {
            if (bossesDefeated == 0)
            {
                return enemiesDestroyed >= GameConstants.FirstBossSpawnThreshold;
            }
            else
            {
                int threshold = GameConstants.FirstBossSpawnThreshold + bossesDefeated * GameConstants.BossSpawnInterval;
                return enemiesDestroyed >= threshold;
            }
        }
    }
}
