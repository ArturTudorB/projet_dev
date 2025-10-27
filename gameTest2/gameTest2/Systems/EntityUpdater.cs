using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using gameTest2.Models;
using gameTest2.Config;

namespace gameTest2.Systems
{
    /// <summary>
    /// Handles all entity update logic including movement, AI, and collision responses.
    /// Separates entity behavior from the main game loop for better organization and testability.
    /// </summary>
    public class EntityUpdater
    {
        private readonly CollisionDetector _collisionDetector;
        private readonly Random _random;
        private readonly int _screenWidth;
        private readonly int _screenHeight;

        /// <summary>
        /// Initializes the entity updater with required dependencies.
        /// </summary>
        /// <param name="collisionDetector">Collision detection system</param>
        /// <param name="screenWidth">Width of the game screen</param>
        /// <param name="screenHeight">Height of the game screen</param>
        public EntityUpdater(CollisionDetector collisionDetector, int screenWidth, int screenHeight)
        {
            _collisionDetector = collisionDetector;
            _random = new Random();
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        /// <summary>
        /// Updates all enemies including movement, AI, collision, and death animations.
        /// </summary>
        public void UpdateEnemies(
            List<Enemy> enemies,
            Texture2D[] enemyTextures,
            float dt,
            int scoreInt,
            float totalPlayTime,
            Vector2 playerPosition,
            Texture2D playerTexture,
            Action<Vector2> onEnemyShoot,
            Action<int> onPlayerDamage,
            Action onPlayerDeath,
            ref int shieldCharges)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var en = enemies[i];
                en.Position.Y += en.Speed * dt;

                if (!en.IsDying)
                {
                    // Horizontal movement logic
                    en.HorizontalSwitchTimer += dt;
                    if (en.HorizontalSwitchTimer >= en.HorizontalSwitchInterval)
                    {
                        en.HorizontalSwitchTimer -= en.HorizontalSwitchInterval;
                        en.HorizontalDir = -en.HorizontalDir;
                    }

                    // Apply horizontal movement with difficulty scaling
                    float horiz = en.HorizontalSpeed;
                    float scale = 1f + MathF.Min(scoreInt / GameConstants.EnemySpeedScoreDivisor, GameConstants.EnemyMaxSpeedMultiplier);
                    en.Position.X += en.HorizontalDir * horiz * scale * dt;

                    // Keep enemies within horizontal bounds
                    float left = GameConstants.EnemyHorizontalEdgeMargin;
                    float right = _screenWidth - GameConstants.EnemyHorizontalEdgeMargin;
                    if (en.Position.X < left)
                    {
                        en.Position.X = left;
                        en.HorizontalDir = 1;
                        en.HorizontalSwitchTimer = 0f;
                    }
                    else if (en.Position.X > right)
                    {
                        en.Position.X = right;
                        en.HorizontalDir = -1;
                        en.HorizontalSwitchTimer = 0f;
                    }
                }

                // Check collision with player
                if (!en.IsDying && _collisionDetector.CheckEnemyHitsPlayer(en, enemyTextures[en.TextureIndex], playerPosition, playerTexture))
                {
                    en.IsDying = true;
                    en.Visible = false;
                    en.BlinkTimer = 0f;
                    en.BlinkInterval = GameConstants.EnemyBlinkInterval;
                    en.BlinkCount = 0;
                    en.BlinkToggleTarget = GameConstants.EnemyBlinkToggleTarget;

                    // Apply damage with shield check
                    if (shieldCharges > 0)
                    {
                        shieldCharges--;
                    }
                    else
                    {
                        onPlayerDamage(GameConstants.EnemyCollisionDamage);
                    }
                }

                // Handle death animation
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
                        enemies.RemoveAt(i);
                        continue;
                    }
                    enemies[i] = en;
                    continue;
                }

                // Remove if off screen
                if (en.Position.Y - enemyTextures[en.TextureIndex].Height / 2f > _screenHeight + 50)
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                // Enemy shooting logic
                if (totalPlayTime >= en.NextShotTime)
                {
                    onEnemyShoot(en.Position);
                    
                    // Calculate next shot time with difficulty scaling
                    float diff = (float)Math.Clamp(1.0 - (scoreInt * 0.000025), 0.4, 1.0);
                    float minShot = GameConstants.EnemyBaseShotMin * diff;
                    float maxShot = GameConstants.EnemyBaseShotMax * diff;
                    en.NextShotTime = totalPlayTime + (float)(_random.NextDouble() * (maxShot - minShot) + minShot);
                }

