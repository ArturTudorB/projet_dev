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
                
                // Déplacement vertical de l'ennemi
                en.Position.Y += en.Speed * dt;

                if (!en.IsDying)
                {
                    // Gestion du changement de direction horizontale
                    en.HorizontalSwitchTimer += dt;
                    if (en.HorizontalSwitchTimer >= en.HorizontalSwitchInterval)
                    {
                        en.HorizontalSwitchTimer -= en.HorizontalSwitchInterval;
                        en.HorizontalDir = -en.HorizontalDir;
                    }

                    // Déplacement horizontal avec scaling de difficulté
                    float horiz = en.HorizontalSpeed;
                    float scale = 1f + MathF.Min(scoreInt / GameConstants.EnemySpeedScoreDivisor, GameConstants.EnemyMaxSpeedMultiplier);
                    en.Position.X += en.HorizontalDir * horiz * scale * dt;

                    // Garde les ennemis dans les limites de l'écran
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

                // Vérification de collision avec le joueur
                if (!en.IsDying && _collisionDetector.CheckEnemyHitsPlayer(en, enemyTextures[en.TextureIndex], playerPosition, playerTexture))
                {
                    // Début de l'animation de mort
                    en.IsDying = true;
                    en.Visible = false;
                    en.BlinkTimer = 0f;
                    en.BlinkInterval = GameConstants.EnemyBlinkInterval;
                    en.BlinkCount = 0;
                    en.BlinkToggleTarget = GameConstants.EnemyBlinkToggleTarget;

                    // Application des dégâts avec vérification du bouclier
                    if (shieldCharges > 0)
                    {
                        shieldCharges--;
                    }
                    else
                    {
                        onPlayerDamage(GameConstants.EnemyCollisionDamage);
                    }
                }

                // Gestion de l'animation de mort (clignotement)
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

                // Suppression si hors écran
                if (en.Position.Y - enemyTextures[en.TextureIndex].Height / 2f > _screenHeight + 50)
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                // Gestion des tirs de l'ennemi
                if (totalPlayTime >= en.NextShotTime)
                {
                    onEnemyShoot(en.Position);
                    
                    // Calcul du prochain tir avec scaling de difficulté
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
                
                // Déplacement de l'astéroïde
                a.Position += a.Velocity * dt;

                // Vérification de collision avec le joueur
                if (!a.IsDying && _collisionDetector.CheckAsteroidHitsPlayer(a, playerPosition, playerTexture))
                {
                    // Début de l'animation de mort
                    a.IsDying = true;
                    a.Visible = false;
                    a.BlinkTimer = 0f;
                    a.BlinkInterval = GameConstants.AsteroidBlinkInterval;
                    a.BlinkCount = 0;
                    a.BlinkToggleTarget = GameConstants.AsteroidBlinkToggleTarget;

                    // Application des dégâts avec vérification du bouclier
                    if (shieldCharges > 0)
                    {
                        shieldCharges--;
                    }
                    else
                    {
                        onPlayerDamage(GameConstants.AsteroidCollisionDamage);
                    }
                }

                // Gestion de l'animation de mort (clignotement)
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

                // Suppression si hors écran
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
            // Déplacement du boss vers le milieu de l'écran
            float halfScreenHeight = _screenHeight / 2f;
            if (bossPosition.Y < halfScreenHeight)
            {
                bossPosition.Y += GameConstants.BossSpeed * dt;
                if (bossPosition.Y > halfScreenHeight)
                    bossPosition.Y = halfScreenHeight;
            }

            // Changement de direction horizontale périodique
            bossMovementTimer += dt;
            if (bossMovementTimer >= GameConstants.BossMovementChangeInterval)
            {
                bossMovementTimer -= GameConstants.BossMovementChangeInterval;
                bossHorizontalDir = -bossHorizontalDir;
            }

            // Déplacement horizontal
            bossPosition.X += bossHorizontalDir * bossHorizontalSpeed * dt;

            // Restriction du boss à la zone centrale
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

            // Vérification de collision avec le joueur
            if (_collisionDetector.CheckBossHitsPlayer(bossActive, bossPosition, bossTexture, playerPosition, playerTexture))
            {
                // Application des dégâts avec vérification du bouclier
                if (shieldCharges > 0)
                {
                    shieldCharges--;
                }
                else
                {
                    onPlayerDamage(GameConstants.BossCollisionDamage);
                }

                // Repousse légèrement le boss
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

            // Démarrage d'une nouvelle rafale de tirs
            if (!bossIsBursting && bossShootTimer >= GameConstants.BossShootInterval)
            {
                bossIsBursting = true;
                bossShotsFired = 0;
                bossBurstTimer = 0f;
                bossShootTimer = 0f;
            }

            // Gestion de la rafale en cours
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
            Func<Vector2, bool> isOffScreen,
            Action<double> onScoreGain)
        {
            for (int i = lasers.Count - 1; i >= 0; i--)
            {
                var l = lasers[i];
                
                // Déplacement du laser
                l.Position += l.Velocity * dt;
                lasers[i] = l;
                bool removed = false;

                // Lasers du joueur
                if (!l.IsEnemy)
                {
                    // Collision avec le boss
                    if (_collisionDetector.CheckLaserHitsBoss(bossActive, l.Position, laserTexture, bossPosition, bossTexture))
                    {
                        bossHp--;
                        lasers.RemoveAt(i);
                        removed = true;

                        // Vérification si le boss est vaincu
                        if (bossHp <= 0)
                        {
                            isBossActive = false;
                            bossesDefeated++;
                            enemiesDestroyed += 5;

                            // Drop de buff garanti lors de la défaite du boss
                            onBuffDrop(bossPosition);
                        }

                        if (removed) continue;
                    }

                    // Collision avec les ennemis
                    for (int ei = 0; ei < enemies.Count; ei++)
                    {
                        var en = enemies[ei];
                        if (en.IsDying) continue;

                        if (_collisionDetector.CheckLaserHitsEnemy(l.Position, laserTexture, en, enemyTextures[en.TextureIndex]))
                        {
                            // Début de l'animation de mort de l'ennemi
                            en.IsDying = true;
                            en.Visible = false;
                            en.BlinkTimer = 0f;
                            en.BlinkInterval = 0.15f;
                            en.BlinkCount = 0;
                            en.BlinkToggleTarget = 4;
                            enemies[ei] = en;
                            enemiesDestroyed++;

                            // Attribution de points
                            onScoreGain(GameConstants.EnemyKillScore);

                            lasers.RemoveAt(i);
                            removed = true;

                            // Chance de drop de buff
                            onBuffDrop(en.Position);

                            break;
                        }
                    }
                    if (removed) continue;

                    // Collision avec les astéroïdes
                    for (int ai = 0; ai < asteroids.Count; ai++)
                    {
                        var a = asteroids[ai];
                        if (a.IsDying) continue;

                        if (_collisionDetector.CheckLaserHitsAsteroid(l.Position, laserTexture, a))
                        {
                            a.Hp--;
                            if (a.Hp <= 0)
                            {
                                // Début de l'animation de mort de l'astéroïde
                                a.IsDying = true;
                                a.Visible = false;
                                a.BlinkTimer = 0f;
                                a.BlinkInterval = 0.18f;
                                a.BlinkCount = 0;
                                a.BlinkToggleTarget = 4;

                                // Chance de drop de buff
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
                    // Lasers ennemis touchant le joueur
                    if (_collisionDetector.CheckLaserHitsPlayer(l.Position, laserTexture, playerPosition, playerTexture))
                    {
                        lasers.RemoveAt(i);
                        removed = true;

                        // Application des dégâts avec vérification du bouclier
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

                // Suppression si hors écran
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

                // Chute du buff vers le bas
                buff.Position.Y += GameConstants.BuffFallSpeed * dt;

                // Animation flottante (oscillation horizontale)
                buff.FloatOffset = MathF.Sin(buff.Lifetime * 3f) * GameConstants.BuffFloatAmplitude;

                // Vérification de la collecte
                float dist = Vector2.Distance(playerPosition, buff.Position);
                if (dist < GameConstants.BuffPickupRadius)
                {
                    onBuffCollected(buff.Type);
                    buffPickups.RemoveAt(i);
                    continue;
                }

                // Suppression si hors écran ou expiré
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
