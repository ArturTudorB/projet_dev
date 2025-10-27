using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace gameTest2.States
{
    /// <summary>
    /// Interface for all game states.
    /// Implements the State Pattern for clean state management and transitions.
    /// </summary>
    public interface IGameState
    {
        /// <summary>
        /// Called when this state becomes active.
        /// Use for initialization and setup specific to this state.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called when this state is about to become inactive.
        /// Use for cleanup and saving state if needed.
        /// </summary>
        void Exit();

        /// <summary>
        /// Updates the state logic.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update</param>
        void Update(float deltaTime);

        /// <summary>
        /// Renders the state.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch for drawing (already begun)</param>
        void Draw(SpriteBatch spriteBatch);
    }
}
