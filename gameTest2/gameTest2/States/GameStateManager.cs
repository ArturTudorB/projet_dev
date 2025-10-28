using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace gameTest2.States
{
    public class GameStateManager
    {
        private IGameState _currentState;
        private IGameState _pendingState;
        public IGameState CurrentState => _currentState;
        public bool HasPendingTransition => _pendingState != null;
        public GameStateManager(IGameState initialState)
        {
            _currentState = initialState;
            _currentState?.Enter();
        }

        public void ChangeState(IGameState newState)
        {
            if (newState == null)
                throw new ArgumentNullException(nameof(newState));

            _pendingState = newState;
        }

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

        public void Draw(SpriteBatch spriteBatch)
        {
            _currentState?.Draw(spriteBatch);
        }

        public T GetCurrentStateAs<T>() where T : class, IGameState
        {
            return _currentState as T;
        }

        public bool IsInState<T>() where T : class, IGameState
        {
            return _currentState is T;
        }
    }
}
