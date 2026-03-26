# BloodInk — Agent Team

BloodInk is a 2D top-down stealth-assassination RPG built in **Godot 4.6 / C#**.
Core loop: infiltrate human kingdoms → kill targets → harvest ink from corpses → apply tattoos → unlock abilities.
Viewport: 640×360 (2× scaled to 1280×720). Genre: stealth-action RPG.

---

## The Seven

Each domain below is owned by a named agent. When working in a domain, that agent's CLAUDE.md
(in the relevant subdirectory) provides specialized context and architectural constraints.

| Agent | Domain | Directories |
|---|---|---|
| **The Hollow Hand** | Player & Core | `Scripts/Player/`, `Scripts/Core/` |
| **The Combat Forge** | Combat & Abilities | `Scripts/Combat/`, `Scripts/Abilities/` |
| **The Shadow Hand** | Stealth & Enemies | `Scripts/Stealth/`, `Scripts/Enemies/` |
| **The Ink Oracle** | Tattoo Progression | `Scripts/Ink/`, `Scripts/Content/` |
| **The Mission Scribe** | Levels & Missions | `Scripts/Missions/`, `Scripts/Tools/` |
| **The Story Keeper** | Narrative & Campaigns | `Scripts/Dialogue/`, `Scripts/Campaigns/`, `Scripts/Progression/`, `Scripts/BloodEchoes/` |
| **The World Forger** | World, UI, Audio, VFX | `Scripts/World/`, `Scripts/Interaction/`, `Scripts/Audio/`, `Scripts/VFX/`, `Scripts/UI/` |

---

## CURRENT PRIORITY — Demo Ship Mode

The demo must be playable start-to-finish: MainMenu → Camp → Mission → MissionComplete → Camp → kill Lord Cowl → DemoEnd screen. **Get it working and feeling good. Skip perfection.**

- Fix things that would crash or confuse a first-time player. Ignore everything else.
- Do not audit or refactor code outside the task at hand.
- When in doubt, do the simpler thing and move on.

Remaining demo work (in priority order):

1. Gate Assessor/Rootwarden missions as "Intel Required" in MissionBoard
2. DemoIntro narration scene (text cards before camp)
3. DemoEnd screen (after Cowl kill)
4. Door key system (simple pickup → unlock)
5. Guard timing + combat feel tuning
6. Death/retry loop verified clean
7. Hint line on Goldmanor mission start

---

## Cross-cutting Rules (all agents follow these)

- **Signals over direct calls** for cross-system communication. Do not reach across domain boundaries with hard references.
- **Autoloads** for global state: `GameManager`, `NoisePropagator`, `DialogueManager`, `InteractionManager`, `AudioManager`. Access them by name, never with `GetNode` paths.
- **StateMachine.cs** (`Scripts/Core/`) is the generic FSM — use it for any new state-driven behavior.
- **PlaceholderSprites.cs** generates colored rect stand-ins. Do not add real art references until assets exist.
- **No audio files exist yet.** AudioManager code is complete; do not add `AudioStreamPlayer` nodes that reference missing files.
- C# namespace: each directory maps to a matching namespace (e.g., `BloodInk.Combat`, `BloodInk.Stealth`).
- Scene files live in `Scenes/` with subdirectories mirroring `Scripts/`.
- No README files. Design docs live in `Design/`.
