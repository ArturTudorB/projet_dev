using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaColor = Microsoft.Xna.Framework.Color;
using gameTest2.Models;
using gameTest2.Config;
using System;
using System.Collections.Generic;

namespace gameTest2.Rendering
{
    /// <summary>
    /// Handles all rendering for the playing game state.
    /// Draws entities, player, HUD, and visual effects.
    /// </summary>
    public class GameplayRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _uiPixel;

        public GameplayRenderer(SpriteBatch spriteBatch, SpriteFont font, Texture2D uiPixel)
        {
            _spriteBatch = spriteBatch;
            _font = font;
            _uiPixel = uiPixel;
        }

        /// <summary>
        /// Renders the complete playing game state.
        /// </summary>
        public void DrawPlaying(
            List<Asteroid> asteroids,
            Texture2D[] enemyTextures,
            List<Enemy> enemies,
            Texture2D[] buffTextures,
            List<BuffPickup> buffPickups,
            Texture2D laserTexture,
            List<Laser> lasers,
            Texture2D playerTexture,
            Vector2 playerPosition,
            int shieldCharges,
            float totalPlayTime,
            Texture2D[] bossTextures,
            bool bossActive,
            int currentBossTextureIndex,
            Vector2 bossPosition,
            int screenWidth,
            int scoreInt,
            int playerHp,
            string playerName,
            float bossHp,
            float speedBuffTimer,
            float fireRateBuffTimer,
            float multiShotBuffTimer)
        {
            // Draw asteroids
            DrawAsteroids(asteroids);

            // Draw enemies
            DrawEnemies(enemies, enemyTextures);

            // Draw buff pickups
            DrawBuffPickups(buffPickups, buffTextures);

            // Draw lasers
            DrawLasers(lasers, laserTexture);

            // Draw player with shield effect
            DrawPlayer(playerTexture, playerPosition, shieldCharges, totalPlayTime);

            // Draw boss
            if (bossActive && bossTextures[currentBossTextureIndex] != null)
            {
                DrawBoss(bossTextures[currentBossTextureIndex], bossPosition);
            }

            // Draw HUD
            DrawHUD(screenWidth, scoreInt, playerHp, shieldCharges, playerName, bossActive, bossHp);

            // Draw active buff icons
            DrawActiveBuffIcons(screenWidth, speedBuffTimer, fireRateBuffTimer, multiShotBuffTimer, buffTextures);
        }

        private void DrawAsteroids(List<Asteroid> asteroids)
        {
            foreach (var asteroid in asteroids)
            {
                if (!asteroid.Visible) continue;
                var tex = asteroid.Texture;
                _spriteBatch.Draw(tex, asteroid.Position, null, XnaColor.White, 0f,
                    new Vector2(tex.Width / 2f, tex.Height / 2f), 1f, SpriteEffects.None, 0f);
            }
        }

        private void DrawEnemies(List<Enemy> enemies, Texture2D[] enemyTextures)
        {
            foreach (var enemy in enemies)
            {
                if (!enemy.Visible) continue;
                var tex = enemyTextures[enemy.TextureIndex];
                _spriteBatch.Draw(tex, enemy.Position, null, XnaColor.White, 0f,
                    new Vector2(tex.Width / 2f, tex.Height / 2f), 1f, SpriteEffects.None, 0f);
            }
        }

        private void DrawBuffPickups(List<BuffPickup> buffPickups, Texture2D[] buffTextures)
        {
            foreach (var buff in buffPickups)
            {
                var tex = buffTextures[(int)buff.Type];
                // Apply horizontal wobble effect
                Vector2 drawPos = buff.Position + new Vector2(buff.FloatOffset, 0);

                // Pulsing alpha effect for buffs about to expire
                float alpha = buff.Lifetime < 2f ? (MathF.Sin(buff.Lifetime * 10f) * 0.5f + 0.5f) : 1f;
                XnaColor tint = XnaColor.White * alpha;

                _spriteBatch.Draw(tex, drawPos, null, tint, 0f,
                    new Vector2(tex.Width / 2f, tex.Height / 2f), 1f, SpriteEffects.None, 0f);
            }
        }

