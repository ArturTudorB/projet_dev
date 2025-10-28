# Game Architecture Documentation

## System Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              GAME1 (Main Entry)                             │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │ Core Loop: Update() → Draw()                                           │ │
│  │ - Manages GraphicsDeviceManager                                        │ │
│  │ - Initializes all systems                                              │ │
│  │ - Delegates to GameStateManager                                        │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
                                       │
                    ┌──────────────────┴──────────────────┐
                    │                                     │
                    ▼                                     ▼
    ┌───────────────────────────┐         ┌──────────────────────────┐
    │   CORE SYSTEMS            │         │   RENDERING SYSTEMS      │
    ├───────────────────────────┤         ├──────────────────────────┤
    │ • InputManager            │         │ • GameplayRenderer       │
    │ • AudioManager            │         │ • MenuRenderer           │
    │ • CollisionDetector       │         │ • BackgroundAnimator     │
    │ • EntitySpawner           │         └──────────────────────────┘
    │ • EntityUpdater           │
    │ • ContentLoader           │
    │ • ScoreDatabase           │
    └───────────────────────────┘
                    │
                    ▼
    ┌─────────────────────────────────────────────────────────────┐
    │              GAME STATE MANAGER (State Pattern)             │
    │  ┌───────────────────────────────────────────────────────┐  │
    │  │ _currentState → Update() → Draw()                     │  │
    │  │ _pendingState (for safe transitions)                  │  │
    │  └───────────────────────────────────────────────────────┘  │
    └─────────────────────────────────────────────────────────────┘
                    │
        ┌───────────┴───────────┬──────────────┬────────────┐
        ▼                       ▼              ▼            ▼
┌───────────────┐    ┌─────────────────┐  ┌─────────┐  ┌──────────────┐
│ MainMenuState │    │ UsernameInput   │  │ Playing │  │ GameOverState│
│               │    │ State           │  │ State   │  │              │
│ [Leaderboard] │───▶│                 │─▶│         │─▶│              │
│ [Start Game]  │    │ Enter Name      │  │ ACTIVE  │  │ Final Score  │
│ [Quit]        │    │                 │  │GAMEPLAY │  │              │
└───────────────┘    └─────────────────┘  └─────────┘  └──────────────┘
        │                                     │              │
        │                                     │              │
        ▼                                     │              ▼
┌───────────────┐                             │         [Return to Menu]
│ Leaderboard   │                             │
│ State         │                             │
│               │                             │
│ Top 10 Scores │                             │
└───────────────┘                             │
                                              │
                    ┌─────────────────────────┘
                    ▼
    ┌─────────────────────────────────────────────────────────┐
    │                 PLAYING STATE GAMEPLAY                  │
    │  ┌───────────────────────────────────────────────────┐  │
    │  │                 Game Entities                     │  │
    │  │  ┌────────┐  ┌────────┐  ┌─────────┐  ┌───────┐   │  │
    │  │  │ Player │  │ Enemies│  │Asteroids│  │ Lasers│   │  │
    │  │  └────────┘  └────────┘  └─────────┘  └───────┘   │  │
    │  │       │           │            │            │     │  │
    │  │  ┌────────┐  ┌────────┐  ┌─────────┐  ┌───────┐   │  │
    │  │  │  Boss  │  │ Buffs  │  │ Shields │  │ Score │   │  │
    │  │  └────────┘  └────────┘  └─────────┘  └───────┘   │  │
    │  └───────────────────────────────────────────────────┘  │
    └─────────────────────────────────────────────────────────┘
```

## Game Flow

### 1. Initialization Phase
```
Game1() Constructor
    ↓
Initialize()
    ↓
LoadContent()
    ↓
Game Loop Starts
```

### 2. Main Game Loop
```
┌─────────────────────────────┐
│ Update(GameTime)            │
│  1. InputManager.Update()   │
│  2. Fullscreen Check        │
│  3. Background Update       │
│  4. StateManager.Update()   │
└──────────┬──────────────────┘
           │
           ▼
┌─────────────────────────────┐
│ Draw(GameTime)              │
│  1. Clear Screen            │
│  2. Begin SpriteBatch       │
│  3. Draw Background         │
│  4. StateManager.Draw()     │
│  5. End SpriteBatch         │
└─────────────────────────────┘
```

## State Transitions

```
        ┌──────────────┐
        │  Main Menu   │ ◄─────────────────────────┐
        └──────┬───────┘                           │
               │ [Start]                           │
               ▼                                   │
        ┌──────────────┐                           │
        │  Username    │                           │
        │    Input     │                           │
        └──────┬───────┘                           │
               │ [Enter]                           │
               ▼                                   │
        ┌──────────────┐                           │
        │   Playing    │                           │
        │   (Active    │                           │
        │   Gameplay)  │                           │
        └──────┬───────┘                           │
               │ [Player Dies]                     │
               ▼                                   │
        ┌──────────────┐                           │
        │  Game Over   │                           │
        │              │                           │
        └──────┬───────┘                           │
               │ [Click Anywhere]                  │
               └───────────────────────────────────┘

        Main Menu ──[Press L]──► Leaderboard ──[Any Key]──► Main Menu
