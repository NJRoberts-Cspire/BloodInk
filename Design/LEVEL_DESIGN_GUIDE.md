# BloodInk — Level Design Guide

A practical reference for designing and building mission rooms in Godot 4.6.

---

## Table of Contents

1. [Room Structure Checklist](#1-room-structure-checklist)
2. [Step-by-Step: Creating a New Room](#2-step-by-step-creating-a-new-room)
3. [Building the Geometry](#3-building-the-geometry)
4. [Placing Guards](#4-placing-guards)
5. [Stealth Zones](#5-stealth-zones)
6. [Lighting](#6-lighting)
7. [Environmental Interactions](#7-environmental-interactions)
8. [Puzzles](#8-puzzles)
9. [Room Connections & Transitions](#9-room-connections--transitions)
10. [Mission Alert System](#10-mission-alert-system)
11. [Design Principles](#11-design-principles)
12. [Component Quick Reference](#12-component-quick-reference)

---

## 1. Room Structure Checklist

Every mission room needs these elements. Use this as a sign-off checklist before considering a room done.

```
□ Root Node2D with RoomManager script
□ At least one SpawnPoint (named "Default")
□ Floor/wall geometry (TileMap or MapBuilder)
□ At least one LevelTransition to connect to another room
□ MissionAlertManager (one per mission, on the mission root)
□ At least 2 patrol routes with guards
□ At least 2 distinct approach paths for the player
□ Shadow/cover zones along stealth paths
□ 1+ hiding spot per major corridor
□ Surface zones on floors that differ from stone
□ Light sources the player can interact with
□ Camera limits set (via CameraShake.SetLimits)
```

---

## 2. Step-by-Step: Creating a New Room

### 2.1 Create the Scene File

1. **File → New Scene** in Godot
2. Root node: **Node2D** — name it after the room (e.g., `Barracks`)
3. Save to `Scenes/Missions/<Kingdom>/<RoomName>.tscn`
   - Greenhold rooms go in `Scenes/Missions/Greenhold/`
4. Attach the **RoomManager** script (`Scripts/World/RoomManager.cs`)

### 2.2 Configure RoomManager

In the Inspector:
| Property | What to Set |
|----------|-------------|
| `RoomId` | Unique string like `"greenhold_barracks"` |
| `FadeInDuration` | `0.3` (default is fine) |
| `AmbientTension` | `0.0` for safe areas, `0.3–0.7` for mission areas |

### 2.3 Add Spawn Points

1. Add a **Marker2D** child, attach **SpawnPoint** script
2. **Name the node** after where the player comes from:
   - `Default` — first-time entry / fallback
   - `FromEast` — entering from the east door
   - `FromChapel` — entering from the Chapel scene
3. Set `FacingDirection` to the direction the player should face on entry

### 2.4 Add the Alert Manager (Mission Root Only)

On the **top-level mission scene** (not sub-rooms), add a **Node** child with the **MissionAlertManager** script. Only one per mission — it's a singleton.

---

## 3. Building the Geometry

### Option A: MapBuilder (Code-Based — Fast Prototyping)

Create a script on your room's root node and call `MapBuilder.Build()` in `_Ready()`:

```csharp
public override void _Ready()
{
    var map = new string[]
    {
        "################",
        "#..............#",
        "#..~~..........#",
        "#..~~....####..#",
        "#........#,,#..#",
        "#........#,,#..#",
        "#..####..####..#",
        "#..#ww#........#",
        "#..#ww#........#",
        "#..####..~~....#",
        "#..........~~..#",
        "################",
    };
    MapBuilder.Build(this, map);
}
```

**Tile key:**
| Char | Result | Collision? | Stealth Effect? |
|------|--------|-----------|----------------|
| `#` | Wall | Yes (Layer 1) | Blocks vision & noise raycasts |
| `.` | Stone floor | No | None |
| `w` | Wood floor | No | None (add SurfaceZone for noise) |
| `~` | Shadow | No | Creates functional ShadowZone |
| `,` | Grass | No | None (add SurfaceZone for noise) |
| `p` | Path | No | None |
| `c` | Carpet | No | None (add SurfaceZone for noise) |
| ` ` | Empty | No | Nothing rendered |

**Converting tile coords to world positions:**
```csharp
Vector2 guardPos = MapBuilder.TileToWorld(5, 3); // tile (5,3) → pixel center
```

### Option B: TileMap (Editor-Based — Production Quality)

1. Create a **TileMapLayer** node
2. Paint floors and walls using your tileset
3. Add **StaticBody2D** collision on wall tiles (Layer 1)
4. Manually place ShadowZone/CoverZone/SurfaceZone Area2Ds

### Walls and Collision Layers

| Layer # | Name | What Uses It |
|---------|------|-------------|
| 1 | World | Walls, floors, obstacles |
| 2 | Player | PlayerController |
| 3 | Enemies | GuardEnemy, EnemyBase |
| 4 | PlayerHitbox | Player attack hitbox |
| 5 | EnemyHitbox | Enemy attack hitbox |
| 6 | Interactables | Doors, chests, levers, etc. |
| 7 | PushBlock | Pushable puzzle blocks |

---

## 4. Placing Guards

### 4.1 Manual Assembly (Current Method)

Each guard needs this node structure:

```
GuardEnemy (CharacterBody2D — Scripts/Enemies/GuardEnemy.cs)
├── AnimatedSprite2D
├── HealthComponent (Scripts/Combat/HealthComponent.cs)
├── Hurtbox (Area2D — Scripts/Combat/Hurtbox.cs)
├── Hitbox (Area2D — Scripts/Combat/Hitbox.cs)
├── DetectionSensor (Node2D — Scripts/Stealth/DetectionSensor.cs)
├── PatrolRoute (Node2D — Scripts/Stealth/PatrolRoute.cs)
└── StateMachine (Scripts/Core/StateMachine.cs)
    ├── GuardPatrolState
    ├── GuardInvestigateState
    ├── GuardAlertState
    ├── GuardSearchState
    ├── GuardChaseState
    └── GuardAttackState
```

> **Tip:** Build this once, save as `Scenes/Enemies/Guard.tscn`, then instance it. Change exports per-instance.

### 4.2 Configuring Guard Behavior

**GuardEnemy exports:**
| Property | Default | Tuning |
|----------|---------|--------|
| `PatrolSpeed` | 40 | Slow = menacing, fast = aggressive |
| `AlertedSpeed` | 70 | How fast they investigate |
| `ChaseSpeed` | 90 | Sprint speed during pursuit |
| `InvestigateTime` | 5s | How long they look around at noise |
| `SearchTime` | 8s | How long they wander after losing player |
| `AlertCallRadius` | 200px | How far their shout carries |

**DetectionSensor exports (on child node):**
| Property | Default | Tuning Notes |
|----------|---------|-------------|
| `ViewDistance` | 120px | Longer = harder. 80–200 is the useful range |
| `ViewAngle` | 55° | Narrow = focused, wide = sweeping |
| `CloseDetectRadius` | 25px | Auto-detect if player walks right up |
| `HearingRange` | 150px | Paired with NoisePropagator wall occlusion |
| `HearingSensitivity` | 1.0 | <1 = half-deaf, >1 = sharp ears |
| `AwarenessGainRate` | 22/s | How fast the "!" meter fills |
| `AwarenessDecayRate` | 15/s | How fast they forget |

### 4.3 Designing Patrol Routes

On the guard's **PatrolRoute** child, set:
- `Waypoints`: Array of **local-space** Vector2 positions (relative to the PatrolRoute node)
- `Mode`: `Loop` (circle), `PingPong` (back and forth), `Once` (walk and stop)
- `WaitTimeAtPoint`: Seconds to pause at each waypoint (creates timing windows!)

**Good patrol design:**
- Overlap patrol routes so two guards' paths cross — creates tension
- Leave 3–5 second gaps between overlaps for the player to slip through
- Use `WaitTimeAtPoint` of 2–4s at doorways and corners — the guard pauses while looking
- Make waypoints follow L-shapes or U-shapes, not straight lines
- Point waypoints so the guard faces away from stealth corridors at pause points

**Example waypoints for an L-shaped patrol:**
```
Waypoints = [
    (0, 0),        // Start: facing south
    (0, 80),       // Walk south
    (80, 80),      // Turn east
    (80, 0),       // Walk north back to start
]
```

---

## 5. Stealth Zones

### 5.1 Shadow Zones

**What:** Areas where the player becomes harder to detect.
**Script:** `ShadowZone` (Area2D)
**Effect:** Increments `StealthProfile.ShadowZoneCount`. Crouching + shadow + still = **Hidden** (undetectable).

**Placement rules:**
- Place along walls, under overhangs, in alcoves
- Leave gaps between shadow zones — don't make entire rooms dark
- Shadow zones should connect to form "stealth lanes" parallel to guard patrol routes
- Size shadows to fit 1–2 tiles (16–32px) wide — narrow enough to require commitment

**How to add:**
1. Add an **Area2D** child, attach `ShadowZone` script
2. Add a **CollisionShape2D** child with a RectangleShape2D
3. (Optional) Add a dark-tinted **ColorRect** or **Sprite2D** as visual feedback

### 5.2 Cover Zones

**What:** Hard cover — player is **Hidden** regardless of crouching/movement.
**Script:** `CoverZone` (Area2D)
**Use for:** Behind crates, barrels, low walls, tall vegetation.

| Export | Purpose |
|--------|---------|
| `IsDestructible` | If true, enemies can break the cover (default false) |
| `Health` | HP before destruction (default 30) |

### 5.3 Surface Zones

**What:** Floor material that affects both footstep sounds AND detection noise radius.
**Script:** `SurfaceZone` (Area2D)

| Surface | NoiseMultiplier | Use Case |
|---------|----------------|----------|
| `grass` | `0.5` | Gardens, courtyards — quiet |
| `carpet` | `0.6` | Interior rooms — somewhat quiet |
| `wood` | `1.0` | Default wooden floors |
| `stone` | `1.3` | Castle corridors — loud |
| `metal` | `1.5` | Industrial areas — very loud |

**Placement:** Cover entire floor areas. The zone auto-sets the player's footstep audio and noise multiplier on overlap.

### 5.4 Hiding Spots

**What:** Closets, barrels, tall grass the player enters to become invisible.
**Script:** `HidingSpot` (extends Interactable, Area2D)
**Effect:** Full invisibility — collision removed, sprite hidden, +100 CoverZoneCount.

**Placement rules:**
- 1 hiding spot per major corridor or room
- Place near patrol route intersection points — the player needs somewhere to duck
- Space them 100–150px apart (not too close or stealth is trivial)
- Don't place them inside shadow zones (redundant)

---

## 6. Lighting

### 6.1 Light Sources (Torches, Lanterns)

**Script:** `LightSource` (extends Interactable)
**Key feature:** Player can extinguish → creates ShadowZone in its place.

| Export | Default | Notes |
|--------|---------|-------|
| `IsLit` | true | Start lit or pre-extinguished |
| `LightRadius` | 48px | Size of the illuminated area |
| `CanBeRelit` | true | Guards can relight on patrol if true |
| `LightColor` | warm orange | Visual only |
| `ExtinguishNoise` | 30px | Small noise — won't alert distant guards |

**Placement rules:**
- Place torches at corridor intersections, doorways, and patrol endpoints
- Spacing: every 80–120px along corridors — creates pools of light with dark gaps
- Put torches on walls the player needs to sneak past — extinguishing creates a route
- If `CanBeRelit = true`, the player's window is temporary (adds tension)
- Place a Shadow Zone manually for the "default dark" areas between torches

### 6.2 Light Design Pattern

```
          Torch          Torch          Torch
    ──────|████|──────────|████|──────────|████|──────
           lit            lit            lit
    
    ▓▓▓▓▓▓    ▓▓▓▓▓▓▓▓▓▓    ▓▓▓▓▓▓▓▓▓▓    ▓▓▓▓▓▓
    shadow     shadow         shadow        shadow
```

The player extinguishes a torch to widen the shadow gap, creating a safe passage.

---

## 7. Environmental Interactions

### 7.1 Doors

**Script:** `Door` (extends Interactable)

| Export | Default | Notes |
|--------|---------|-------|
| `IsLocked` | false | Requires key to open |
| `RequiredKeyId` | `""` | Must match a `Chest` or `PickupItem` key ID |
| `NoiseRadius` | 60px | Guard hears you open doors! |

**Design tips:**
- Locked doors gate player progression — pair with a key chest nearby
- Opening doors creates noise — the player must time it with patrol gaps
- Place doors at choke points between open areas

### 7.2 Breakable Walls

**Script:** `BreakableWall` (StaticBody2D)

| Export | Default | Notes |
|--------|---------|-------|
| `HitsRequired` | 1 | Sword strikes to destroy |
| `NoiseRadius` | 80px | LOUD — will alert guards |

**Design tips:**
- Use as "loud shortcut" — the player CAN break through but it alerts everyone
- Place breakable walls on alternate routes that skip a puzzle
- Pair with a puzzle gate on the "quiet" route — risk/reward choice

### 7.3 Alarm Bells

**Script:** `AlarmBell` (extends Interactable)

| Export | Default | Notes |
|--------|---------|-------|
| `AlarmRadius` | 400px | Massive range — affects all nearby guards |
| `RingDuration` | 10s | Keeps pulsing noise while ringing |

**Design tips:**
- Place at guard posts and restricted zone entrances
- Guards will ring these when they discover the player (call `bell.Ring()` from AI)
- Player can sabotage BEFORE guards ring it — rewards scouting
- Place 1–2 per major area, not more

### 7.4 Chests

**Script:** `Chest` (extends Interactable)

| Export | Purpose |
|--------|---------|
| `ContentsType` | `"key"`, `"ink"`, `"item"`, `"gadget"` |
| `ContentsId` | Unique ID for the item |
| `RequiredKeyId` | Lock the chest itself (optional) |

### 7.5 Distraction Throwables

**Script:** `DistractionThrowable` (CharacterBody2D)

The player throws objects to lure guards away. Not placed in the editor — spawned at runtime via:
```csharp
DistractionThrowable.Throw(parent, playerPos, aimDirection, noiseRadius: 120f);
```

| Variant | NoiseRadius | IsRepeating | Use |
|---------|------------|-------------|-----|
| Rock | 120px | false | Quick one-shot lure |
| Bone Chimes | 80px | true (8s) | Sustained distraction |

---

## 8. Puzzles

### Puzzle Components

| Component | What It Does | Key Exports |
|-----------|-------------|-------------|
| **FloorSwitch** | Pressure plate — activated by player or push block | `StayPressed` |
| **PushBlock** | Grid-snapping block, push onto switches | `GridSize=16` |
| **Lever** | Wall toggle, emits `Toggled(bool)` | `IsOn`, `OneWay` |
| **PuzzleGate** | Opens when N conditions met | `RequiredConditions`, `StayOpen` |

### Wiring Puzzles

Connect switches/levers to gates via signals in code or the editor:

```csharp
// In your room script:
var lever = GetNode<Lever>("Lever1");
var gate = GetNode<PuzzleGate>("PuzzleGate1");
lever.Toggled += (isOn) => { if (isOn) gate.ConditionMet(); else gate.ConditionUnmet(); };
```

### Puzzle Design for Stealth

- **Switches that make noise** — pushing a block onto a switch should create sound
- **Timed gates** — gate opens briefly when switch is pressed (forces quick movement)
- **Multi-room puzzles** — lever in room A opens gate in room B (player must backtrack past guards)
- **Optional puzzles** — reward with shortcuts, keys, or ink (never block the main path)

---

## 9. Room Connections & Transitions

### 9.1 Level Transitions

**Script:** `LevelTransition` (Area2D)

Place at doorways/edges between rooms. When the player overlaps, it fades and loads the target scene.

| Export | What to Set |
|--------|-------------|
| `TargetScene` | Path to `.tscn` file (e.g., `res://Scenes/Missions/Greenhold/Chapel.tscn`) |
| `TargetSpawnPoint` | Name of the SpawnPoint in the target scene (e.g., `"FromBarracks"`) |
| `Direction` | `North/South/East/West` — player facing on arrival |
| `IsLocked` | Set true + unlock via code for progression gating |

### 9.2 Matching Transitions

Transitions come in **pairs** — one in each room:

**Room A (Barracks):**
```
LevelTransition "ToChapel"
├── TargetScene = "res://Scenes/Missions/Greenhold/Chapel.tscn"
├── TargetSpawnPoint = "FromBarracks"
└── Direction = East

SpawnPoint "FromChapel"
└── FacingDirection = West
```

**Room B (Chapel):**
```
LevelTransition "ToBarracks"
├── TargetScene = "res://Scenes/Missions/Greenhold/Barracks.tscn"
├── TargetSpawnPoint = "FromChapel"
└── Direction = West

SpawnPoint "FromBarracks"
└── FacingDirection = East
```

### 9.3 Level Transition Placement

- Place at the very edge of the room (1 tile from the wall)
- Make the collision shape span the full doorway width
- Add a door sprite/node nearby for visual framing

---

## 10. Mission Alert System

### Overview

Each mission has a `MissionAlertManager` that tracks a global alert level (0–4):

| Level | Name | Effect |
|-------|------|--------|
| 0 | Unaware | Normal patrols |
| 1 | Suspicious | Guards patrol 15% faster |
| 2 | Alerted | Guards 30% faster, active searching |
| 3 | Hunted | Guards 50% faster, all pursue |
| 4 | Siege | Reinforcements signal fired |

### What Escalates Alert

| Event | Points | Notes |
|-------|--------|-------|
| Player detected (Alerted+) | +25 | Per guard that sees you |
| Corpse discovered | +40 | Guard walks over a body |
| Alarm bell rung | +60 | Massive escalation |

Alert decays at 2 points/second after 15 seconds of quiet.

### Design Implications

- **Corpse placement matters** — killing a guard in a busy corridor guarantees discovery
- **Alarm bells are high-value sabotage targets** — plan routes past them
- **Alert level 3+ should feel dangerous** — consider scripted reinforcement spawning

---

## 11. Design Principles

### The Three-Path Rule

Every room should offer at least **three approaches** to each objective:

1. **Stealth Path** — Shadow zones, cover, hiding spots. Slow but safe. Extinguish torches, crouch through shadows.
2. **Puzzle Path** — Push blocks, levers, breakable walls. Requires thought but avoids guards entirely.
3. **Combat Path** — Fewer guards, but direct engagement. Fast but loud — raises alert.

### Timing Windows

Good stealth design is about **timing**, not memorization:

- Guards pause at waypoints for 2–4 seconds → player moves during pauses
- Two overlapping patrols create a 3–5 second gap → player watches and waits
- Doors create noise → the player times the opening with a guard walking away
- Torch extinguishing has 30px noise → do it when the nearest guard is 40+ px away

### Noise Geography

Walls now block 60% of noise. Design rooms to exploit this:

```
┌─────────┬─────────┐
│ Guard A  │ Guard B  │   ← Wall between them
│    ↕     │    ↕     │
│ patrol   │ patrol   │
└─────────┴─────────┘
```

The player can make noise in Room A without alerting Guard B (unless the noise is very loud). This rewards the player for understanding the room layout.

### Restricted Zones

Mark sensitive areas with `AreaZone.IsRestricted = true`:
- Treasure rooms, command quarters, Edictbearer chambers
- Player is automatically **Exposed** while inside — no hiding in plain sight
- Forces the player to either clear the area or move fast

### Stealth Lane Pattern

The fundamental building block of stealth level design:

```
    Guard patrol path (lit)
    =========================>
    
    ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓   ← Shadow zone (stealth lane)
    
    ########################     ← Wall
    
    ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓   ← Another shadow zone
    
    =========================>
    Guard patrol path (lit)
```

The player moves through the shadow lane between two guard routes.

### Room Composition Template

```
┌─────────────────────────────────────┐
│  ENTRY          ALARM     GUARD POST│
│  ↓ spawn       [bell]    ↑guard B  │
│  ·····→ corridor ···→ ···↗         │
│        ▓shadow▓       ▓shadow▓     │
│  ┌─────────┐   ┌──────────────┐   │
│  │ SIDE    │   │  MAIN ROOM   │   │
│  │ ROOM    │   │              │   │
│  │ [chest] │   │  guard A ↕   │   │
│  │ [lever]→────│→[gate]       │   │
│  │         │   │     ↓EXIT    │   │
│  └─────────┘   └──────────────┘   │
│        ▓▓▓ shadow corridor ▓▓▓     │
│  [hiding spot]  [breakable wall]   │
│        ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓        │
│                    ↓                │
│              SECRET EXIT            │
└─────────────────────────────────────┘
```

**Three paths visible:**
1. Shadow corridor → sneak past guards → exit (stealth)
2. Side room → lever → gate opens → exit (puzzle)
3. Breakable wall → secret exit (loud shortcut)

---

## 12. Component Quick Reference

### Stealth Components

| Component | Node Type | Script Path | Purpose |
|-----------|----------|-------------|---------|
| ShadowZone | Area2D | `Scripts/Stealth/ShadowZone.cs` | Reduces visibility |
| CoverZone | Area2D | `Scripts/Stealth/CoverZone.cs` | Full concealment |
| SurfaceZone | Area2D | `Scripts/Stealth/SurfaceZone.cs` | Floor noise multiplier |
| LightSource | Area2D | `Scripts/Stealth/LightSource.cs` | Extinguishable torch |
| HidingSpot | Area2D | `Scripts/Interaction/HidingSpot.cs` | Enter to become invisible |
| PatrolRoute | Node2D | `Scripts/Stealth/PatrolRoute.cs` | Guard walk path |
| DetectionSensor | Node2D | `Scripts/Stealth/DetectionSensor.cs` | Vision cone + hearing |
| MissionAlertManager | Node | `Scripts/Stealth/MissionAlertManager.cs` | Global alert level |
| CorpseMarker | Area2D | `Scripts/Stealth/CorpseMarker.cs` | Dead body discovery |

### Interaction Components

| Component | Node Type | Script Path | Purpose |
|-----------|----------|-------------|---------|
| Door | Area2D | `Scripts/Interaction/Door.cs` | Open/close/lock |
| AlarmBell | Area2D | `Scripts/Interaction/AlarmBell.cs` | Sabotage-able alarm |
| Chest | Area2D | `Scripts/Interaction/Chest.cs` | Loot container |
| BreakableWall | StaticBody2D | `Scripts/Interaction/BreakableWall.cs` | Destructible shortcut |
| PushBlock | CharacterBody2D | `Scripts/Interaction/PushBlock.cs` | Grid puzzle block |
| FloorSwitch | Area2D | `Scripts/Interaction/FloorSwitch.cs` | Pressure plate |
| Lever | Area2D | `Scripts/Interaction/Lever.cs` | Toggle switch |
| PuzzleGate | Node2D | `Scripts/Interaction/PuzzleGate.cs` | Conditional gate |

### World Components

| Component | Node Type | Script Path | Purpose |
|-----------|----------|-------------|---------|
| RoomManager | Node2D | `Scripts/World/RoomManager.cs` | Room setup + player spawn |
| LevelTransition | Area2D | `Scripts/World/LevelTransition.cs` | Scene change trigger |
| SpawnPoint | Marker2D | `Scripts/World/SpawnPoint.cs` | Player entry position |
| AreaZone | Area2D | `Scripts/World/AreaZone.cs` | Named area + restricted flag |

---

## Appendix: Noise Reference

How far noise travels (before wall occlusion):

| Action | Radius | Notes |
|--------|--------|-------|
| Standing still | 0px | Silent |
| Crouch-walk | 30px | Very quiet |
| Normal walk | 70px | Moderate |
| Running | 150px | Loud — alerts most guards |
| Door open/close | 60px | Configurable |
| Breakable wall | 80px | Configurable |
| Guard death | 60px | Body hits ground |
| Rock throw impact | 120px | Distraction |
| Alarm bell pulse | 400px | Massive |
| Torch extinguish | 30px | Very quiet |

**Wall occlusion:** Walls block **60%** of noise radius. A 150px running noise only carries 60px through a wall.

**Surface multipliers:** Multiply the base radius by the surface zone's `NoiseMultiplier`:
- Grass (0.5×): 70px walk → 35px
- Stone (1.3×): 70px walk → 91px

---

*Last updated: March 2026*
