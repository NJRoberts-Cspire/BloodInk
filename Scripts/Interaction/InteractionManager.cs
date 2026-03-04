using Godot;
using System.Collections.Generic;
using System.Linq;

namespace BloodInk.Interaction;

/// <summary>
/// Manages interaction — tracks nearby interactables, focuses the closest one,
/// and dispatches interact events when the player presses the interact button.
/// Attach as an autoload or as a child of the player.
/// </summary>
public partial class InteractionManager : Node
{
    [Signal] public delegate void InteractPromptChangedEventHandler(string promptText);
    [Signal] public delegate void InteractPromptHiddenEventHandler();

    public static InteractionManager? Instance { get; private set; }

    /// <summary>All interactables currently in range of the player.</summary>
    private readonly List<Interactable> _nearbyInteractables = new();

    /// <summary>Currently focused interactable (closest enabled one).</summary>
    public Interactable? FocusedInteractable { get; private set; }

    /// <summary>Reference to the player node (for distance calculations).</summary>
    private Node2D? _player;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        if (_player == null)
        {
            _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
            if (_player == null) return;
        }

        UpdateFocus();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("interact") && FocusedInteractable != null)
        {
            if (FocusedInteractable.IsEnabled && _player != null)
            {
                FocusedInteractable.OnInteract(_player);
                GetViewport().SetInputAsHandled();
            }
        }
    }

    // ─── Registration ─────────────────────────────────────────────

    public void RegisterNearbyInteractable(Interactable interactable)
    {
        if (!_nearbyInteractables.Contains(interactable))
            _nearbyInteractables.Add(interactable);
    }

    public void UnregisterNearbyInteractable(Interactable interactable)
    {
        _nearbyInteractables.Remove(interactable);
        if (FocusedInteractable == interactable)
        {
            FocusedInteractable?.SetFocused(false);
            FocusedInteractable = null;
            EmitSignal(SignalName.InteractPromptHidden);
        }
    }

    // ─── Focus Management ─────────────────────────────────────────

    private void UpdateFocus()
    {
        // Clean up freed interactables.
        _nearbyInteractables.RemoveAll(i => i == null || !IsInstanceValid(i));

        // Find the closest enabled interactable.
        Interactable? closest = null;
        float closestDist = float.MaxValue;

        foreach (var interactable in _nearbyInteractables)
        {
            if (!interactable.IsEnabled || !interactable.PlayerInRange) continue;

            float dist = _player!.GlobalPosition.DistanceTo(interactable.GlobalPosition);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = interactable;
            }
        }

        // Update focus.
        if (closest != FocusedInteractable)
        {
            FocusedInteractable?.SetFocused(false);
            FocusedInteractable = closest;
            FocusedInteractable?.SetFocused(true);

            if (FocusedInteractable != null)
                EmitSignal(SignalName.InteractPromptChanged, FocusedInteractable.GetPromptText());
            else
                EmitSignal(SignalName.InteractPromptHidden);
        }
    }
}
