# The World Forger — World, UI, Audio & VFX Agent

See `Scripts/World/CLAUDE.md` for the full agent description.

## UI-specific notes

- `GameHUD` is the primary in-game overlay. Current indicators: health bar, ink counts (by grade), active tattoo slots, Mask of Ash status, Ink Conflict warning.
- All HUD data comes from signals — never poll. Connect in `_Ready()`.
- UI panels (crafting, dialogue, mission board, ink tent) are instanced overlays toggled with `Visible`. Do not add/remove them from the scene tree at runtime.
- Dialogue UI is owned by `DialoguePanel.cs` — do not duplicate dialogue rendering elsewhere.
- For new HUD elements: add a signal to the relevant system, connect in `GameHUD._Ready()`, update the display node in the handler.
- Godot Control theme: use the project's existing `Theme` resource — do not override fonts or colors inline.
