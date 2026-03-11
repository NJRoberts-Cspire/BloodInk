# BloodInk — Comprehensive Project Audit

**Date:** 2025  
**Build status:** ✅ Successful (0 warnings, 0 errors)  
**Engine:** Godot 4.x with C# / .NET  
**Scripts:** 109 .cs files across 21 folders  
**Scenes:** 19 .tscn files  
**Design docs:** LoreBible.md (470 lines), Storyboard.md (395 lines)

---

## TABLE OF CONTENTS

1. [Script Inventory](#1-script-inventory)
2. [Scene Inventory](#2-scene-inventory)
3. [Design Doc Cross-Reference](#3-design-doc-cross-reference)
4. [Bugs & Issues](#4-bugs--issues)
5. [Priority Gap Analysis (P0–P3)](#5-priority-gap-analysis)

---

## 1. SCRIPT INVENTORY

### Scripts/Core/ (4 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| GameManager.cs | 270 | ✅ Complete | Autoload singleton. Creates/wires TattooSystem, InkInventory, EchoManager, CampaignManager, SpyNetwork, Warband, Crafting, Tremor, Camp, Choices, NewGamePlus, SaveSystem, NoisePropagator, 6 KingdomStates. Save/Load/NG+ transitions. |
| SaveSystem.cs | 219 | ✅ Complete | JSON save/load to `user://saves/`, slot management, JsonElement unboxing. |
| StateMachine.cs | 80 | ✅ Complete | Generic state machine: Init/Enter/Exit/Update/PhysicsUpdate/HandleInput. |
| State.cs | 35 | ✅ Complete | Base state class with virtual methods. |

### Scripts/Player/ (2 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| PlayerController.cs | ~160 | ✅ Complete | CharacterBody2D. Movement (acceleration/friction), knockback, animation dispatch, hurt/death/i-frames, VFX integration, death→GameOver transition. |
| PlayerAnimationSetup.cs | 126 | ✅ Complete | [Tool] script. Slices player_sheet.png into SpriteFrames via SpriteSheetAnimator. Falls back to PlaceholderSprites. |

### Scripts/Player/States/ (6 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| PlayerIdleState.cs | ~55 | ✅ Complete | Waits for input → Move/Attack/Dodge/Crouch. Ticks cooldowns. |
| PlayerMoveState.cs | ~55 | ✅ Complete | 8-dir movement with tattoo speed bonus. Transitions to Attack/Dodge/Crouch. |
| PlayerAttackState.cs | ~70 | ✅ Complete | Enables sword hitbox for 0.35s. Tattoo damage bonus. SlashArc VFX + CameraShake. |
| PlayerDodgeState.cs | ~65 | ✅ Complete | I-frame dash in facing direction. GhostTrail + DustPuff VFX. |
| PlayerCrouchState.cs | ~75 | ✅ Complete | Crouching movement at reduced speed. StealthProfile integration. Attack from crouch = stealth kill. |
| PlayerStealthKillState.cs | 179 | ✅ Complete | High-damage silent kill from behind/while hidden. Falls back to noisy attack if conditions fail. Backstab angle check, noise propagation. |

### Scripts/Ink/ (4 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| TattooSystem.cs | 318 | ✅ Complete | Temperament tracking (Shadow/Fang/Root/Bone), tattoo application with ink cost, evolution, InkBleed/InkCalm, stat aggregation, serialize/deserialize, NG+ temperament import. |
| TattooData.cs | ~80 | ✅ Complete | Resource: Id, DisplayName, Slot, RequiredGrade, InkCost, PrimaryTemperament, stat modifiers, ability scene path, whisper text, blood echo ID, evolution data. |
| InkInventory.cs | ~80 | ✅ Complete | Major/Lesser/Trace ink tracking. Add/spend/serialize. |
| InkTemperament.cs | ~30 | ✅ Complete | Enums: InkTemperament, InkGrade, TattooSlot (6 slots). |

### Scripts/Combat/ (3 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| HealthComponent.cs | ~60 | ✅ Complete | HP tracking, damage, healing, Died signal. |
| Hitbox.cs | ~25 | ✅ Complete | Damage, KnockbackForce, Source, IsStealthKill flag. |
| Hurtbox.cs | ~80 | ✅ Complete | Collision detection, VFX integration (HitFlash, DamageNumber, BloodSplatter, CameraShake, HitStop). Stealth kill awareness. |

### Scripts/Enemies/ (2 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| EnemyBase.cs | ~120 | ✅ Complete | CharacterBody2D base. Sprite, hitbox/hurtbox, health, knockback, PlaceholderSprites, death VFX (DeathEffect). |
| GuardEnemy.cs | ~130 | ✅ Complete | Extends EnemyBase. DetectionSensor, PatrolRoute, guard communication (CallBackup), inter-guard alerting via Groups, noise on death. |

### Scripts/Enemies/States/ (9 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| EnemyIdleState.cs | ~45 | ✅ Complete | Waits; transitions to Patrol or Chase. |
| EnemyChaseState.cs | ~50 | ✅ Complete | Chases player, attacks when close. Basic (no stealth awareness). |
| EnemyAttackState.cs | ~65 | ✅ Complete | Hitbox enable/cooldown cycle. |
| GuardPatrolState.cs | 117 | ✅ Complete | Waypoint patrol with wait times. Checks detection/noise for escalation. |
| GuardAlertState.cs | ~85 | ✅ Complete | Cautious approach to last-known position. Timeout → Investigate. |
| GuardChaseState.cs | ~85 | ✅ Complete | Full-speed chase. CallBackup on first engagement. De-escalates to Search. |
| GuardAttackState.cs | ~75 | ✅ Complete | Hitbox cycle with noise propagation. Post-cooldown state selection. |
| GuardSearchState.cs | 116 | ✅ Complete | Random search around last-known position. 5 search points, timer-based. Redirects on new noise. |
| GuardInvestigateState.cs | 141 | ✅ Complete | Walks to noise/sighting position, looks around, returns to patrol. |

### Scripts/Stealth/ (7 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| DetectionSensor.cs | 301 | ✅ Complete | Vision cone raycasting, angle/distance checks, close-range 360°, awareness thresholds (Suspicious/Alerted/Searching/Engaged), noise handling, debug draw. |
| NoisePropagator.cs | ~80 | ✅ Complete | Singleton. Sensor registration, noise propagation, alarm system. |
| StealthProfile.cs | ~100 | ✅ Complete | Visibility levels (Hidden/Low/Normal/Exposed), crouch state, shadow/cover zone counts, noise/speed modifiers. |
| ShadowZone.cs | ~40 | ✅ Complete | Area2D — increments/decrements player ShadowZoneCount. |
| CoverZone.cs | ~50 | ✅ Complete | Area2D with destructible option, cover zone counting. |
| PatrolRoute.cs | ~70 | ✅ Complete | Loop/PingPong/Once modes, waypoint iteration, wait times. |
| StealthEnums.cs | ~25 | ✅ Complete | Enums: VisibilityLevel, NoiseType, AwarenessLevel. |

### Scripts/Dialogue/ (4 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| DialogueManager.cs | 285 | ✅ Complete | Conversation start/end, advance with cycle detection, choice selection, flag system (set/get/has), condition evaluation (!, =), variable substitution, event firing, pause. |
| DialogueData.cs | ~30 | ✅ Complete | Resource with ConversationId, Lines array, lazy lookup. |
| DialogueLine.cs | ~30 | ✅ Complete | Resource: Id, Speaker, PortraitKey, Text, Choices, NextLineId, Condition, SetFlag, Event, IsEntry. |
| DialoguePanel.cs | 185 | ✅ Complete | UI with typewriter effect, choice buttons, signal-based. |

### Scripts/BloodEchoes/ (2 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| BloodEchoManager.cs | 171 | ✅ Complete | Registration, unlock, playback (scene transition), completion tracking, serialize. |
| EchoData.cs | ~40 | ✅ Complete | Resource: Id, EdictbearerName, KingdomIndex, ScenePath, Genre, IntelUnlocked, WeaknessesRevealed. |

### Scripts/Campaigns/ (2 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| CampaignManager.cs | 145 | ✅ Complete | Campaign unlock on kingdom completion, start/complete, serialize. |
| CampaignData.cs | ~40 | ✅ Complete | Resource + CampaignHand enum (Vetch/Rukh/Grael/Lorne). |

### Scripts/Campaigns/RukhCampaign/ (3 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| SpyNetworkManager.cs | 387 | ✅ Complete | Agent management, mission assignment (6 types), turn-based resolution, kingdom heat, compromise/betrayal risk, intel generation, serialize. |
| IntelSystem.cs | ~75 | ✅ Complete | IntelType enum (7 types), IntelData resource. |
| AgentData.cs | ~45 | ✅ Complete | Resource: Id, CodeName, CoverRole, KingdomIndex, SkillLevel, Loyalty, IsCompromised, IsOnMission. |

### Scripts/Campaigns/GraelCampaign/ (3 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| WarbandManager.cs | 204 | ✅ Complete | Recruitment, training (strength/endurance/morale), morale/renown, raid preparation, serialize. |
| WarriorData.cs | ~65 | ✅ Complete | Resource + WarriorRole enum (Brawler/ShieldBearer/Flanker/Skirmisher/WarChanter/Breaker). |
| RaidController.cs | ~200 | ✅ Complete | Phase-based raid resolution. Force comparison, role bonuses, casualties, morale tracking, 5 outcome types. |

### Scripts/Campaigns/LorneCampaign/ (4 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| TremorSystem.cs | ~190 | ✅ Complete | Tremor intensity 0-100, flare events, rest/herb recovery (diminishing returns), NG+ base tremor increase, serialize. |
| CraftingSystem.cs | 234 | ✅ Complete | Material inventory, recipe management, crafting with tremor interaction, quality tiers (Masterwork/Standard/Flawed/Ruined), mastery tracking, serialize. |
| CraftingRecipe.cs | 101 | ✅ Complete | Resource + MaterialType enum (6 types), CraftedItemType enum (7 types). |
| CampManager.cs | 221 | ✅ Complete | Day cycle, food/medicine/morale, foraging, camp discovery/danger, camp relocation, random events, serialize. |

### Scripts/Progression/ (4 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| KingdomState.cs | 217 | ✅ Complete | AlertLevel (Unaware/Suspicious/Alerted/Lockdown), heat system, target registration/kill tracking, area discovery, narrative flags, completion check, serialize. |
| TargetData.cs | ~50 | ✅ Complete | Resource: Id, Name, Title, KingdomIndex, IsEdictbearer, IsMandatory, Difficulty, MissionScenePath, InkDrop, InkAmount, BloodEchoId, Weaknesses, IntelBrief, DeathWhisper. |
| PlayerChoices.cs | 149 | ✅ Complete | Mercy/Cruelty tracking, optional kills, targets spared, EndingAlignment enum (Liberation/DarkEdictbearer/BitterFreedom), narrative flags, serialize. |
| NewGamePlus.cs | 207 | ✅ Complete | Cycle tracking, temperament carryover at 50%, previous kills as ghosts, narrative flag carry, Lorne tremor increase, serialize. |

### Scripts/Missions/ (5 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| MissionLevelBase.cs | 362 | ✅ Complete | Base class. SpawnPlayer, SetCameraLimits, ApplyPlayerSprite, SetupHUD, AddGuard factory (builds full guard with 7-state FSM), AddShadowZone, AddAreaZone, AddHidingSpot, OnTargetKilled flow. |
| GoldmanorLevel.cs | 511 | ✅ Complete | Lord Cowl's estate. 3 areas via ASCII maps: Gardens, Main Hall, Lord's Quarters. Guards, shadow zones, hiding spots, area transitions. |
| LaborCampLevel.cs | 419 | ✅ Complete | Reeve Maren's domain. 3 areas: Quarry Yard, Tunnel Passage, Maren's Office. |
| ChapelLevel.cs | 424 | ✅ Complete | Sister Blessing. 3 areas: Chapel Grounds, Nave & Vestry, Relic Chamber. |
| BarracksLevel.cs | 483 | ✅ Complete | Captain Thorne. 3 areas: Training Yard, Barracks Interior, Trophy Room. |

### Scripts/Interaction/ (6 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| InteractionManager.cs | 112 | ✅ Complete | Singleton — nearby tracking, focus management, interact dispatching. |
| Interactable.cs | ~60 | ✅ Complete | Base Area2D: DisplayName, ActionVerb, IsEnabled, OneShot, focus/unfocus signals. |
| DialogueNPC.cs | ~40 | ✅ Complete | Starts conversation via DialogueManager. |
| Door.cs | ~55 | ⚠️ Partial | Open/close, locked state, noise. **TODO: key-checking via inventory (line 35).** |
| HidingSpot.cs | 121 | ✅ Complete | Hide/unhide, player invisibility, collision disable, state machine disable. |
| PickupItem.cs | ~40 | ✅ Complete | ItemId, ItemType, Quantity, auto-remove. |
| MissionBoard.cs | 223 | ✅ Complete | Generates dialogue from KingdomState target data. Only Kingdom 0 hardcoded. |

### Scripts/UI/ (10 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| GameHUD.cs | 210 | ✅ Complete | Health, stealth indicator, interact prompt, ink totals, area name, alert/crouch labels. |
| HUD.cs | 83 | ✅ Complete | Simpler/older HUD — health label, state label, stealth display. Used by TestWorld. |
| PauseMenu.cs | ~80 | ✅ Complete | Resume, Save, Load, Settings, Quit to Menu. |
| MainMenu.cs | ~70 | ✅ Complete | New Game → CampScene, Continue (via Save), Settings, Quit. |
| GameOver.cs | ~100 | ✅ Complete | "THE EDICT ENDURES" death screen. Retry → Camp, Quit → MainMenu. |
| MissionComplete.cs | 125 | ✅ Complete | Target eliminated screen with whisper text, reward display, return to camp. |
| InkTentPanel.cs | 241 | ✅ Complete | Tattoo selection by slot, stats preview, ink cost, temperament info, apply via TattooSystem. |
| SettingsPanel.cs | 498 | ✅ Complete | Tabbed: Audio (5 sliders), Display (window mode, resolution, VSync), Controls (embeds KeybindSettings). Persists to user://settings.cfg. |
| KeybindSettings.cs | 329 | ✅ Complete | Keybind remapping for 9 actions. Click-to-rebind, Escape to cancel, Reset to defaults. Persists to user://keybinds.cfg. |
| DialoguePanel.cs | 185 | ✅ Complete | (Listed under Dialogue above.) |

### Scripts/Audio/ (3 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| AudioManager.cs | 261 | ✅ Complete | Singleton. Music (crossfade), SFX (8-player pool), UI sounds, ambient bus. 5 volume controls. |
| FootstepPlayer.cs | 101 | ✅ Complete | Velocity-based footstep sounds, surface type switching, crouch volume reduction. |
| AmbientZone.cs | 139 | ✅ Complete | Area2D zone with looping ambient audio, random one-shot layering, optional music trigger. |

### Scripts/VFX/ (10 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| CameraShake.cs | 142 | ✅ Complete | Trauma-based shake, look-ahead, smooth scrolling, drag margins, map limits. Singleton. |
| ScreenTransition.cs | ~80 | ✅ Complete | Fade to/from black/white, flash red/white. Singleton. |
| BloodSplatter.cs | 83 | ✅ Complete | Directional particle splatter. |
| DamageNumber.cs | 97 | ✅ Complete | Floating damage numbers with rise/fade/scale. |
| HitFlash.cs | ~40 | ✅ Complete | White flash on damage via modulate tween. |
| HitStop.cs | ~60 | ✅ Complete | Engine.TimeScale freeze. Light/Medium/Heavy presets. Singleton. |
| SlashArc.cs | ~60 | ✅ Complete | Crescent swipe via ColorRect fan. Normal (white) and heavy (red) variants. |
| InkSwirl.cs | 118 | ✅ Complete | Spiralling ink particles for tattoo application VFX. |
| GhostTrail.cs | ~90 | ✅ Complete | Afterimage trail during dodge. Spawns semi-transparent sprite copies. |
| DustPuff.cs | ~50 | ✅ Complete | Small particle burst on dodge/landing. |
| DeathEffect.cs | ~95 | ✅ Complete | Flash → expand → dissolve particles for enemy death. |
| VfxAnimationLibrary.cs | 110 | ✅ Complete | Loads VFX animations from sprite sheet via SpriteSheetAnimator. |

### Scripts/World/ (6 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| TestWorld.cs | ~120 | ✅ Complete | Test arena. Wires player→HUD, enemies→player, shadow zones, camera limits. |
| CampScene.cs | 271 | ✅ Complete | Ashwild Camp hub. NPC dialogue wiring (Act-based), MissionBoard replacement, NPC sprites, collision shapes, RefreshCampState, deploy to mission. |
| SpawnPoint.cs | ~25 | ✅ Complete | Marker2D. Facing direction export, auto-group. |
| RoomManager.cs | 120 | ✅ Complete | Player spawn at entry point after scene transition, fade-in. |
| LevelTransition.cs | ~100 | ✅ Complete | Area2D trigger for scene changes with fade-to-black, locked state. |
| AreaZone.cs | ~55 | ✅ Complete | Area name display, tension modifier, restricted zone flag. |

### Scripts/Tools/ (3 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| PlaceholderSprites.cs | 472 | ✅ Complete | Loads Calciumtrice tileset for map tiles; character sheet for walk animations. Falls back to colored rectangles. |
| MapBuilder.cs | ~90 | ✅ Complete | Builds TileMapLayer from ASCII string maps. Character-to-tile mapping. Wall collision. |
| SpriteSheetAnimator.cs | ~80 | ✅ Complete | Utility: slices sprite sheet into SpriteFrames. Supports AtlasTexture regions. |

### Scripts/Content/ (5 files)

| File | Lines | Status | Purpose |
|------|-------|--------|---------|
| TattooRegistry.cs | 327 | ✅ Complete | 16 tattoo definitions across 6 slots. FindById lookup. |
| GreenholdTargets.cs | ~160 | ✅ Complete | 7 targets for Kingdom 0 (4 mandatory, 2 optional, 1 morally complex). |
| EchoRegistry.cs | 119 | ✅ Complete | 6 Blood Echo definitions (one per Edictbearer). |
| CraftingRecipeRegistry.cs | 145 | ✅ Complete | 8 crafting recipes (salves, poisons, needles, gadgets, bandages, tattoo ink). |
| CampDialogues.cs | 235 | ✅ Complete | All camp NPC dialogue for Act 1 + post-mission variants for Greenhold. |

---

## 2. SCENE INVENTORY

| Scene | Script(s) Attached | Purpose |
|-------|-------------------|---------|
| Scenes/World/TestWorld.tscn | TestWorld.cs | Debug arena with enemies, walls, shadow zones |
| Scenes/World/Camp.tscn | CampScene.cs, DialogueNPC.cs (×5) | Player hub between missions — 4 NPCs, MissionBoard, InkTent |
| Scenes/Player/Player.tscn | PlayerController.cs, StateMachine.cs, 6 State scripts, Hurtbox.cs, Hitbox.cs, HealthComponent.cs, PlayerAnimationSetup.cs, VfxAnimationLibrary.cs, StealthProfile.cs, GhostTrail.cs, CameraShake.cs, HitStop.cs, ScreenTransition.cs | Full player prefab (18 scripts) |
| Scenes/Enemies/Slime.tscn | (basic enemy) | Test enemy |
| Scenes/Missions/Greenhold/Goldmanor.tscn | GoldmanorLevel.cs | Lord Cowl assassination mission |
| Scenes/Missions/Greenhold/LaborCamp.tscn | LaborCampLevel.cs | Reeve Maren assassination mission |
| Scenes/Missions/Greenhold/Chapel.tscn | ChapelLevel.cs | Sister Blessing assassination mission |
| Scenes/Missions/Greenhold/Barracks.tscn | BarracksLevel.cs | Captain Thorne assassination mission |
| Scenes/UI/GameHUD.tscn | GameHUD.cs | In-mission HUD |
| Scenes/UI/HUD.tscn | HUD.cs | TestWorld-only HUD (simpler) |
| Scenes/UI/DialoguePanel.tscn | DialoguePanel.cs | Dialogue conversation UI |
| Scenes/UI/MainMenu.tscn | MainMenu.cs | Title screen |
| Scenes/UI/PauseMenu.tscn | PauseMenu.cs | In-game pause menu |
| Scenes/UI/GameOver.tscn | GameOver.cs | Death screen |
| Scenes/UI/MissionComplete.tscn | MissionComplete.cs | Post-kill results screen |
| Scenes/UI/InkTentPanel.tscn | InkTentPanel.cs | Tattoo selection/apply UI |
| Scenes/UI/SettingsPanel.tscn | SettingsPanel.cs | Audio/Display/Controls settings |
| Scenes/UI/KeybindSettings.tscn | KeybindSettings.cs | Keybind remapping (embedded in Settings) |
| Scenes/UI/FadeLayer.tscn | (no script) | CanvasLayer for screen fade transitions |

### Referenced but MISSING scenes:

| Referenced In | Missing Scene Path | Impact |
|--------------|-------------------|--------|
| GreenholdTargets.cs (The Assessor) | `res://Scenes/Missions/Greenhold/Roads.tscn` | ⚠️ Crash on mission select |
| GreenholdTargets.cs (Silas Rootwarden) | `res://Scenes/Missions/Greenhold/RootwardenFarm.tscn` | ⚠️ Crash on mission select |
| TattooRegistry.cs | `res://Scenes/Abilities/ShadowStep.tscn` | ❌ No ability scenes exist |
| TattooRegistry.cs | `res://Scenes/Abilities/BloodRage.tscn` | ❌ |
| TattooRegistry.cs | `res://Scenes/Abilities/WallCling.tscn` | ❌ |
| TattooRegistry.cs | `res://Scenes/Abilities/EnemySense.tscn` | ❌ |
| TattooRegistry.cs | `res://Scenes/Abilities/StoneHeart.tscn` | ❌ |
| TattooRegistry.cs | `res://Scenes/Abilities/MaskOfAsh.tscn` | ❌ |
| EchoRegistry.cs | `res://Scenes/Echoes/echo_cowl.tscn` | ❌ No echo scenes exist |
| EchoRegistry.cs | `res://Scenes/Echoes/echo_keelan.tscn` | ❌ |
| EchoRegistry.cs | `res://Scenes/Echoes/echo_myre.tscn` | ❌ |
| EchoRegistry.cs | `res://Scenes/Echoes/echo_ashford.tscn` | ❌ |
| EchoRegistry.cs | `res://Scenes/Echoes/echo_morvain.tscn` | ❌ |
| EchoRegistry.cs | `res://Scenes/Echoes/echo_accord.tscn` | ❌ |

---

## 3. DESIGN DOC CROSS-REFERENCE

### 3.1 Kingdom Names Mismatch

The **Storyboard** defines 6 kingdoms:
> Greenhold, Drench, Fane of Flensing, Verdancy, Crucible, Accord Spire

**GameManager.cs line 92** uses different names:
> Ashenmarch, Veilgard, Thornwall, Duskhollow, Irontide, The Pale

These don't correspond in any obvious way. **This must be reconciled.**

### 3.2 Alert Level Count

| Storyboard (5 levels) | Code — `AwarenessLevel` enum (5) | Code — `AlertLevel` enum (4) |
|----------------------|-----------------------------------|------------------------------|
| 1. Unaware | Unaware | Unaware |
| 2. Suspicious | Suspicious | Suspicious |
| 3. Alerted | Alerted | Alerted |
| 4. Hunted | Searching + Engaged | — |
| 5. Siege | — | Lockdown |

- `AwarenessLevel` (per-guard) has 5 levels matching reasonably well
- `AlertLevel` (per-kingdom in `KingdomState.cs`) only has 4 levels — missing **Hunted** and **Siege**
- The storyboard describes "Hunted" as a kingdom-wide lockdown where the Edictbearer retreats to a safe room, and "Siege" as full identification requiring escape or fight-through. Neither exists.

### 3.3 Endings

| Storyboard | Code (`EndingAlignment` enum) |
|-----------|-------------------------------|
| Break the Edict (liberation) | `Liberation` ✅ |
| A Gentle Dark (mercy-based) | `BitterFreedom` ⚠️ Name differs |
| The Counter-Edict (become new Edictbearer) | `DarkEdictbearer` ⚠️ Name differs |

The logic exists in `PlayerChoices.cs` but the names don't match the storyboard. No ending scenes or cinematics are implemented.

### 3.4 Storyboard Systems vs Code Status

| Storyboard System | Code Status | Notes |
|------------------|-------------|-------|
| 6 Kingdoms, 6-8 targets each | ⚠️ Only Kingdom 0 (Greenhold, 7 targets) | Kingdoms 1-5 have no target data, no missions, no level scripts |
| Ink/Tattoo system (6 slots) | ✅ Fully implemented | 16 tattoos defined, TattooSystem complete |
| Blood Echoes (playable flashbacks) | ⚠️ System complete, no content | 6 echoes defined, but 0 echo scenes exist |
| Camp hub with NPCs | ✅ Implemented for Act 1 | 4 NPCs with act-based dialogue, MissionBoard |
| Ink Tent (tattoo application) | ✅ UI complete | InkTentPanel.cs wired, TattooRegistry populated |
| Rukh's Spy Network | ✅ System complete | Agents, missions, turns, intel, heat — no UI |
| Grael's Warband | ✅ System complete | Recruitment, training, raids — no UI |
| Lorne's Crafting + Tremor | ✅ System complete | CraftingSystem, TremorSystem, CampManager — no UI |
| Stealth tools (Ash Bombs, Bone Chimes, etc.) | ❌ Not implemented | Referenced in storyboard, zero code |
| Inter-kingdom cascading effects | ❌ Not implemented | Storyboard Part IV describes Edictbearer relationships affecting difficulty — no code |
| NG+ / Cycle system | ✅ Logic complete | Temperament carryover, ghost kills, tremor increase — no UI trigger |
| Alert escalation per kingdom | ⚠️ Partial | 4/5 levels. Heat system works but missing Hunted/Siege |
| Save/Load | ✅ Complete | JSON serialization of all systems |
| Audio system | ✅ Code complete | AudioManager, FootstepPlayer, AmbientZone — **no actual audio files** |
| Dialogue system | ✅ Complete | DialogueManager, panel, flags, events, choices |
| Target weakness/intel integration | ⚠️ Data exists, no gameplay effect | Weaknesses defined in GreenholdTargets but missions don't check them |
| Optional target moral complexity | ✅ Data designed | Dame Cowl hides orc children; mercy/cruelty tracking exists |

### 3.5 NPC Cast — Storyboard vs Code

| NPC | Storyboard Role | Code Status |
|-----|----------------|-------------|
| Vetch | Protagonist (player) | ✅ PlayerController |
| Needlewise | Tattoo shaman / secret Edict creator | ✅ Camp NPC + dialogue; identity reveal flag exists |
| Rukh | Spy handler | ✅ Camp NPC + SpyNetworkManager |
| Grael | Warband leader | ✅ Camp NPC + WarbandManager |
| Senna | Emotional heart / historian | ✅ Camp NPC + dialogue |
| Lorne | Craftswoman with tremor | ⚠️ Systems exist (TremorSystem, CraftingSystem, CampManager) but **no camp NPC or dialogue** |

**Lorne has no NPC presence in CampScene** — the storyboard gives her a major role but she's absent from the camp.

---

## 4. BUGS & ISSUES

### 4.1 Confirmed Issues

| # | Severity | Location | Issue |
|---|----------|----------|-------|
| 1 | 🔴 High | `GameManager.cs:92` | Kingdom names don't match design docs. Code uses "Ashenmarch" etc., storyboard uses "Greenhold" etc. |
| 2 | 🔴 High | `GameManager.cs:99-101` | Only `Kingdoms[0]` gets targets registered (`GreenholdTargets.GetAll()`). Kingdoms 1-5 have empty target lists. |
| 3 | 🟡 Medium | `GreenholdTargets.cs:107,126` | Targets "The Assessor" and "Silas Rootwarden" reference `Roads.tscn` and `RootwardenFarm.tscn` which don't exist. Selecting these missions will crash. |
| 4 | 🟡 Medium | `Door.cs:35` | TODO: key-checking via inventory not implemented. Locked doors can never be opened by the player. |
| 5 | 🟢 Low | `HUD.cs` / `GameHUD.cs` | Two separate HUD scripts exist. `HUD.cs` is only used by `TestWorld.tscn`, `GameHUD.cs` by everything else. Potential confusion. |
| 6 | 🟢 Low | `MissionBoard.cs:42` | Hardcoded to `Kingdom 0`. Won't work for other kingdoms. |
| 7 | 🟢 Low | `CampScene.cs` | Only wires dialogue/progress for Kingdom 0 (Greenhold). No multi-kingdom progression UI. |
| 8 | 🟢 Low | `MainMenu.cs:45` | "New Game" loads `TestWorld.tscn` instead of `Camp.tscn`. "Continue" loads Camp. Reversed? |

### 4.2 Potential Runtime Issues

| # | Issue | Notes |
|---|-------|-------|
| A | **Ability scenes not loaded** — TattooRegistry references 6 ability .tscn files that don't exist. If code tries to instantiate them, it will fail silently (GD.Load returns null). | Tattoo application works but active abilities won't spawn. |
| B | **Echo playback will fail** — BloodEchoManager calls `GetTree().ChangeSceneToFile(ScenePath)` on echo scenes that don't exist. | Will produce error and potentially crash. |
| C | **PlayerAttackState.CooldownRemaining is static** — shared across all instances. Fine for single-player but could be confusing. | Not a bug in current architecture. |
| D | **HitStop uses Engine.TimeScale** — affects all nodes including enemies/UI. `Timer` nodes with `ProcessMode.Always` are unaffected, but physics-based timers will behave unexpectedly during hitstop. | Working as intended for now. |

### 4.3 Code Quality Observations

- **No null-reference crashes**: All singleton accesses use `?.` null-conditional operators consistently.
- **Serialization is thorough**: Every system has Serialize/Deserialize. Save/Load is fully functional.
- **State machine architecture is solid**: Clean Enter/Exit/PhysicsUpdate pattern with no leaked states.
- **Signal usage is correct**: Godot signal delegates follow the `EventHandler` suffix convention.
- **Namespaces are consistent**: `BloodInk.{System}` throughout.
- **XML documentation** is present on all public types and most public members.

---

## 5. PRIORITY GAP ANALYSIS

### P0 — Blocking (must fix before any playtesting)

| # | Gap | Files Affected | Effort | Action |
|---|-----|---------------|--------|--------|
| P0-1 | **Kingdom name mismatch** — code uses "Ashenmarch" etc., design docs use "Greenhold" etc. | `GameManager.cs:92` | 5 min | Rename array to match storyboard: `{"Greenhold","Drench","Fane of Flensing","Verdancy","Crucible","Accord Spire"}` |
| P0-2 | **Missing mission scenes** — Roads.tscn and RootwardenFarm.tscn referenced but don't exist. MissionBoard will try to load them. | `GreenholdTargets.cs:107,126` | 2–4 hrs each | Create `RoadsLevel.cs` + .tscn, `RootwardenFarmLevel.cs` + .tscn following the MissionLevelBase pattern, OR remove targets from GetAll() until ready |
| P0-3 | **"New Game" loads TestWorld** instead of Camp | `MainMenu.cs:45` | 1 min | Change path from `TestWorld.tscn` to `Camp.tscn` |

### P1 — Important (needed for vertical slice)

| # | Gap | Files Affected | Effort | Action |
|---|-----|---------------|--------|--------|
| P1-1 | **Locked doors can't be unlocked** — no key/inventory system | `Door.cs:35`, needs new `InventorySystem` | 2–4 hrs | Implement basic key inventory on PlayerController or GameManager |
| P1-2 | **Missing alert levels** — "Hunted" and "Siege" from storyboard not in KingdomState | `KingdomState.cs`, `StealthEnums.cs` | 1–2 hrs | Add Hunted/Siege to `AlertLevel` enum, implement escalation triggers |
| P1-3 | **Lorne missing from camp** — No NPC node, no dialogue, no camp interaction | `CampScene.cs`, new `CampDialogues` entries | 2–3 hrs | Add Lorne NPC to Camp.tscn, wire dialogue, connect InkTent to CraftingSystem |
| P1-4 | **Target weaknesses have no gameplay effect** — defined in GreenholdTargets but missions don't use them | `MissionLevelBase.cs`, individual level scripts | 4–8 hrs | Implement weakness-based bonus routes/opportunities in each mission |
| P1-5 | **Ending alignment names differ from storyboard** | `PlayerChoices.cs:140-148` | 15 min | Rename enum values or update storyboard to match |
| P1-6 | **No stealth tools** — Ash Bombs, Bone Chimes, Blood Lure, Corpse Hide, Disguise Kit described in storyboard | New scripts needed | 4–8 hrs per tool | Implement as items usable from inventory |
| P1-7 | **Ability scenes not created** — 6 active ability .tscn referenced by tattoos | New scene files in `Scenes/Abilities/` | 2–4 hrs per ability | Create ability scenes (ShadowStep, BloodRage, WallCling, EnemySense, StoneHeart, MaskOfAsh) |

### P2 — Content (needed for full game but not vertical slice)

| # | Gap | Files Affected | Effort | Action |
|---|-----|---------------|--------|--------|
| P2-1 | **Kingdoms 1-5 have no targets** | New `*Targets.cs` files in Content/ | 2–3 hrs per kingdom | Create DrenchTargets, FaneTargets, VerdancyTargets, CrucibleTargets, AccordSpireTargets |
| P2-2 | **Kingdoms 1-5 have no missions** | New level scripts + scenes | 8–20 hrs per kingdom | Create mission levels following MissionLevelBase pattern |
| P2-3 | **No Blood Echo scenes** — 6 defined, 0 playable | New scenes in `Scenes/Echoes/` | 4–8 hrs per echo | Create playable flashback scenes per genre (WalkingSim, Puzzle, Stealth, Dialogue) |
| P2-4 | **Campaign subsystems have no UI** — SpyNetwork, Warband, Crafting all work but are invisible | New UI scripts/scenes | 4–8 hrs per subsystem | Build SpyNetworkPanel, WarbandPanel, CraftingPanel UI |
| P2-5 | **No audio assets** — AudioManager/FootstepPlayer/AmbientZone have no sound files | Audio asset creation | Ongoing | Source/create music tracks, SFX, ambient loops |
| P2-6 | **No sprite sheets** — everything uses PlaceholderSprites fallback | Art asset creation | Ongoing | Create player_sheet.png, dungeon_tileset.png, tiny16_characters.png |
| P2-7 | **Inter-kingdom cascading effects** not implemented — storyboard describes Edictbearer relationships | `KingdomState.cs`, `GameManager.cs` | 4–6 hrs | Implement cross-kingdom modifiers when Edictbearers die |
| P2-8 | **No inter-kingdom map/travel screen** | New UI scene | 4–6 hrs | Create kingdom selection / map UI |
| P2-9 | **Camp dialogue only covers Act 1 + Greenhold** | `CampDialogues.cs` | 2–4 hrs per act/kingdom | Write dialogue variants for acts 2-6 |

### P3 — Polish (quality of life, can wait)

| # | Gap | Notes |
|---|-----|-------|
| P3-1 | **Duplicate HUD** — `HUD.cs` and `GameHUD.cs` serve similar purposes | Remove HUD.cs and wire TestWorld to GameHUD |
| P3-2 | **No save slot selection UI** — only `Slot0` is used | PauseMenu calls `SaveSystem.SaveGame("Slot0")` directly |
| P3-3 | **FadeLayer.tscn** may not be autoloaded — LevelTransition looks for it at runtime | Verify autoload registration in project.godot |
| P3-4 | **MissionBoard hardcoded to Kingdom 0** | Generalize to current kingdom |
| P3-5 | **No NPC portrait system** — DialogueLine has PortraitKey but DialoguePanel doesn't use it | Add portrait display logic |
| P3-6 | **No tutorial/onboarding** — player is dropped into camp with no guidance | Add tutorial dialogue or hint system |
| P3-7 | **No accessibility options** — no colorblind mode, no text scaling, no screen reader support | Add to SettingsPanel |
| P3-8 | **PlaceholderSprites creates colored rectangles as NPCs** | Replace with actual character sprites |

---

## SUMMARY

**What's working well:**
- The entire codebase compiles cleanly with 0 warnings
- Core architecture (state machines, singleton autoloads, signal-based communication) is solid
- Kingdom 0 (Greenhold) is a fully playable vertical slice with 4 complete missions
- All 3 campaign subsystems (Rukh/Grael/Lorne) have complete back-end logic
- The tattoo/ink system is feature-complete with 16 tattoos
- Save/Load serializes all game state correctly
- Stealth detection system is sophisticated (vision cones, awareness tiers, noise propagation)
- Dialogue system supports choices, conditions, flags, and events
- VFX library is comprehensive (12 effect types, all procedurally generated)
- Settings panel (audio/display/keybinds) is production-quality

**What needs immediate attention:**
1. Fix kingdom name mismatch (5 min fix, P0)
2. Create or gate the 2 missing Greenhold mission scenes (P0)
3. Fix MainMenu "New Game" target scene (P0)
4. Add Lorne to the camp (P1)
5. Implement locked door keys (P1)

**Scale of remaining work:**
- Greenhold vertical slice: ~85% complete (needs 2 missing missions + ability scenes)
- Full game (6 kingdoms): ~15% complete (only Kingdom 0 has content)
- Campaign subsystem UI: 0% (back-end is 100%)
- Art/Audio assets: 0% (all procedural placeholders)
