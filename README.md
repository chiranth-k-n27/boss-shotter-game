# 🎯 3D Mobile Shooter: Modular Weapon Crafting & Boss FSM

A high-end 3D Mobile Shooter game prototype (similar to *Free Fire / PUBG Mobile*) featuring real-time modular weapon crafting, Boss state machine AI, segmented hitboxes, multi-level progressive scaling, and mid-battle challenge rewards.

---

## 🎮 Play Live in Browser (Mobile & PC)

👉 **[Click Here to Play Live on Mobile or PC Browser](https://chiranth-k-n27.github.io/boss-shotter-game/)**

No installation required! Open the link above on your mobile phone or PC browser to play instantly with full touch joysticks and 3D graphics.

---

## 🔥 Key Gameplay Features

### 1. Mobile & PC Hybrid Controls
- **Mobile Touch Controls**: Left-side touch virtual joystick for 3D movement, right-side touch drag for camera look, and thumb action buttons.
- **PC Keyboard & Mouse**: WASD movement, mouse camera look, Left-Click to fire, Right-Click for ADS zoom, `R` to reload, and `🎯 FACE BOSS` camera lock-on.

### 2. Real-Time Modular Weapon Bench
- **Transform Sockets**: `[BarrelSocket]`, `[ScopeSocket]`, `[StockSocket]`, `[MagazineSocket]`.
- **Dynamic Gun Naming Engine**: Automatically classifies your custom weapon combinations into titles like *"Heavy Plasma Cannon"*, *"Rapid CQB Submachine Gun"*, and *"Tactical Battle Rifle"*.
- **Live Stat Modifiers**: Real-time comparison bars for Damage, Fire Rate, Recoil, Bullet Velocity, and Magazine Capacity.

### 3. Boss FSM AI & Segmented Hitboxes
- **Finite State Machine**: Dynamic state cycling (`Patrol/Approach` ➔ `Telegraphed Attack Windup` ➔ `Phase 2 Enrage`).
- **Segmented Hitboxes**:
  - 🟡 **Head (Weak Point)**: `2.5x` Critical Damage (Yellow popups).
  - ⚪ **Body**: `1.0x` Standard Damage (White popups).
  - ⬛ **Chest Armor**: `0.5x` Reduced Damage (Gray popups).
- **Telegraphed Attack**: Boss stops, charges an expanding red ring on the floor, and slams.
- **Phase 2 Enrage (at 50% HP)**: Boss turns glowing crimson red and gains `+60%` movement speed.
- **Boss Tracking**: Floating overhead `⚠️ BOSS TITAN` marker tag and distance HUD.

### 4. Multi-Level Progressive Scaling
- Defeating a Boss clears the level and automatically spawns the next level Boss titan with scaled HP, speed, and damage!

### 5. Mid-Battle Challenges & Overdrive Power-Ups
- Random mid-combat challenges (*"Land 3 Headshots!"*).
- Completing challenges grants **OVERDRIVE 2X DAMAGE** (+100% damage boost) or **INFINITE AMMO**!

---

## 📁 Repository Structure

```
├── index.html                   # WebGL Standalone 3D Game Engine (Playable in Browser)
├── README.md                    # Project Documentation & Gameplay Guide
└── Assets/Scripts/
    ├── Interfaces/CoreInterfaces.cs  # IDamageable, IInputProvider, IWeapon, IAttachmentHolder, IBossAI
    ├── Events/GameEvents.cs          # Decoupled C# Actions for UI, Boss, and Combat
    ├── Data/                         # Attachment & Weapon ScriptableObjects
    ├── Weapon/WeaponAssembler.cs     # Dynamic 3D socket mounting & stat calculator
    ├── Input/MobileTouchInput.cs     # Touch Joystick & Drag Look controller
    ├── Player/PlayerShooter.cs       # Movement, ADS FOV lerp, and Raycast shooting
    ├── Boss/                         # Boss FSM AI, Health, and Segmented Hitboxes
    ├── UI/                           # Floating Damage numbers, HUD & Crafting Bench UI
    ├── Level/LevelManager.cs         # Progressive Level Scaling engine
    ├── Challenge/ChallengeManager.cs # Mid-battle challenge generator & rewards
    └── Bootstrap/GameSceneBootstrapper.cs # Zero-setup auto scene generator
```
