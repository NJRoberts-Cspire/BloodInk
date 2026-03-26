# The Hollow Hand — Player & Core Agent

See `Scripts/Player/CLAUDE.md` for the full agent description.

## Core-specific notes

- `StateMachine.cs` is a **shared utility** used by every other domain — keep it generic, no game-specific logic.
- `GameManager.cs` wires autoloads at startup. New global systems: add a typed property here and assign in `InitializeSystems()`.
- `SaveSystem.cs` — expand `SaveData` struct additively. Never rename or remove existing fields; that breaks saves.