                enemies[i] = en;
            }
        }

        /// <summary>
        /// Updates all asteroids including movement, collision, and death animations.
        /// </summary>
        public void UpdateAsteroids(
            List<Asteroid> asteroids,
            float dt,
            Vector2 playerPosition,
            Texture2D playerTexture,
            Action<int> onPlayerDamage,
            Action onPlayerDeath,
            ref int shieldCharges)
        {
            for (int i = asteroids.Count - 1; i >= 0; i--)
            {
                var a = asteroids[i];
                a.Position += a.Velocity * dt;

                // Check collision with player
                if (!a.IsDying && _collisionDetector.CheckAsteroidHitsPlayer(a, playerPosition, playerTexture))
                {
                    a.IsDying = true;
                    a.Visible = false;
                    a.BlinkTimer = 0f;
                    a.BlinkInterval = GameConstants.AsteroidBlinkInterval;
                    a.BlinkCount = 0;
                    a.BlinkToggleTarget = GameConstants.AsteroidBlinkToggleTarget;

                    // Apply damage with shield check
                    if (shieldCharges > 0)
                    {
                        shieldCharges--;
                    }
                    else
                    {
                        onPlayerDamage(GameConstants.AsteroidCollisionDamage);
                    }
                }

                // Handle death animation
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
                        asteroids.RemoveAt(i);
                        continue;
                    }
                    asteroids[i] = a;
                    continue;
                }

                // Remove if off screen
                if (a.Position.Y - a.Texture.Width / 2f > _screenHeight + 60)
                {
                    asteroids.RemoveAt(i);
                    continue;
                }

                asteroids[i] = a;
            }
        }

        /// <summary>
        /// Updates boss movement, AI, and collision.
        /// </summary>
        public void UpdateBoss(
            ref Vector2 bossPosition,
            ref float bossMovementTimer,
            ref float bossHorizontalDir,
            float bossHorizontalSpeed,
            Texture2D bossTexture,
            float dt,
            Vector2 playerPosition,
            Texture2D playerTexture,
            bool bossActive,
            Action<int> onPlayerDamage,
            Action onPlayerDeath,
            ref int shieldCharges)
        {
            // Move boss down towards middle of screen
            float halfScreenHeight = _screenHeight / 2f;
            if (bossPosition.Y < halfScreenHeight)
            {
                bossPosition.Y += GameConstants.BossSpeed * dt;
                if (bossPosition.Y > halfScreenHeight)
                    bossPosition.Y = halfScreenHeight;
            }

            // Horizontal movement
            bossMovementTimer += dt;
            if (bossMovementTimer >= GameConstants.BossMovementChangeInterval)
            {
                bossMovementTimer -= GameConstants.BossMovementChangeInterval;
                bossHorizontalDir = -bossHorizontalDir;
            }

            bossPosition.X += bossHorizontalDir * bossHorizontalSpeed * dt;

            // Restrict boss to central zone
            float restrictedZoneWidth = _screenWidth * GameConstants.BossMovementZoneWidthPercent;
            float centerX = _screenWidth / 2f;
            float leftBound = centerX - (restrictedZoneWidth / 2f);
            float rightBound = centerX + (restrictedZoneWidth / 2f);

            if (bossPosition.X < leftBound)
            {
                bossPosition.X = leftBound;
                bossHorizontalDir = 1f;
                bossMovementTimer = 0f;
            }
            else if (bossPosition.X > rightBound)
            {
                bossPosition.X = rightBound;
                bossHorizontalDir = -1f;
                bossMovementTimer = 0f;
            }

            // Check collision with player
            if (_collisionDetector.CheckBossHitsPlayer(bossActive, bossPosition, bossTexture, playerPosition, playerTexture))
            {
                // Apply damage with shield check
                if (shieldCharges > 0)
                {
                    shieldCharges--;
                }
                else
                {
                    onPlayerDamage(GameConstants.BossCollisionDamage);
                }

                // Push boss back slightly
                bossPosition.Y = Math.Max(bossPosition.Y - 50, halfScreenHeight - 100);
            }
        }

        /// <summary>
        /// Updates boss shooting behavior with burst fire patterns.
        /// </summary>
        public void UpdateBossShooting(
            ref float bossShootTimer,
            ref int bossShotsFired,
            ref float bossBurstTimer,
            ref bool bossIsBursting,
            float dt,
            Action onFireShot)
        {
            bossShootTimer += dt;

            if (!bossIsBursting && bossShootTimer >= GameConstants.BossShootInterval)
            {
                bossIsBursting = true;
                bossShotsFired = 0;
                bossBurstTimer = 0f;
                bossShootTimer = 0f;
            }

            if (bossIsBursting)
            {
                bossBurstTimer += dt;

                if (bossBurstTimer >= GameConstants.BossBurstShotInterval && bossShotsFired < GameConstants.BossBurstSize)
                {
                    onFireShot();
                    bossShotsFired++;
                    bossBurstTimer = 0f;
                }

                if (bossShotsFired >= GameConstants.BossBurstSize)
                {
                    bossIsBursting = false;
                }
            }
        }

        /// <summary>
        /// Updates all lasers including movement and collision detection.
        /// </summary>
        public void UpdateLasers(
            List<Laser> lasers,
            Texture2D laserTexture,
            float dt,
            bool bossActive,
            Vector2 bossPosition,
            Texture2D bossTexture,
            ref float bossHp,
            ref bool isBossActive,
            ref int bossesDefeated,
            ref int enemiesDestroyed,
            Action<Vector2> onBuffDrop,
            List<Enemy> enemies,
            Texture2D[] enemyTextures,
            List<Asteroid> asteroids,
            Vector2 playerPosition,
            Texture2D playerTexture,
            Action<int> onPlayerDamage,
            Action onPlayerDeath,
            ref int shieldCharges,
            Func<Vector2, bool> isOffScreen)
        {
            for (int i = lasers.Count - 1; i >= 0; i--)
            {
                var l = lasers[i];
                l.Position += l.Velocity * dt;
                lasers[i] = l;
                bool removed = false;

                if (!l.IsEnemy)
                {
                    // Boss collision with player lasers
                    if (_collisionDetector.CheckLaserHitsBoss(bossActive, l.Position, laserTexture, bossPosition, bossTexture))
                    {
                        bossHp--;
                        lasers.RemoveAt(i);
                        removed = true;

                        // Check if boss is defeated
                        if (bossHp <= 0)
                        {
                            isBossActive = false;
                            bossesDefeated++;
                            enemiesDestroyed += 5;

                            // Buff drop always occurs on boss defeat
                            onBuffDrop(bossPosition);
                        }

                        if (removed) continue;
                    }

                    // Check enemies
                    for (int ei = 0; ei < enemies.Count; ei++)
                    {
                        var en = enemies[ei];
                        if (en.IsDying) continue;

                        if (_collisionDetector.CheckLaserHitsEnemy(l.Position, laserTexture, en, enemyTextures[en.TextureIndex]))
                        {
                            en.IsDying = true;
                            en.Visible = false;
                            en.BlinkTimer = 0f;
                            en.BlinkInterval = 0.15f;
                            en.BlinkCount = 0;
                            en.BlinkToggleTarget = 4;
                            enemies[ei] = en;
                            enemiesDestroyed++;

                            // Remove laser
                            lasers.RemoveAt(i);
                            removed = true;

                            // Chance to drop buff
                            onBuffDrop(en.Position);

                            break;
                        }
                    }
                    if (removed) continue;

                    // Check asteroids
                    for (int ai = 0; ai < asteroids.Count; ai++)
                    {
                        var a = asteroids[ai];
                        if (a.IsDying) continue;

                        if (_collisionDetector.CheckLaserHitsAsteroid(l.Position, laserTexture, a))
                        {
                            a.Hp--;
                            if (a.Hp <= 0)
                            {
                                a.IsDying = true;
                                a.Visible = false;
                                a.BlinkTimer = 0f;
                                a.BlinkInterval = 0.18f;
                                a.BlinkCount = 0;
                                a.BlinkToggleTarget = 4;

                                // Chance to drop buff
                                onBuffDrop(a.Position);
                            }
                            asteroids[ai] = a;
                            lasers.RemoveAt(i);
                            removed = true;
                            break;
                        }
                    }
                }
                else
                {
                    // Enemy laser hitting player
                    if (_collisionDetector.CheckLaserHitsPlayer(l.Position, laserTexture, playerPosition, playerTexture))
                    {
                        lasers.RemoveAt(i);
                        removed = true;

                        // Apply damage with shield check
                        if (shieldCharges > 0)
                        {
                            shieldCharges--;
                        }
                        else
                        {
                            onPlayerDamage(1);
                        }
                        continue;
                    }
                }

                if (removed) continue;
                if (isOffScreen(l.Position))
                    lasers.RemoveAt(i);
            }
        }

        /// <summary>
        /// Updates all buff pickups including movement, animation, and collection.
        /// </summary>
        public void UpdateBuffPickups(
            List<BuffPickup> buffPickups,
            float dt,
            Vector2 playerPosition,
            Action<BuffType> onBuffCollected)
        {
            for (int i = buffPickups.Count - 1; i >= 0; i--)
            {
                var buff = buffPickups[i];
                buff.Lifetime -= dt;

                // Make buffs fall downward
                buff.Position.Y += GameConstants.BuffFallSpeed * dt;

                // Floating animation (horizontal wobble)
                buff.FloatOffset = MathF.Sin(buff.Lifetime * 3f) * GameConstants.BuffFloatAmplitude;

                // Check for pickup
                float dist = Vector2.Distance(playerPosition, buff.Position);
                if (dist < GameConstants.BuffPickupRadius)
                {
                    onBuffCollected(buff.Type);
                    buffPickups.RemoveAt(i);
                    continue;
                }

                // Remove if buff goes off screen or expires
                if (buff.Lifetime <= 0f || buff.Position.Y > _screenHeight + 50)
                {
                    buffPickups.RemoveAt(i);
                    continue;
                }

                buffPickups[i] = buff;
            }
        }
    }
}