        private void DrawLasers(List<Laser> lasers, Texture2D laserTexture)
        {
            foreach (var laser in lasers)
            {
                float rotation = (float)Math.Atan2(laser.Velocity.Y, laser.Velocity.X) + MathHelper.PiOver2;
                var tint = laser.IsEnemy ? XnaColor.OrangeRed : XnaColor.White;
                _spriteBatch.Draw(laserTexture, laser.Position, null, tint, rotation,
                    new Vector2(laserTexture.Width / 2f, laserTexture.Height / 2f), 1f,
                    SpriteEffects.None, 0f);
            }
        }

        private void DrawPlayer(Texture2D playerTexture, Vector2 playerPosition, int shieldCharges, float totalPlayTime)
        {
            XnaColor playerTint = XnaColor.White;
            if (shieldCharges > 0)
            {
                // Pulsing blue tint when shield is active
                float pulse = MathF.Sin(totalPlayTime * 5f) * 0.3f + 0.7f;
                playerTint = XnaColor.Lerp(XnaColor.White, XnaColor.Cyan, pulse * 0.5f);
            }

            _spriteBatch.Draw(playerTexture, playerPosition, null, playerTint, 0f,
                new Vector2(playerTexture.Width / 2f, playerTexture.Height / 2f), 1f,
                SpriteEffects.None, 0f);
        }

        private void DrawBoss(Texture2D bossTexture, Vector2 bossPosition)
        {
            _spriteBatch.Draw(bossTexture, bossPosition, null, XnaColor.White, 0f,
                new Vector2(bossTexture.Width / 2f, bossTexture.Height / 2f), 1f,
                SpriteEffects.None, 0f);
        }

        private void DrawHUD(int screenWidth, int scoreInt, int playerHp, int shieldCharges, string playerName, bool bossActive, float bossHp)
        {
            if (_font == null) return;

            string text = $"SCORE: {scoreInt}  HP: {playerHp}";
            if (shieldCharges > 0)
                text += $"  BOUCLIER: {shieldCharges}";
            text += $"  Joueur: {playerName}";

            if (bossActive)
            {
                text += $"  BOSS HP: {bossHp}/{GameConstants.BossMaxHp}";
            }

            var size = _font.MeasureString(text);
            var pos = new Vector2(screenWidth - 10 - size.X, 10);
            _spriteBatch.DrawString(_font, text, pos, XnaColor.White);
        }

        private void DrawActiveBuffIcons(int screenWidth, float speedBuffTimer, float fireRateBuffTimer, float multiShotBuffTimer, Texture2D[] buffTextures)
        {
            if (_font == null) return;

            float buffIconY = 50f;
            float buffIconX = screenWidth - 45f;

            if (speedBuffTimer > 0f)
            {
                DrawBuffIcon(BuffType.Speed, new Vector2(buffIconX, buffIconY), speedBuffTimer, buffTextures);
                buffIconY += 40f;
            }
            if (fireRateBuffTimer > 0f)
            {
                DrawBuffIcon(BuffType.FireRate, new Vector2(buffIconX, buffIconY), fireRateBuffTimer, buffTextures);
                buffIconY += 40f;
            }
            if (multiShotBuffTimer > 0f)
            {
                DrawBuffIcon(BuffType.MultiShot, new Vector2(buffIconX, buffIconY), multiShotBuffTimer, buffTextures);
            }
        }

        private void DrawBuffIcon(BuffType type, Vector2 position, float timeRemaining, Texture2D[] buffTextures)
        {
            var tex = buffTextures[(int)type];

            // Draw semi-transparent background
            var bgRect = new Rectangle((int)(position.X - 18), (int)(position.Y - 18), 36, 36);
            _spriteBatch.Draw(_uiPixel, bgRect, new XnaColor(0, 0, 0, 128));

            // Draw buff icon
            _spriteBatch.Draw(tex, position, null, XnaColor.White, 0f,
                new Vector2(tex.Width / 2f, tex.Height / 2f), 0.8f, SpriteEffects.None, 0f);

            // Draw timer
            string timeText = ((int)MathF.Ceiling(timeRemaining)).ToString();
            var textSize = _font.MeasureString(timeText);
            var textPos = new Vector2(position.X - textSize.X / 2f, position.Y + 20);
            _spriteBatch.DrawString(_font, timeText, textPos, XnaColor.Yellow);
        }
    }
}