```

## Core Systems Architecture

### Input System
```
┌──────────────────────────────┐
│     InputManager             │
├──────────────────────────────┤
│ - Current Keyboard State     │
│ - Previous Keyboard State    │
│ - Current Mouse State        │
│ - Previous Mouse State       │
│ - Current GamePad State      │
│ - Previous GamePad State     │
├──────────────────────────────┤
│ + Update()                   │
│ + IsKeyPressed()             │
│ + IsKeyJustPressed()         │
│ + IsMouseButtonClicked()     │
│ + IsRectangleClicked()       │
│ + GetMovementDirection()     │
└──────────────────────────────┘
```

### Audio System
```
┌──────────────────────────────┐
│     AudioManager             │
├──────────────────────────────┤
│ - Button Click Sound         │
│ - Main Menu Music            │
│ - Audio Enabled Flag         │
├──────────────────────────────┤
│ + PlayMainMenuMusic()        │
│ + StopMainMenuMusic()        │
│ + PlayButtonClickSound()     │
│ + EnsureMusicPlaying()       │
└──────────────────────────────┘
```

### Collision System
```
┌──────────────────────────────┐
│   CollisionDetector          │
├──────────────────────────────┤
│ + CheckEnemyHitsPlayer()     │
│ + CheckAsteroidHitsPlayer()  │
│ + CheckLaserHitsPlayer()     │
│ + CheckBossHitsPlayer()      │
│ + CheckLaserHitsBoss()       │
│ + CheckLaserHitsEnemy()      │
│ + CheckLaserHitsAsteroid()   │
└──────────────────────────────┘
```

### Entity Management
```
┌──────────────────────────────┐
│     EntitySpawner            │
├──────────────────────────────┤
│ + SpawnEnemy()               │
│ + SpawnAsteroid()            │
│ + SpawnBoss()                │
│ + TrySpawnBuffDrop()         │
│ + SpawnLaserInDirection()    │
│ + SpawnLaserTowards()        │
│ + SpawnBossLaser()           │
│ + CalculateSpawnIntervals()  │
└──────────────────────────────┘
         │
         ▼
┌──────────────────────────────┐
│     EntityUpdater            │
├──────────────────────────────┤
│ + UpdateLasers()             │
│ + UpdateEnemies()            │
│ + UpdateAsteroids()          │
│ + UpdateBoss()               │
│ + UpdateBuffs()              │
│ + HandleCollisions()         │
└──────────────────────────────┘
```


### Player
```
┌──────────────────────────┐
│        Player            │
├──────────────────────────┤
│ Position: Vector2        │
│ Velocity: Vector2        │
│ Facing: Vector2          │
│ HP: int (max 3)          │
│ Shield: int (max 3)      │
│ Speed: float             │
│ Shoot Cooldown: float    │
│ Active Buffs: List       │
├──────────────────────────┤
│ + Update()               │
│ + Move()                 │
│ + TakeDamage()           │
│ + ApplyBuff()            │
└──────────────────────────┘
```

### Enemy
```
┌──────────────────────────┐
│         Enemy            │
├──────────────────────────┤
│ Position: Vector2        │
│ Speed: float             │
│ TextureIndex: int        │
│ NextShotTime: float      │
│ IsDying: bool            │
│ BlinkTimer: float        │
│ HorizontalDir: float     │
├──────────────────────────┤
│ + MoveTowardsPlayer()    │
│ + Shoot()                │
│ + Die()                  │
└──────────────────────────┘
```

### Boss
```
┌──────────────────────────┐
│          Boss            │
├──────────────────────────┤
│ Position: Vector2        │
│ HP: int (40-100)         │
│ TextureIndex: int        │
│ HorizontalDir: float     │
│ NextShotTime: float      │
│ IsActive: bool           │
├──────────────────────────┤
│ + MoveHorizontally()     │
│ + ShootSpread()          │
│ + TakeDamage()           │
│ + Die()                  │
└──────────────────────────┘
```

### Buff Types
```
┌─────────────────────────────────────┐
│          Buff System                │
├─────────────────────────────────────┤
│ 1. Speed Buff (Green)               │
│    - Increases movement speed       │
│    - Duration: 15 seconds           │
│                                     │
│ 2. Fire Rate Buff (Red)             │
│    - Faster shooting                │
│    - Duration: 15 seconds           │
│                                     │
│ 3. Multi-shot Buff (Yellow)         │
│    - 3 lasers with spread           │
│    - Duration: 15 seconds           │
│                                     │
│ 4. Shield Buff (Blue)               │
│    - +1 Shield charge               │
│    - Instant effect                 │
│                                     │
│ 5. Health Buff (Purple)             │
│    - +1 HP (max 3)                  │
│    - Instant effect                 │
└─────────────────────────────────────┘
```

## Database Schema

```
┌──────────────────────────────┐
│      ScoreDatabase           │
├──────────────────────────────┤
│ Table: Scores                │
│  - Id (INTEGER PRIMARY KEY)  │
│  - PlayerName (TEXT)         │
│  - Score (INTEGER)           │
│  - Timestamp (TEXT)          │
├──────────────────────────────┤
│ + SaveScore()                │
│ + GetTopScores(n)            │
│ + ClearAllScores()           │
└──────────────────────────────┘
```



