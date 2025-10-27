using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using gameTest2.Models;

namespace gameTest2.Systems
{
    /// <summary>
    /// Handles all collision detection in the game.
    /// Provides clean, testable methods for checking intersections between game entities.
    /// </summary>
    public class CollisionDetector
    {
        /// <summary>
        /// Checks if an enemy collides with the player.
        /// </summary>
        /// <param name="enemy">The enemy to check</param>
        /// <param name="enemyTexture">The texture used for the enemy (determines collision bounds)</param>
        /// <param name="playerPosition">Current player position</param>
        /// <param name="playerTexture">The player's texture (determines collision bounds)</param>
        /// <returns>True if collision detected</returns>
        public bool CheckEnemyHitsPlayer(Enemy enemy, Texture2D enemyTexture, Vector2 playerPosition, Texture2D playerTexture)
        {
            var enemyRect = new Rectangle(
                (int)(enemy.Position.X - enemyTexture.Width / 2f),
                (int)(enemy.Position.Y - enemyTexture.Height / 2f),
                enemyTexture.Width,
                enemyTexture.Height);
            
            var playerRect = new Rectangle(
                (int)(playerPosition.X - playerTexture.Width / 2f),
                (int)(playerPosition.Y - playerTexture.Height / 2f),
                playerTexture.Width,
                playerTexture.Height);
            
            return enemyRect.Intersects(playerRect);
        }

        /// <summary>
        /// Checks if an asteroid collides with the player.
        /// </summary>
        /// <param name="asteroid">The asteroid to check</param>
        /// <param name="playerPosition">Current player position</param>
        /// <param name="playerTexture">The player's texture (determines collision bounds)</param>
        /// <returns>True if collision detected</returns>
        public bool CheckAsteroidHitsPlayer(Asteroid asteroid, Vector2 playerPosition, Texture2D playerTexture)
        {
            var asteroidRect = new Rectangle(
                (int)(asteroid.Position.X - asteroid.Texture.Width / 2f),
                (int)(asteroid.Position.Y - asteroid.Texture.Height / 2f),
                asteroid.Texture.Width,
                asteroid.Texture.Height);
            
            var playerRect = new Rectangle(
                (int)(playerPosition.X - playerTexture.Width / 2f),
                (int)(playerPosition.Y - playerTexture.Height / 2f),
                playerTexture.Width,
                playerTexture.Height);
            
            return asteroidRect.Intersects(playerRect);
        }

        /// <summary>
        /// Checks if a laser projectile collides with the player.
        /// </summary>
        /// <param name="laserPosition">Position of the laser</param>
        /// <param name="laserTexture">The laser's texture (determines collision bounds)</param>
        /// <param name="playerPosition">Current player position</param>
        /// <param name="playerTexture">The player's texture (determines collision bounds)</param>
        /// <returns>True if collision detected</returns>
        public bool CheckLaserHitsPlayer(Vector2 laserPosition, Texture2D laserTexture, Vector2 playerPosition, Texture2D playerTexture)
        {
            var playerRect = new Rectangle(
                (int)(playerPosition.X - playerTexture.Width / 2f),
                (int)(playerPosition.Y - playerTexture.Height / 2f),
                playerTexture.Width,
                playerTexture.Height);
            
            var laserRect = new Rectangle(
                (int)(laserPosition.X - laserTexture.Width / 2f),
                (int)(laserPosition.Y - laserTexture.Height / 2f),
                laserTexture.Width,
                laserTexture.Height);
            
            return playerRect.Intersects(laserRect);
        }

