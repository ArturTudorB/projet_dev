using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using gameTest2.Config;

namespace gameTest2.Models
{
    public class Player
    {
        public Vector2 Position { get; set; }
        public Vector2 Facing { get; set; }
        public Texture2D Texture { get; private set; }
        public float Speed { get; private set; }
        
        public int Health { get; private set; }
        public bool IsDead => Health <= 0;
        
        public float ShootCooldownTimer { get; private set; }
        
        public float SpeedBuffTimer { get; private set; }
        public float FireRateBuffTimer { get; private set; }
        public float MultiShotBuffTimer { get; private set; }
        public int ShieldCharges { get; private set; }
        
        public bool HasSpeedBuff => SpeedBuffTimer > 0f;
        public bool HasFireRateBuff => FireRateBuffTimer > 0f;
        public bool HasMultiShotBuff => MultiShotBuffTimer > 0f;
        public bool HasShield => ShieldCharges > 0;
        
        public float CurrentSpeed => HasSpeedBuff ? Speed * GameConstants.SpeedBuffMultiplier : Speed;
        
        public float CurrentShootCooldown => HasFireRateBuff 
            ? GameConstants.ShootCooldownSeconds * GameConstants.FireRateBuffMultiplier 
            : GameConstants.ShootCooldownSeconds;
        
        public Player(Texture2D texture, Vector2 startPosition)
        {
            Texture = texture;
            Position = startPosition;
            Facing = new Vector2(0, -1);
            Speed = GameConstants.PlayerSpeed;
            Health = GameConstants.PlayerStartingHp;
            ShootCooldownTimer = 0f;
            
            SpeedBuffTimer = 0f;
            FireRateBuffTimer = 0f;
            MultiShotBuffTimer = 0f;
            ShieldCharges = 0;
        }
        
        public void Update(float deltaTime)
        {
            if (SpeedBuffTimer > 0f) SpeedBuffTimer -= deltaTime;
            if (FireRateBuffTimer > 0f) FireRateBuffTimer -= deltaTime;
            if (MultiShotBuffTimer > 0f) MultiShotBuffTimer -= deltaTime;
            
            if (ShootCooldownTimer > 0f) ShootCooldownTimer -= deltaTime;
        }
        
        public void Move(Vector2 direction, float deltaTime)
        {
            if (direction.LengthSquared() > 0)
            {
                Vector2 movement = direction * CurrentSpeed * deltaTime;
                Position += movement;
                Facing = direction;
            }
        }
        
        public void ClampToScreen(int screenWidth, int screenHeight)
        {
            float halfWidth = Texture.Width / 2f;
            float halfHeight = Texture.Height / 2f;
            
            Position = new Vector2(
                MathHelper.Clamp(Position.X, halfWidth, screenWidth - halfWidth),
                MathHelper.Clamp(Position.Y, halfHeight, screenHeight - halfHeight)
            );
        }
        
        public bool CanShoot()
        {
            return ShootCooldownTimer <= 0f;
        }
        
        public void ResetShootCooldown()
        {
            ShootCooldownTimer = CurrentShootCooldown;
        }
        
        public bool TakeDamage(int damage)
        {
            if (ShieldCharges > 0)
            {
                ShieldCharges--;
                return true;
            }
            
            Health -= damage;
            if (Health < 0) Health = 0;
            return false;
        }
        
        public void ApplyBuff(BuffType buffType)
        {
            switch (buffType)
            {
                case BuffType.Speed:
                    SpeedBuffTimer = GameConstants.BuffDuration;
                    break;
                case BuffType.FireRate:
                    FireRateBuffTimer = GameConstants.BuffDuration;
                    break;
                case BuffType.MultiShot:
                    MultiShotBuffTimer = GameConstants.BuffDuration;
                    break;
                case BuffType.Shield:
                    ShieldCharges = System.Math.Min(
                        ShieldCharges + GameConstants.ShieldChargesPerBuff, 
                        GameConstants.ShieldMaxCharges);
                    break;
                case BuffType.Heal:
                    Health = System.Math.Min(
                        Health + GameConstants.HealBuffAmount, 
                        GameConstants.PlayerMaxHp);
                    break;
            }
        }
        
        public void SetShieldCharges(int charges)
        {
            ShieldCharges = charges;
        }
        
        public void Reset(Vector2 startPosition)
        {
            Position = startPosition;
            Facing = new Vector2(0, -1);
            Health = GameConstants.PlayerStartingHp;
            ShootCooldownTimer = 0f;
            
            SpeedBuffTimer = 0f;
            FireRateBuffTimer = 0f;
            MultiShotBuffTimer = 0f;
            ShieldCharges = 0;
        }
        
        public void SetTexture(Texture2D newTexture)
        {
            Texture = newTexture;
        }
    }
}
