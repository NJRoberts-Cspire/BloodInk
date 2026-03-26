# The World Forger — World, UI, Audio & VFX Agent

See `Scripts/World/CLAUDE.md` for the full agent description.

## Audio-specific notes

- **Zero audio files exist.** All AudioManager code is ready; do not block other work on audio absence.
- When audio files are added: place them in `Assets/Audio/` with subdirectories `SFX/`, `Music/`, `Ambient/`.
- Sound IDs are string constants — define them in `AudioConstants.cs` (create if it doesn't exist).
- `FootstepPlayer` determines surface from collision layer bits: Layer 1 = stone, Layer 2 = wood, Layer 3 = grass, Layer 4 = dirt.
- `AmbientZone`: set `TrackId` and `Volume` as exported fields; the manager handles crossfade on enter/exit.
