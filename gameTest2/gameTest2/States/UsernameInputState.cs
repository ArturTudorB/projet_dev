using Microsoft.Xna.Framework.Graphics;
using gameTest2.Systems;
using gameTest2.Rendering;
using gameTest2.Config;

namespace gameTest2.States
{
    /// <summary>
    /// Username input state - handles player name entry before starting the game.
    /// </summary>
    public class UsernameInputState : BaseGameState
    {
        private readonly MenuRenderer _menuRenderer;
        private readonly GameStateManager _stateManager;
        
        private string _playerName = "";
        private bool _showCursor = true;
        private float _cursorBlinkTimer = 0f;

        public UsernameInputState(
            Game1 game,
            InputManager input,
            AudioManager audio,
            MenuRenderer menuRenderer,
            GameStateManager stateManager)
            : base(game, input, audio)
        {
            _menuRenderer = menuRenderer;
            _stateManager = stateManager;
        }

        public override void Enter()
        {
            Audio.EnsureMainMenuMusicPlaying();
            _playerName = "";
        }

        public override void Update(float deltaTime)
        {
            // Cursor blinking
            _cursorBlinkTimer += deltaTime;
            if (_cursorBlinkTimer >= GameConstants.CursorBlinkInterval)
            {
                _cursorBlinkTimer -= GameConstants.CursorBlinkInterval;
                _showCursor = !_showCursor;
            }

            // Handle text input
            var pressedKeys = Input.GetPressedKeys();
            foreach (var key in pressedKeys)
            {
                if (!Input.PreviousKeyboard.IsKeyDown(key))
                {
                    HandleTextInput(key);
                }
            }

            // Start game when Enter is pressed
            if (Input.IsKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                if (!string.IsNullOrWhiteSpace(_playerName))
                {
                    Audio.StopMainMenuMusic();
                    _stateManager.ChangeState(Game.CreatePlayingState(_stateManager, _playerName));
                }
            }

            // Back to main menu
            if (Input.IsBackJustPressed())
            {
                _stateManager.ChangeState(Game.CreateMainMenuState(_stateManager));
            }
        }

        private void HandleTextInput(Microsoft.Xna.Framework.Input.Keys key)
        {
            if (_playerName.Length >= GameConstants.MaxUsernameLength) return;

            if (key >= Microsoft.Xna.Framework.Input.Keys.A && key <= Microsoft.Xna.Framework.Input.Keys.Z)
            {
                char letter = (char)('a' + (key - Microsoft.Xna.Framework.Input.Keys.A));
                _playerName += letter;
            }
            else if (key >= Microsoft.Xna.Framework.Input.Keys.D0 && key <= Microsoft.Xna.Framework.Input.Keys.D9)
            {
                char number = (char)('0' + (key - Microsoft.Xna.Framework.Input.Keys.D0));
                _playerName += number;
            }
            else if (key == Microsoft.Xna.Framework.Input.Keys.Space && _playerName.Length > 0 && !_playerName.EndsWith(" "))
            {
                _playerName += " ";
            }
            else if (key == Microsoft.Xna.Framework.Input.Keys.Back && _playerName.Length > 0)
            {
                _playerName = _playerName[..^1];
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _menuRenderer.DrawUsernameInput(_playerName, _showCursor);
        }
    }
}
