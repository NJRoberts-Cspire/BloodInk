# The Story Keeper — Narrative & Campaigns Agent

See `Scripts/Dialogue/CLAUDE.md` for the full agent description.

## Progression-specific notes

- `PlayerChoices`: all external systems call `PlayerChoices.RecordChoice(ChoiceType)` — never set scores directly.
- `KingdomState`: one instance per kingdom (6 total). Stored in `GameManager`; accessed by kingdom enum key.
- `NewGamePlus`: tracks which NG+ modifiers are active. Unlock conditions live here; do not scatter them across level scripts.
- Binary story flags (`KnowsEdictTruth`, etc.) are the source of truth for late-game content gating — set them exactly once, never toggle.
