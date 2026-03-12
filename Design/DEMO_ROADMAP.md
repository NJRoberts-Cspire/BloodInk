# BloodInk — Demo Showcase Roadmap

**Created:** 2026-03-12  
**Target:** A polished, self-contained demo covering **Act 1 (The Greenhold)** — the Camp hub + 2-3 missions — playable start-to-finish in ~20-30 minutes.

---

## CURRENT STATE SNAPSHOT

| Area | Status | Notes |
|------|--------|-------|
| Core architecture | ✅ Solid | 109 scripts, 0 compile errors, clean state machines, full save/load |
| Kingdom 0 (Greenhold) | ⚠️ ~80% | 4 of 7 target missions exist; 2 reference missing scenes |
| Camp hub | ✅ Functional | 5 NPCs (Needlewise, Rukh, Grael, Senna, Lorne), MissionBoard, InkTent |
| Tattoo system | ✅ Complete | 16 tattoos, 6 slots, temperament tracking, evolution |
| Stealth system | ✅ Complete | Vision cones, awareness tiers, noise propagation, shadow/cover zones |
| Combat | ✅ Complete | Attack/dodge/crouch/stealth-kill, hitboxes, knockback, i-frames |
| Guard AI (7-state FSM) | ✅ Complete | Patrol → Alert → Chase → Attack → Search → Investigate, backup calls |
| Dialogue system | ✅ Complete | Typewriter, choices, flags, conditions, events |
| Campaign subsystems | ✅ Backend only | SpyNetwork, Warband, Crafting — zero UI |
| VFX | ✅ Complete | 12 procedural effects (blood, slash, ghost trail, ink swirl, etc.) |
| Audio | ❌ No assets | AudioManager code is complete but 0 game audio files exist |
| Art / Sprites | ❌ Placeholder only | 4 reference sheets; PlaceholderSprites generates colored rectangles |
| Resource files (.tres) | ❌ None | All data is hardcoded in registry classes |
| Ability scenes | ❌ None | 6 referenced, 0 exist |
| Echo scenes | ❌ None | 6 referenced, 0 exist |
| Stealth tools | ❌ None | Ash bombs, bone chimes, etc. — zero code |

---

## DEMO SCOPE DEFINITION

### What the demo MUST show:
1. **The hook** — Opening text/narration establishing the world (30 seconds)
2. **Camp hub** — Talk to NPCs, feel the orc resistance, get mission briefing
3. **Ink Tent** — Apply at least one tattoo (demonstrate the core progression mechanic)
4. **Mission 1: Goldmanor** — Full stealth sandbox mission (Lord Cowl assassination)
5. **Mission 2: LaborCamp or Chapel** — Second mission to show variety
6. **Core loop closure** — Return to camp, see the world change, get debriefed
7. **Enough juice** — Screen shake, hit stop, blood splatter, slash arcs, ambient sound

### What the demo can SKIP:
- Kingdoms 2-6 (not in scope)
- Blood Echo playback (flashback scenes)
- Campaign subsystem UIs (SpyNetwork, Warband, Crafting panels)
- NG+ system
- Endings / final choice
- The Assessor and Rootwarden missions (missing scenes — gate them cleanly)
- Active tattoo abilities (ShadowStep etc. — passive stat bonuses are enough)

---

## ROADMAP — 5 PHASES

---

### PHASE 1: FIX BLOCKERS & GATE INCOMPLETE CONTENT
**Goal:** Make the existing game loop crash-free and completable  
**Effort:** 1-2 days

| # | Task | Details | Priority |
|---|------|---------|----------|
| 1.1 | Gate missing Greenhold missions | The Assessor (Roads.tscn) and Rootwarden (RootwardenFarm.tscn) are referenced but don't exist. Either: (a) remove them from `GreenholdTargets.GetAll()` for the demo, or (b) mark them as "Intel Required — Unavailable" in MissionBoard UI. Option (a) is fastest. | P0 |
| 1.2 | Guard ability scene null checks | TattooRegistry references 6 ability scenes (ShadowStep etc.) that don't exist. Ensure `TattooSystem` doesn't crash when ability scene path is null/missing. Add null guard if not already present. | P0 |
| 1.3 | Guard echo scene null checks | BloodEchoManager.PlayEcho() will crash if echo scene doesn't exist. Add file-existence check or disable echo playback in demo. | P0 |
| 1.4 | Verify full playthrough: MainMenu → Camp → Mission → MissionComplete → Camp | Test the complete loop. Fix any transition crashes. | P0 |
| 1.5 | Fix Door.cs key system stub | Locked doors are unsolvable. For demo: either unlock all doors, or implement a simple key-pickup system keyed to `PickupItem.ItemId`. | P1 |

---

### PHASE 2: DEMO INTRO & NARRATIVE FRAMING
**Goal:** Give the player context — who they are, why they're here, what's at stake  
**Effort:** 2-3 days

