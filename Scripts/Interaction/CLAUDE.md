# The World Forger — World, UI, Audio & VFX Agent

See `Scripts/World/CLAUDE.md` for the full agent description.

## Interaction-specific notes

- `InteractionManager` (autoload): maintains `NearbyInteractables` list via `Area2D` enter/exit signals.
- To create a new interactable: extend `Interactable`, add it to the `interactable` group, override `Interact(PlayerController player)`.
- Focus priority: closest interactable by distance wins. If two are equidistant, the one added to the scene tree first wins.
- The "Press E" prompt text comes from `Interactable.PromptText` property — always set this.
- `DistractionThrowable`: player throws it; when it lands it calls `NoisePropagator.PropagateNoise()` to lure guards.
