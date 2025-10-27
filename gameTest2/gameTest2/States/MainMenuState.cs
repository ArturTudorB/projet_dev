using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using gameTest2.Systems;
using gameTest2.Rendering;

namespace gameTest2.States
{
    /// <summary>
    /// Main menu state - handles menu navigation and options.
    /// </summary>
    public class MainMenuState : BaseGameState
    {
        private readonly MenuRenderer _menuRenderer;
        private readonly GameStateManager _stateManager;
        private readonly int _screenWidth;
        private readonly int _screenHeight;

        public MainMenuState(
            Game1 game,
            InputManager input,
            AudioManager audio,
            MenuRenderer menuRenderer,
            GameStateManager stateManager,
            int screenWidth,
            int screenHeight)
            : base(game, input, audio)
        {
            _menuRenderer = menuRenderer;
            _stateManager = stateManager;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        public override void Enter()
        {
            Audio.EnsureMainMenuMusicPlaying();
        }

        public override void Update(float deltaTime)
        {
            // Gamepad back button check for Xbox controller compatibility
            if (Input.IsGamePadBackPressed())
            {
                Game.Exit();
                return;
            }

            int bw = System.Math.Min(360, _screenWidth - 160);
            int bh = 64;
            int x = (_screenWidth - bw) / 2;
            int yStart = (int)(_screenHeight * 0.4f);
            int spacing = 18;

            var startRect = new Rectangle(x, yStart, bw, bh);
            var leaderboardRect = new Rectangle(x, yStart + bh + spacing, bw, bh);
            var quitRect = new Rectangle(x, yStart + 2 * (bh + spacing), bw, bh);

            // Check for start game input
            if (Input.IsConfirmJustPressed() || Input.IsRectangleJustClicked(startRect))
            {
                Audio.PlayButtonClickSound();
                _stateManager.ChangeState(Game.CreateUsernameInputState(_stateManager));
            }
            // Check for leaderboard input
            else if (Input.IsRectangleJustClicked(leaderboardRect) || Input.IsKeyJustPressed(Keys.L))
            {
                Audio.PlayButtonClickSound();
                Game.LoadTopScores();
                _stateManager.ChangeState(Game.CreateLeaderboardState(_stateManager));
            }
            // Check for quit input
            else if (Input.IsRectangleJustClicked(quitRect))
            {
                Audio.PlayButtonClickSound();
                Game.Exit();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _menuRenderer.DrawMainMenu(Input.CurrentMouse);
        }
    }
}