| # | Task | Details | Priority |
|---|------|---------|----------|
| 2.1 | Create demo intro sequence | A `DemoIntro.tscn` scene that plays before Camp. Scrolling text or a series of illustrated cards explaining: the Edict, the dying orcs, the Hollow Hand, Vetch's mission. 4-6 screens. Fade transitions between them. Skip button. | P0 |
| 2.2 | Write intro narration text | Adapt Storyboard Part I + Vetch's profile into ~200 words of player-facing intro text. Dark, evocative, sets the mood. | P0 |
| 2.3 | Add "first visit" camp dialogue pass | The first time the player enters Camp, NPCs should have tutorial-adjacent dialogue: Needlewise explains tattoos, Rukh explains the mission board, Senna explains health/condition, Lorne grounds the emotional stakes. Check if CampDialogues.cs already covers this; enhance if needed. | P1 |
| 2.4 | Add demo end screen | After completing 2 missions and returning to camp, show a "Thanks for playing the BloodInk demo" screen with: missions completed, targets killed, tattoos applied, mercy/cruelty count. Link to wishlist/social. | P1 |

---

### PHASE 3: AUDIO — MINIMUM VIABLE SOUNDSCAPE
**Goal:** The game should not be silent. Audio is ~50% of atmosphere.  
**Effort:** 3-5 days

| # | Task | Details | Priority |
|---|------|---------|----------|
| 3.1 | Source/create core SFX pack | Minimum sounds needed: sword_slash (×2 variants), hit_impact (×2), footstep_stone (×3), footstep_grass (×3), dodge_whoosh, death_thud, door_open, door_locked, pickup_item, UI_click, UI_hover, UI_confirm, alert_sting (guard spots player), stealth_kill_slice, heartbeat_loop (low health). ~20 sounds. Use free CC0 packs (freesound.org, kenney.nl) or generate with sfxr/Chiptone. | P0 |
| 3.2 | Source/create ambient loops | Minimum: camp_fire_crickets (camp hub), wind_desolate (Ashwild), interior_stone_echo (Goldmanor/Chapel interiors), outdoor_birds_wheat (Greenhold fields). 4 loops. | P0 |
| 3.3 | Source/create music tracks | Minimum: main_menu_theme (melancholic, 1-2 min loop), camp_theme (quiet, hopeful-sad), mission_stealth (tense, low), mission_combat (urgent, drums), mission_complete_sting (short victory). 5 tracks. Use royalty-free dark fantasy music or commission. | P1 |
| 3.4 | Wire audio to AudioManager | Place .wav/.ogg files in `Assets/Audio/SFX/`, `Assets/Audio/Music/`, `Assets/Audio/Ambient/`. Create AudioManager helper methods or use existing bus system. Wire SFX to: sword swing, hit received, footsteps, doors, pickups, UI. Wire ambients to AmbientZone nodes. Wire music to scene transitions. | P1 |
| 3.5 | Add camp ambient audio | Camp.tscn needs AmbientZone nodes: fire crackling near center, wind at edges, distant animal calls. | P2 |

---

### PHASE 4: VISUAL POLISH — READABLE PLACEHOLDER → STYLIZED PLACEHOLDER
**Goal:** Not pixel-perfect art, but enough visual clarity and style that the demo *reads* correctly. A color-coded, icon-driven style is fine — it just needs intention.  
**Effort:** 5-8 days

| # | Task | Details | Priority |
|---|------|---------|----------|
| 4.1 | Establish visual style guide | Decide: is the demo using (a) the existing Calciumtrice tileset + tiny16 characters, (b) custom pixel art, or (c) a "stylized placeholder" approach (colored shapes with clear silhouettes + particle effects)? Option (a) or (c) is fastest. Document the decision. | P0 |
| 4.2 | Player character sprite | Even with placeholders, the player needs a recognizable silhouette. Either use zelda_character.png with recoloring, commission a simple 4-dir walk/attack/dodge sheet, or make a distinct placeholder (dark green orc with visible tusks, even at 16×16). | P0 |
| 4.3 | Guard enemy sprites | Guards need to be visually distinct from the player. Different color, helmet shape, or shield icon. At minimum, the existing placeholder system should use a clearly different palette. | P1 |
| 4.4 | Camp NPC differentiation | Each of the 5 camp NPCs needs a distinct look — even if it's just different colored rectangles with name labels. Needlewise=purple, Rukh=blue, Grael=red, Senna=green, Lorne=yellow. Verify PlaceholderSprites handles this. | P1 |
| 4.5 | Tilemap visual clarity | Walls, floors, shadow zones, cover zones, and restricted areas must be visually distinguishable. If using Calciumtrice tileset, verify the MapBuilder tile mapping looks acceptable. If not, adjust palette or add overlay indicators. | P1 |
| 4.6 | UI polish pass | MainMenu: add game title text, subtitle, atmospheric background. GameHUD: ensure health/ink/stealth indicators are readable. PauseMenu: verify all buttons work. DialoguePanel: add speaker name styling. MissionComplete: add visual flair. | P1 |
| 4.7 | Vision cone visualization | DetectionSensor has debug draw. For the demo, a subtle vision cone indicator (even a simple triangle) helps players understand stealth. Toggle via a "show detection" debug option or always-on in demo. | P2 |
| 4.8 | Camp scene layout | Camp.tscn needs spatial design: fire pit center, NPC tents around the perimeter, mission board as a physical object. Even with placeholder tiles, spatial layout communicates "this is a camp." | P2 |
| 4.9 | Screen effects | CRT shader is available as an addon. Consider enabling it for a retro-stylized look that masks placeholder art. Also: subtle vignette, desaturated palette with red accents (blood-ink). | P3 |

