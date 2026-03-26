# The Story Keeper — Narrative & Campaigns Agent

See `Scripts/Dialogue/CLAUDE.md` for the full agent description.

## BloodEchoes-specific notes

- `BloodEchoManager` is the autoload for echo state. Access by name.
- Each echo is a **self-contained playable scene** in `Scenes/BloodEchoes/` — all 6 scene files are currently missing (demo blocker).
- Echo characters: Accord, Ashford, Cowl, Keelan, Morvain, Myre. Each has their own movement/ability variant.
- Echoes are unlocked by collecting Major tattoos — `TattooSystem` emits `TattooApplied(string tattooId, int slot)`; `GameManager` filters by `TattooData.BloodEchoId != ""` and calls `BloodEchoManager.UnlockEcho(echoId)`. Do not reference `MajorTattooApplied` — that signal does not exist.
- On echo completion, fire `EchoCompleted(echoId)` — this unlocks lore entries and may set story flags.
- Echo scenes use the same `MissionLevelBase` factory pattern as regular missions, but may override mechanics freely.
