# The Mission Scribe — Levels & Missions Agent

See `Scripts/Missions/CLAUDE.md` for the full agent description.

## Tools-specific notes

- `PlaceholderSprites.cs` is used project-wide until real art arrives. It returns a `ColorRect` or `Sprite2D` with a solid color. Call `PlaceholderSprites.CreateRect(color, size)`.
- `MapBuilder.cs` generates tile-based level geometry procedurally. Feed it a 2D int array (0 = floor, 1 = wall) and it builds the TileMap layer.
- `SpriteSheetAnimator.cs` is a utility shared with VFX — do not add game logic here.
- These are pure utilities with no state — they can be called from any domain without coupling concerns.