---

### PHASE 5: GAMEPLAY TUNING & DEMO FLOW
**Goal:** The 20-30 minute experience feels intentional, paced, and satisfying  
**Effort:** 3-5 days

| # | Task | Details | Priority |
|---|------|---------|----------|
| 5.1 | Tune guard patrol timing | Guards should feel dangerous but fair. Patrol wait times, walk speeds, detection ranges, and awareness thresholds need playtesting. Target: player can complete a mission in 5-10 minutes with careful play, 2-3 minutes if speedrunning. | P0 |
| 5.2 | Tune combat feel | Attack damage, enemy HP, knockback force, i-frame duration, hit stop duration. The game should feel snappy — 2-3 hits to kill a guard, 4-5 hits to kill the player. Stealth kills should be instant and visceral. | P0 |
| 5.3 | Tune tattoo rewards | After Goldmanor (killing Cowl), the player should have enough ink for 1-2 tattoos. These should feel impactful — noticeable speed boost, damage increase, or stealth bonus. Verify InkInventory drop amounts from GreenholdTargets. | P1 |
| 5.4 | Add mission variety cues | Each of the playable missions (Goldmanor, LaborCamp, Chapel, Barracks) should feel distinct: different guard counts, different environmental hazards, different map layouts. Verify ASCII maps create meaningfully different spaces. | P1 |
| 5.5 | Add hint system for first mission | On the first mission (Goldmanor), add subtle guidance: a dialogue line from Rukh on mission start ("The gardens are less guarded at the edges"), or a UI tooltip ("Press [crouch] to enter stealth mode"). Remove these for subsequent missions. | P2 |
| 5.6 | Add death/retry flow | On death: GameOver screen → Retry loads last save or restarts mission. Verify this loop works cleanly. Deaths should feel fair — the player should understand what killed them. | P1 |
| 5.7 | Demo analytics (optional) | Track: time to complete each mission, deaths per mission, tattoos chosen, targets killed vs spared, stealth vs combat ratio. Saves to a demo_analytics.json. Useful for tuning. | P3 |

---

## PHASE SUMMARY & TIMELINE

| Phase | Name | Effort Estimate | Depends On |
|-------|------|----------------|------------|
| **1** | Fix Blockers | 1-2 days | — |
| **2** | Narrative Framing | 2-3 days | Phase 1 |
| **3** | Audio | 3-5 days | Phase 1 |
| **4** | Visual Polish | 5-8 days | Phase 1 |
| **5** | Gameplay Tuning | 3-5 days | Phases 1-4 |

**Phases 2, 3, 4 can run in parallel** after Phase 1 is complete.

**Minimum viable demo:** Phases 1 + 5 (4-7 days) — functional but silent and placeholder-looking  
**Presentable demo:** Phases 1 + 2 + 3 + 5 (9-15 days) — has narrative context, audio, and tuned gameplay  
**Polished demo:** All phases (14-23 days) — showcase-ready with visual style and full audio

---

## WHAT ALREADY WORKS (NO CHANGES NEEDED)

These systems are complete and demo-ready as-is:

- **State machine architecture** — Player (6 states) and Guard (7 states) FSMs are clean
- **Save/Load** — Full JSON serialization of all game state
- **Tattoo system** — 16 tattoos, 6 slots, temperament tracking, stat aggregation
- **Ink economy** — Major/Lesser/Trace ink tracking with proper costs
- **Stealth detection** — Vision cones, awareness tiers, noise propagation, shadow/cover zones
- **Guard AI** — Patrol → Alert → Chase → Search → Investigate with inter-guard communication
- **Dialogue system** — Choices, conditions, flags, events, typewriter effect
- **VFX library** — Blood splatter, slash arcs, ghost trail, damage numbers, hit flash, screen shake, hit stop
- **Settings panel** — Audio sliders, display modes, keybind remapping with persistence
- **Camp dialogue** — Act 1 + post-Greenhold mission dialogue for all 5 NPCs
- **Kingdom progression** — Alert levels, heat system, target tracking per kingdom
- **MissionBoard** — Dynamic dialogue generation from target data

