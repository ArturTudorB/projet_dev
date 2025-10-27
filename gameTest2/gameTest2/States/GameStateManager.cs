using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace gameTest2.States
{
    /// <summary>
    /// Manages game state transitions using the State Pattern.
    /// Handles entering/exiting states and delegating update/draw calls.
    /// </summary>
    public class GameStateManager
    {
        private IGameState _currentState;
        private IGameState _pendingState;

        /// <summary>
        /// Gets the currently active state.
        /// </summary>
        public IGameState CurrentState => _currentState;

        /// <summary>
        /// Gets whether there is a pending state transition.
        /// </summary>
        public bool HasPendingTransition => _pendingState != null;

        /// <summary>
        /// Initializes the state manager with an initial state.
        /// </summary>
        /// <param name="initialState">The first state to activate</param>
        public GameStateManager(IGameState initialState)
        {
            _currentState = initialState;
            _currentState?.Enter();
        }

        /// <summary>
        /// Changes to a new game state.
        /// The transition will occur at the start of the next update cycle.
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        public void ChangeState(IGameState newState)
        {
            if (newState == null)
                throw new ArgumentNullException(nameof(newState));

            _pendingState = newState;
        }

        /// <summary>
        /// Updates the current state and processes any pending state transitions.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update</param>
        public void Update(float deltaTime)
        {
            // Process pending state transition at the start of the frame
            if (_pendingState != null)
            {
                _currentState?.Exit();
                _currentState = _pendingState;
                _pendingState = null;
                _currentState.Enter();
            }

            // Update the current state
            _currentState?.Update(deltaTime);
        }

        /// <summary>
        /// Draws the current state.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch for drawing (should already be begun)</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _currentState?.Draw(spriteBatch);
        }

        /// <summary>
        /// Gets the current state as a specific type.
        /// </summary>
        /// <typeparam name="T">The state type to cast to</typeparam>
        /// <returns>The current state cast to the specified type, or null if not that type</returns>
        public T GetCurrentStateAs<T>() where T : class, IGameState
        {
            return _currentState as T;
        }

        /// <summary>
        /// Checks if the current state is of a specific type.
        /// </summary>
        /// <typeparam name="T">The state type to check</typeparam>
        /// <returns>True if the current state is of the specified type</returns>
        public bool IsInState<T>() where T : class, IGameState
        {
            return _currentState is T;
        }
    }
}
