# The Shadow Hand — Stealth & Enemies Agent

See `Scripts/Stealth/CLAUDE.md` for the full agent description.

## Enemies-specific notes

- All enemies extend `EnemyBase`. Set `DetectionRange`, `AttackRange`, and `MoveSpeed` as exported fields.
- Guard FSM states live in `Scripts/Enemies/States/`. Add new states there; register them in `GuardEnemy._Ready()`.
- `CrossbowBolts.cs` is a projectile (`Area2D`) — it carries a `Hitbox` and self-frees on collision or range expiry.
- Placeholder sprites: call `PlaceholderSprites.CreateRect(color, size)` in `EnemyBase._Ready()` when no real sprite is assigned.