---

## DEMO-SPECIFIC DECISIONS NEEDED

| Decision | Options | Recommendation |
|----------|---------|----------------|
| How many missions in demo? | 2 (Goldmanor + 1), 3 (Goldmanor + 2), or all 4 existing | **3 missions** — Goldmanor (mandatory, teaches stealth), Chapel (indoor/outdoor mix), LaborCamp (moral complexity). Skip Barracks for demo pacing. |
| Is the Barracks mission in the demo? | Yes (4 total) or No (gated as "intel required") | **No** — gate it. 3 missions is enough content for a demo. Keeps players wanting more. |
| Are The Assessor/Rootwarden in the demo? | Show as locked, or remove entirely | **Show as locked** with "Intel Required" — implies depth beyond the demo |
| Do tattoo abilities work? | Implement 1-2 abilities, or passive-only | **Passive-only** for demo — stat bonuses are meaningful enough. Note: "Active abilities coming soon" in UI |
| Demo end trigger | After N missions, after killing Cowl, or manual | **After killing Lord Cowl** — the Edictbearer kill is the narrative climax of Act 1 |
| Mandatory mission order? | Free choice, or Goldmanor first | **Goldmanor must be available first** but don't force order — let players pick from available missions. Cowl unlocks only after 1-2 secondary targets are killed (existing `MandatoryKills` check). |

---

## FILES THAT NEED CHANGES (QUICK REFERENCE)

| File | Change Type | Phase |
|------|------------|-------|
| `Scripts/Content/GreenholdTargets.cs` | Gate Assessor + Rootwarden | Phase 1 |
| `Scripts/Ink/TattooSystem.cs` | Null-guard ability scene loading | Phase 1 |
| `Scripts/BloodEchoes/BloodEchoManager.cs` | Null-guard echo scene loading | Phase 1 |
| `Scripts/Interaction/Door.cs` | Implement key check or unlock | Phase 1 |
| `Scripts/UI/MainMenu.cs` | Add demo intro transition | Phase 2 |
| NEW: `Scenes/UI/DemoIntro.tscn` | Intro narration scene | Phase 2 |
| NEW: `Scripts/UI/DemoIntro.cs` | Intro narration script | Phase 2 |
| NEW: `Scenes/UI/DemoEnd.tscn` | Demo end/thanks scene | Phase 2 |
| NEW: `Scripts/UI/DemoEnd.cs` | Demo end screen script | Phase 2 |
| `Scripts/World/CampScene.cs` | Trigger demo end after Cowl kill | Phase 2 |
| NEW: `Assets/Audio/` | Audio asset directories + files | Phase 3 |
| `Scripts/Audio/AudioManager.cs` | Wire new audio files | Phase 3 |
| `Scripts/World/CampScene.cs` | Add AmbientZone nodes | Phase 3 |
| `Scripts/UI/MainMenu.cs` | Title styling, background | Phase 4 |
| `Scripts/UI/GameHUD.cs` | Readability pass | Phase 4 |
| `Scripts/Missions/MissionLevelBase.cs` | Guard tuning parameters | Phase 5 |
| `Scripts/Combat/HealthComponent.cs` | HP tuning | Phase 5 |
| `Scripts/Player/States/*.cs` | Timing/feel tuning | Phase 5 |

---

## SUCCESS CRITERIA

The demo is "showcase-ready" when a first-time player can:

1. ✅ Launch the game and understand the premise within 60 seconds
2. ✅ Navigate the camp, talk to NPCs, and feel invested in the characters
3. ✅ Select a mission from the board and deploy
4. ✅ Sneak through a mission using stealth mechanics (crouch, shadow zones, timing)
5. ✅ Engage in combat that feels responsive (hit stop, screen shake, knockback)
6. ✅ Execute a stealth kill that feels rewarding and distinct from open combat
7. ✅ Kill a target and return to camp with ink rewards
8. ✅ Apply a tattoo and feel the stat difference on the next mission
9. ✅ Kill Lord Cowl (Edictbearer) and experience the narrative payoff
10. ✅ Hear audio that establishes mood (ambient, SFX, at minimum)
11. ✅ See a demo end screen that leaves them wanting more
12. ✅ Save and load without issues
13. ✅ Not encounter any crashes or softlocks

---

*This roadmap scopes the minimum work to transform BloodInk from a technically-complete codebase into a presentable demo. The back-end systems (campaigns, NG+, tattoo evolution, kingdom progression) are already built — they just need content and UI to surface them. The demo's job is to prove the core loop works and the world is worth exploring.*
