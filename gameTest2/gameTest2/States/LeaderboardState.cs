using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using gameTest2.Systems;
using gameTest2.Rendering;

namespace gameTest2.States
{
    public class LeaderboardState : BaseGameState
    {
        private readonly MenuRenderer _menuRenderer;
        private readonly GameStateManager _stateManager;
        private readonly ScoreDatabase _scoreDatabase;
        private List<ScoreEntry> _topScores;

        public LeaderboardState(
            Game1 game,
            InputManager input,
            AudioManager audio,
            MenuRenderer menuRenderer,
            GameStateManager stateManager,
            ScoreDatabase scoreDatabase)
            : base(game, input, audio)
        {
            _menuRenderer = menuRenderer;
            _stateManager = stateManager;
            _scoreDatabase = scoreDatabase;
        }

        public override void Enter()
        {
            Audio.EnsureMainMenuMusicPlaying();
            _topScores = _scoreDatabase.GetTopScores(10);
        }

        public override void Update(float deltaTime)
        {
            if (Input.IsBackJustPressed() ||
                Input.IsConfirmJustPressed() ||
                Input.IsLeftMouseButtonJustClicked())
            {
                _stateManager.ChangeState(Game.CreateMainMenuState(_stateManager));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _menuRenderer.DrawLeaderboard(_topScores);
        }
    }
}
