using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace gameTest2.Systems
{
    /// <summary>
    /// Centralized input handling system.
    /// Tracks input state and provides clean methods for detecting input events.
    /// Eliminates repetitive input checking code throughout the game.
    /// </summary>
    public class InputManager
    {
        private KeyboardState _currentKeyboard;
        private KeyboardState _previousKeyboard;
        private MouseState _currentMouse;
        private MouseState _previousMouse;

        /// <summary>
        /// Gets the current keyboard state.
        /// </summary>
        public KeyboardState CurrentKeyboard => _currentKeyboard;

        /// <summary>
        /// Gets the current mouse state.
        /// </summary>
        public MouseState CurrentMouse => _currentMouse;

        /// <summary>
        /// Gets the previous keyboard state.
        /// </summary>
        public KeyboardState PreviousKeyboard => _previousKeyboard;

        /// <summary>
        /// Gets the previous mouse state.
        /// </summary>
        public MouseState PreviousMouse => _previousMouse;

        /// <summary>
        /// Initializes the input manager with current input states.
        /// </summary>
        public InputManager()
        {
            _currentKeyboard = Keyboard.GetState();
            _currentMouse = Mouse.GetState();
            _previousKeyboard = _currentKeyboard;
            _previousMouse = _currentMouse;
        }

        /// <summary>
        /// Updates input states. Should be called once per frame at the start of Update.
        /// </summary>
        public void Update()
        {
            _previousKeyboard = _currentKeyboard;
            _previousMouse = _currentMouse;
            _currentKeyboard = Keyboard.GetState();
            _currentMouse = Mouse.GetState();
        }

        // ===== KEYBOARD INPUT METHODS =====

        /// <summary>
        /// Checks if a key was just pressed (down this frame, up last frame).
        /// </summary>
        public bool IsKeyJustPressed(Keys key)
        {
            return _currentKeyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if a key is currently held down.
        /// </summary>
        public bool IsKeyDown(Keys key)
        {
            return _currentKeyboard.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if a key was just released (up this frame, down last frame).
        /// </summary>
        public bool IsKeyJustReleased(Keys key)
        {
            return !_currentKeyboard.IsKeyDown(key) && _previousKeyboard.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if any of the specified keys were just pressed.
        /// </summary>
        public bool IsAnyKeyJustPressed(params Keys[] keys)
        {
            foreach (var key in keys)
            {
                if (IsKeyJustPressed(key))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if any of the specified keys are currently held down.
        /// </summary>
        public bool IsAnyKeyDown(params Keys[] keys)
        {
            foreach (var key in keys)
            {
                if (IsKeyDown(key))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all keys that are currently pressed.
        /// </summary>
        public Keys[] GetPressedKeys()
        {
            return _currentKeyboard.GetPressedKeys();
        }

        // ===== MOUSE INPUT METHODS =====

        /// <summary>
        /// Checks if the left mouse button was just clicked (pressed this frame, released last frame).
        /// </summary>
        public bool IsLeftMouseButtonJustClicked()
        {
            return _currentMouse.LeftButton == ButtonState.Pressed &&
                   _previousMouse.LeftButton == ButtonState.Released;
        }

        /// <summary>
        /// Checks if the left mouse button is currently held down.
        /// </summary>
        public bool IsLeftMouseButtonDown()
        {
            return _currentMouse.LeftButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Checks if the right mouse button was just clicked.
        /// </summary>
        public bool IsRightMouseButtonJustClicked()
        {
            return _currentMouse.RightButton == ButtonState.Pressed &&
                   _previousMouse.RightButton == ButtonState.Released;
        }

        /// <summary>
        /// Gets the current mouse position as a Vector2.
        /// </summary>
        public Vector2 GetMousePosition()
        {
            return new Vector2(_currentMouse.X, _currentMouse.Y);
        }

        /// <summary>
        /// Checks if the mouse position is within a specified rectangle.
        /// </summary>
        public bool IsMouseInRectangle(Rectangle rectangle)
        {
            return rectangle.Contains(_currentMouse.Position);
        }

        /// <summary>
        /// Checks if a rectangle was just clicked (mouse is in rectangle and left button just clicked).
        /// </summary>
        public bool IsRectangleJustClicked(Rectangle rectangle)
        {
            return IsLeftMouseButtonJustClicked() && IsMouseInRectangle(rectangle);
        }

        // ===== GAMEPAD INPUT METHODS =====

        /// <summary>
        /// Checks if the gamepad Back button is currently pressed.
        /// </summary>
        public bool IsGamePadBackPressed(PlayerIndex playerIndex = PlayerIndex.One)
        {
            return GamePad.GetState(playerIndex).Buttons.Back == ButtonState.Pressed;
        }

        // ===== COMBINED INPUT METHODS =====

        /// <summary>
        /// Checks if the fullscreen toggle was just pressed (Alt+Enter or F11).
        /// </summary>
        public bool IsFullscreenToggleJustPressed()
        {
            return (IsKeyDown(Keys.LeftAlt) && IsKeyJustPressed(Keys.Enter)) ||
                   IsKeyJustPressed(Keys.F11);
        }

        /// <summary>
        /// Checks if any "confirm" input was just pressed (Enter or Space).
        /// </summary>
        public bool IsConfirmJustPressed()
        {
            return IsAnyKeyJustPressed(Keys.Enter, Keys.Space);
        }

        /// <summary>
        /// Checks if the "back" input was just pressed (Escape).
        /// </summary>
        public bool IsBackJustPressed()
        {
            return IsKeyJustPressed(Keys.Escape);
        }

        // ===== MOVEMENT INPUT METHODS =====

        /// <summary>
        /// Gets the movement direction based on WASD or Arrow keys.
        /// Returns a normalized direction vector, or Vector2.Zero if no movement keys are pressed.
        /// </summary>
        public Vector2 GetMovementDirection()
        {
            Vector2 direction = Vector2.Zero;

            if (IsAnyKeyDown(Keys.W, Keys.Up)) direction.Y -= 1;
            if (IsAnyKeyDown(Keys.S, Keys.Down)) direction.Y += 1;
            if (IsAnyKeyDown(Keys.A, Keys.Left)) direction.X -= 1;
            if (IsAnyKeyDown(Keys.D, Keys.Right)) direction.X += 1;

            if (direction.LengthSquared() > 0)
                direction.Normalize();

            return direction;
        }
    }
}