        /// <summary>
        /// Checks if the boss collides with the player.
        /// </summary>
        /// <param name="bossActive">Whether the boss is currently active</param>
        /// <param name="bossPosition">Position of the boss</param>
        /// <param name="bossTexture">The boss's texture (determines collision bounds, can be null)</param>
        /// <param name="playerPosition">Current player position</param>
        /// <param name="playerTexture">The player's texture (determines collision bounds)</param>
        /// <returns>True if collision detected</returns>
        public bool CheckBossHitsPlayer(bool bossActive, Vector2 bossPosition, Texture2D bossTexture, Vector2 playerPosition, Texture2D playerTexture)
        {
            if (!bossActive || bossTexture == null) 
                return false;
            
            var bossRect = new Rectangle(
                (int)(bossPosition.X - bossTexture.Width / 2f),
                (int)(bossPosition.Y - bossTexture.Height / 2f),
                bossTexture.Width,
                bossTexture.Height);
            
            var playerRect = new Rectangle(
                (int)(playerPosition.X - playerTexture.Width / 2f),
                (int)(playerPosition.Y - playerTexture.Height / 2f),
                playerTexture.Width,
                playerTexture.Height);
            
            return bossRect.Intersects(playerRect);
        }

        /// <summary>
        /// Checks if a laser projectile collides with the boss.
        /// </summary>
        /// <param name="bossActive">Whether the boss is currently active</param>
        /// <param name="laserPosition">Position of the laser</param>
        /// <param name="laserTexture">The laser's texture (determines collision bounds)</param>
        /// <param name="bossPosition">Position of the boss</param>
        /// <param name="bossTexture">The boss's texture (determines collision bounds, can be null)</param>
        /// <returns>True if collision detected</returns>
        public bool CheckLaserHitsBoss(bool bossActive, Vector2 laserPosition, Texture2D laserTexture, Vector2 bossPosition, Texture2D bossTexture)
        {
            if (!bossActive || bossTexture == null) 
                return false;
            
            var bossRect = new Rectangle(
                (int)(bossPosition.X - bossTexture.Width / 2f),
                (int)(bossPosition.Y - bossTexture.Height / 2f),
                bossTexture.Width,
                bossTexture.Height);
            
            var laserRect = new Rectangle(
                (int)(laserPosition.X - laserTexture.Width / 2f),
                (int)(laserPosition.Y - laserTexture.Height / 2f),
                laserTexture.Width,
                laserTexture.Height);
            
            return bossRect.Intersects(laserRect);
        }

        /// <summary>
        /// Checks if a laser projectile collides with an enemy.
        /// </summary>
        /// <param name="laserPosition">Position of the laser</param>
        /// <param name="laserTexture">The laser's texture (determines collision bounds)</param>
        /// <param name="enemy">The enemy to check</param>
        /// <param name="enemyTexture">The texture used for the enemy (determines collision bounds)</param>
        /// <returns>True if collision detected</returns>
        public bool CheckLaserHitsEnemy(Vector2 laserPosition, Texture2D laserTexture, Enemy enemy, Texture2D enemyTexture)
        {
            var enemyRect = new Rectangle(
                (int)(enemy.Position.X - enemyTexture.Width / 2f),
                (int)(enemy.Position.Y - enemyTexture.Height / 2f),
                enemyTexture.Width,
                enemyTexture.Height);
            
            var laserRect = new Rectangle(
                (int)(laserPosition.X - laserTexture.Width / 2f),
                (int)(laserPosition.Y - laserTexture.Height / 2f),
                laserTexture.Width,
                laserTexture.Height);
            
            return enemyRect.Intersects(laserRect);
        }

        /// <summary>
        /// Checks if a laser projectile collides with an asteroid.
        /// </summary>
        /// <param name="laserPosition">Position of the laser</param>
        /// <param name="laserTexture">The laser's texture (determines collision bounds)</param>
        /// <param name="asteroid">The asteroid to check</param>
        /// <returns>True if collision detected</returns>
        public bool CheckLaserHitsAsteroid(Vector2 laserPosition, Texture2D laserTexture, Asteroid asteroid)
        {
            var asteroidRect = new Rectangle(
                (int)(asteroid.Position.X - asteroid.Texture.Width / 2f),
                (int)(asteroid.Position.Y - asteroid.Texture.Height / 2f),
                asteroid.Texture.Width,
                asteroid.Texture.Height);
            
            var laserRect = new Rectangle(
                (int)(laserPosition.X - laserTexture.Width / 2f),
                (int)(laserPosition.Y - laserTexture.Height / 2f),
                laserTexture.Width,
                laserTexture.Height);
            
            return asteroidRect.Intersects(laserRect);
        }
    }
}
