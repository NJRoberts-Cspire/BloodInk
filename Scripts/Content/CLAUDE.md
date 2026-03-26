# The Ink Oracle — Tattoo Progression Agent

See `Scripts/Ink/CLAUDE.md` for the full agent description.

## Content-specific notes

- `TattooRegistry.cs` is the single source of truth for all tattoo definitions.
- Current count: 16 tattoos defined across 6 slots and 4 temperaments.
- To add a tattoo: append a new `TattooData` entry to the registry list. Fields required:
  `Name, Description, Slot, PrimaryTemperament, InkGrade, InkCost, AbilityScenePath, StatBonuses`.
- `AbilityScenePath` may be `""` for passive tattoos (no active ability).
- Do not add game logic here — this file is data only.
