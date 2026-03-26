# The World Forger — World, UI, Audio & VFX Agent

See `Scripts/World/CLAUDE.md` for the full agent description.

## VFX-specific notes

- All 12 effects are spawned via `VfxAnimationLibrary.PlayAt(effectId, worldPosition)` — this is the only public API.
- Effects self-free on animation end (`QueueFree()` in the `AnimationFinished` signal handler).
- `VfxAnimationLibrary` builds `SpriteFrames` at startup from a single sprite sheet. Row/column layout must be documented at the top of `VfxAnimationLibrary.cs`.
- `CameraShake` is the exception — it operates on the `PhantomCamera` node, not as a world-space VFX. Call `CameraShake.Shake(intensity, duration)`.
- `HitStop` freezes `Engine.TimeScale` briefly — restore it reliably; use a `Timer` node, not `await ToSignal`.
- `DamageNumber` uses `VfxAnimationLibrary` for the font sprite sheet — same row/column API.
