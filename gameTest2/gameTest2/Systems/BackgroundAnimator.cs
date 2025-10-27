using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using gameTest2.Config;

namespace gameTest2.Systems
{
    /// <summary>
    /// Manages animated background rendering.
    /// Handles frame timing, animation playback, and fallback rendering.
    /// </summary>
    public class BackgroundAnimator
    {
        private readonly List<Texture2D> _frames;
        private readonly Texture2D _fallbackTexture;
        private int _currentFrameIndex;
        private float _frameTimer;

        /// <summary>
        /// Gets the current frame index in the animation.
        /// </summary>
        public int CurrentFrameIndex => _currentFrameIndex;

        /// <summary>
        /// Gets whether the background has animation frames loaded.
        /// </summary>
        public bool HasAnimation => _frames.Count > 1;

        /// <summary>
        /// Initializes the background animator with frames and a fallback texture.
        /// </summary>
        /// <param name="frames">List of animation frames (can be empty)</param>
        /// <param name="fallbackTexture">Fallback texture to use if no frames are available</param>
        public BackgroundAnimator(List<Texture2D> frames, Texture2D fallbackTexture)
        {
            _frames = frames ?? new List<Texture2D>();
            _fallbackTexture = fallbackTexture;
            _currentFrameIndex = 0;
            _frameTimer = 0f;
        }

        /// <summary>
        /// Updates the animation timer and advances frames as needed.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update in seconds</param>
        public void Update(float deltaTime)
        {
            if (!HasAnimation)
                return;

            _frameTimer += deltaTime;
            
            // Advance frames while timer exceeds frame duration
            while (_frameTimer >= GameConstants.BgFrameDuration)
            {
                _frameTimer -= GameConstants.BgFrameDuration;
                _currentFrameIndex = (_currentFrameIndex + 1) % _frames.Count;
            }
        }

        /// <summary>
        /// Draws the current background frame or fallback texture.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw with</param>
        /// <param name="destinationRectangle">Rectangle defining where to draw the background</param>
        public void Draw(SpriteBatch spriteBatch, Rectangle destinationRectangle)
        {
            if (_frames.Count > 0)
            {
                // Draw current animation frame
                spriteBatch.Draw(_frames[_currentFrameIndex], destinationRectangle, Color.White);
            }
            else if (_fallbackTexture != null)
            {
                // Draw fallback texture
                spriteBatch.Draw(_fallbackTexture, destinationRectangle, Color.Black);
            }
        }

        /// <summary>
        /// Resets the animation to the first frame.
        /// </summary>
        public void Reset()
        {
            _currentFrameIndex = 0;
            _frameTimer = 0f;
        }

        /// <summary>
        /// Gets the current frame texture, or null if no frames are available.
        /// </summary>
        public Texture2D GetCurrentFrame()
        {
            if (_frames.Count > 0)
                return _frames[_currentFrameIndex];
            return _fallbackTexture;
        }
    }
}
