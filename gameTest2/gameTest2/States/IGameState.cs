using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace gameTest2.States
{
    public interface IGameState
    {
        void Enter();
        void Exit();
        void Update(float deltaTime);
        void Draw(SpriteBatch spriteBatch);
    }
}
