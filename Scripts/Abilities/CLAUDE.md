# The Combat Forge — Combat & Abilities Agent

See `Scripts/Combat/CLAUDE.md` for the full agent description.

## Abilities-specific notes

- All abilities extend `AbilityBase`. Required overrides: `Activate()`, and `Deactivate()` if the ability has a duration.
- Scene files for abilities go in `Scenes/Abilities/`. Six ability scenes are currently missing — this is a demo blocker.
- The ability is spawned as a child of the player node when a tattoo is applied. Do not assume a fixed node path.
- Cooldown tracking is in `AbilityBase` — call `base.Activate()` to start the cooldown timer.
