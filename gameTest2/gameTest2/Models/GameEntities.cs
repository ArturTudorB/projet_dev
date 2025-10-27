using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gameTest2.Models
{
    /// <summary>
    /// Types of power-up buffs that can drop in the game
    /// </summary>
    public enum BuffType 
    { 
        Speed,      // Increases movement speed by 50%
        FireRate,   // Doubles fire rate
        MultiShot,  // Fires 3 lasers in a spread
        Shield,     // Adds 3 damage absorption charges
        Heal        // Restores 2 HP
    }

    /// <summary>
    /// Represents a laser projectile fired by player or enemies
    /// </summary>
    public struct Laser
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public bool IsEnemy;
    }

    /// <summary>
    /// Represents an enemy ship with AI behavior
    /// </summary>
    public struct Enemy
    {
        public Vector2 Position;
        public float NextShotTime;
        public float Speed;
        public int TextureIndex;
        
        // Death animation state
        public bool IsDying;
        public bool Visible;
        public float BlinkTimer;
        public float BlinkInterval;
        public int BlinkCount;
        public int BlinkToggleTarget;
        
        // Horizontal movement state
        public float HorizontalDir;
        public float HorizontalSpeed;
        public float HorizontalSwitchTimer;
        public float HorizontalSwitchInterval;
    }

    /// <summary>
    /// Represents an asteroid obstacle with multi-hit health
    /// </summary>
    public struct Asteroid
    {
        public Texture2D Texture;
        public Vector2 Position;
        public Vector2 Velocity;
        public int Hp;
        
        // Death animation state
        public bool IsDying;
        public bool Visible;
        public float BlinkTimer;
        public float BlinkInterval;
        public int BlinkCount;
        public int BlinkToggleTarget;
    }

    /// <summary>
    /// Represents a collectible power-up buff
    /// </summary>
    public struct BuffPickup
    {
        public BuffType Type;
        public Vector2 Position;
        public float Lifetime;      // Time before buff expires
        public float FloatOffset;   // Horizontal wobble animation offset
    }
}
