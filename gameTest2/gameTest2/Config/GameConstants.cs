namespace gameTest2.Config
{
    /// <summary>
    /// Central configuration for all game balance and tuning values.
    /// Adjust these constants to tweak gameplay without touching game logic.
    /// </summary>
    public static class GameConstants
    {
        #region Player Configuration
        
        /// <summary>Player movement speed in pixels per second (base speed)</summary>
        public const float PlayerSpeed = 450f;
        
        /// <summary>Player starting health points</summary>
        public const int PlayerStartingHp = 5;
        
        /// <summary>Maximum health points the player can have</summary>
        public const int PlayerMaxHp = 10;
        
        /// <summary>Cooldown between player shots in seconds</summary>
        public const float ShootCooldownSeconds = 0.25f;
        
        #endregion

        #region Laser Configuration
        
        /// <summary>Multiplier applied to player speed for laser velocity</summary>
        public const float LaserSpeedMultiplier = 3f;
        
        /// <summary>Factor applied to enemy laser speed (relative to player lasers)</summary>
        public const float EnemyLaserSpeedFactor = 0.85f;
        
        /// <summary>Offset from entity center when spawning lasers</summary>
        public const float LaserSpawnOffset = 8f;
        
        /// <summary>Margin around screen edges for laser off-screen check</summary>
        public const int LaserOffScreenMargin = 32;
        
        #endregion

        #region Enemy Configuration
        
        /// <summary>Number of different enemy texture variants</summary>
        public const int EnemyTextureCount = 12;
        
        /// <summary>Base time between enemy spawns at game start (seconds)</summary>
        public const float BaseEnemySpawnInterval = 2.0f;
        
        /// <summary>Minimum time between enemy spawns (difficulty cap)</summary>
        public const float MinEnemySpawnInterval = 1f;
        
        /// <summary>Minimum time between enemy shots (affected by difficulty)</summary>
        public const float EnemyBaseShotMin = 2.0f;
        
        /// <summary>Maximum time between enemy shots (affected by difficulty)</summary>
        public const float EnemyBaseShotMax = 5.2f;
        
        /// <summary>Difficulty scaling factor for enemy shooting frequency</summary>
        public const float EnemyShotDifficultyFactor = 0.025f;
        
        /// <summary>Minimum horizontal movement speed for enemies</summary>
        public const float EnemyHorizontalSpeedMin = 10f;
        
        /// <summary>Maximum horizontal movement speed for enemies</summary>
        public const float EnemyHorizontalSpeedMax = 60f;
        
        /// <summary>Minimum time before enemy changes horizontal direction</summary>
        public const float EnemyHorizontalIntervalMin = 0.8f;
        
        /// <summary>Maximum time before enemy changes horizontal direction</summary>
        public const float EnemyHorizontalIntervalMax = 3.0f;
        
        /// <summary>Edge margin preventing enemies from moving too far horizontally</summary>
        public const float EnemyHorizontalEdgeMargin = 24f;
        
        /// <summary>Damage dealt to player on enemy collision</summary>
        public const int EnemyCollisionDamage = 2;
        
        /// <summary>Duration of enemy death blink animation (seconds)</summary>
        public const float EnemyBlinkInterval = 0.15f;
        
        /// <summary>Number of blinks during enemy death animation</summary>
        public const int EnemyBlinkToggleTarget = 4;
        
        #endregion

        #region Asteroid Configuration
        
        /// <summary>Number of different asteroid texture variants</summary>
        public const int AsteroidTextureCount = 2;
        
        /// <summary>Base time between asteroid spawns at game start (seconds)</summary>
        public const float AsteroidSpawnBaseInterval = 12f;
        
        /// <summary>Minimum time between asteroid spawns (difficulty cap)</summary>
        public const float AsteroidSpawnMinInterval = 4f;
        
        /// <summary>Difficulty scaling factor for asteroid spawn rate</summary>
        public const float AsteroidSpawnDifficultyFactor = 0.00025f;
        
        /// <summary>Maximum health points for asteroids (requires multiple hits)</summary>
        public const int AsteroidMaxHp = 2;
        
        /// <summary>Damage dealt to player on asteroid collision</summary>
        public const int AsteroidCollisionDamage = 3;
        
        /// <summary>Duration of asteroid death blink animation (seconds)</summary>
        public const float AsteroidBlinkInterval = 0.18f;
        
        /// <summary>Number of blinks during asteroid death animation</summary>
        public const int AsteroidBlinkToggleTarget = 4;
        
        #endregion

        #region Buff/Power-up Configuration
        
        /// <summary>Probability that a destroyed enemy/asteroid drops a buff (0.0 to 1.0)</summary>
        public const float BuffDropChance = 0.15f;
        
        /// <summary>Time before uncollected buff disappears (seconds)</summary>
        public const float BuffLifetime = 8f;
        
        /// <summary>Distance from player required to collect buff (pixels)</summary>
        public const float BuffPickupRadius = 40f;
        
        /// <summary>Speed at which buffs fall downward (pixels/second)</summary>
        public const float BuffFallSpeed = 150f;
        
        /// <summary>Duration of most timed buffs (Speed, FireRate, MultiShot)</summary>
        public const float BuffDuration = 10f;
        
        /// <summary>Speed multiplier when Speed buff is active</summary>
        public const float SpeedBuffMultiplier = 1.5f;
        
        /// <summary>Fire rate multiplier when FireRate buff is active (reduces cooldown)</summary>
        public const float FireRateBuffMultiplier = 0.5f;
        
        /// <summary>Angle spread for multi-shot buff (degrees)</summary>
        public const float MultiShotSpreadAngle = 15f;
        
        /// <summary>Number of shield charges granted by Shield buff</summary>
        public const int ShieldChargesPerBuff = 3;
        
        /// <summary>Maximum shield charges player can accumulate</summary>
        public const int ShieldMaxCharges = 5;
        
        /// <summary>Health points restored by Heal buff</summary>
        public const int HealBuffAmount = 2;
        
        /// <summary>Amplitude of horizontal wobble animation for buffs (pixels)</summary>
        public const float BuffFloatAmplitude = 8f;
        
        #endregion

        #region Boss Configuration
        
        /// <summary>Number of different boss texture variants</summary>
        public const int BossTextureCount = 4;
        
        /// <summary>Maximum and starting health points for boss</summary>
        public const float BossMaxHp = 25f;
        
        /// <summary>Vertical movement speed of boss (pixels/second)</summary>
        public const float BossSpeed = 120f;
        
        /// <summary>Horizontal movement speed of boss (pixels/second)</summary>
        public const float BossHorizontalSpeed = 180f;
        
        /// <summary>Time between boss direction changes (seconds)</summary>
        public const float BossMovementChangeInterval = 1.2f;
        
        /// <summary>Time between boss burst attacks (seconds)</summary>
        public const float BossShootInterval = 4.0f;
        
        /// <summary>Time between individual shots in a burst (seconds)</summary>
        public const float BossBurstShotInterval = 0.15f;
        
        /// <summary>Number of shots fired in each boss burst</summary>
        public const int BossBurstSize = 3;
        
        /// <summary>Damage dealt to player on boss collision</summary>
        public const int BossCollisionDamage = 5;
        
        /// <summary>Laser speed multiplier for boss projectiles</summary>
        public const float BossLaserSpeedMultiplier = 1.2f;
        
        /// <summary>Percentage of screen width boss is restricted to (0.0 to 1.0)</summary>
        public const float BossMovementZoneWidthPercent = 0.35f;
        
        /// <summary>Number of enemies destroyed before first boss spawns</summary>
        public const int FirstBossSpawnThreshold = 10;
        
        /// <summary>Number of enemies between subsequent boss spawns</summary>
        public const int BossSpawnInterval = 20;
        
        #endregion

        #region Score & Difficulty Configuration
        
        /// <summary>Score points earned per second of survival</summary>
        public const double ScorePerSecond = 100.0;
        
        /// <summary>Score points earned per enemy destroyed</summary>
        public const double EnemyKillScore = 300.0;
        
        /// <summary>Score threshold for each difficulty increase step</summary>
        public const int ScoreDifficultyStep = 3000;
        
        /// <summary>Enemy spawn interval reduction per difficulty step (seconds)</summary>
        public const float ScoreIntervalReductionPerStep = 0.2f;
        
        /// <summary>Score divisor for enemy speed scaling calculation</summary>
        public const float EnemySpeedScoreDivisor = 9000f;
        
        /// <summary>Maximum enemy speed multiplier from difficulty scaling</summary>
        public const float EnemyMaxSpeedMultiplier = 1.5f;
        
        /// <summary>Score divisor for enemy horizontal speed scaling</summary>
        public const float EnemyHorizontalSpeedScoreDivisor = 15000f;
        
        /// <summary>Maximum enemy horizontal speed multiplier</summary>
        public const float EnemyHorizontalMaxSpeedMultiplier = 1.2f;
        
        #endregion

        #region UI & Animation Configuration
        
        /// <summary>Maximum allowed characters in username input</summary>
        public const int MaxUsernameLength = 15;
        
        /// <summary>Time between cursor blinks in username input (seconds)</summary>
        public const float CursorBlinkInterval = 0.5f;
        
        /// <summary>Duration of each background animation frame (seconds)</summary>
        public const float BgFrameDuration = 0.03f;
        
        /// <summary>Number of background animation frames</summary>
        public const int BackgroundFrameCount = 33; // 0 to 32
        
        #endregion
    }
}
