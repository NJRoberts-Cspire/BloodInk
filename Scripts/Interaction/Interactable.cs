using Godot;

namespace BloodInk.Interaction;

/// <summary>
/// Base class for all interactable objects in the world.
/// Extend this for doors, items, NPCs, levers, hiding spots, etc.
/// Uses Area2D overlap to detect interactable proximity.
/// </summary>
public partial class Interactable : Area2D
{
    [Signal] public delegate void InteractedEventHandler(Node2D interactor);
    [Signal] public delegate void FocusedEventHandler();
    [Signal] public delegate void UnfocusedEventHandler();

    /// <summary>Display name shown in the interact prompt.</summary>
    [Export] public string DisplayName { get; set; } = "Interact";

    /// <summary>Action verb shown in prompt (e.g. "Open", "Pick up", "Talk").</summary>
    [Export] public string ActionVerb { get; set; } = "Interact";

    /// <summary>Whether this interactable is currently usable.</summary>
    [Export] public bool IsEnabled { get; set; } = true;

    /// <summary>Whether this is a one-shot interaction (auto-disables after use).</summary>
    [Export] public bool OneShot { get; set; } = false;

    /// <summary>Whether the player is currently in range.</summary>
    public bool PlayerInRange { get; private set; } = false;

    /// <summary>Whether this interactable currently has focus (nearest to player).</summary>
    public bool IsFocused { get; private set; } = false;

    public override void _Ready()
    {
        CollisionLayer = 1 << 5; // Interactables layer (layer 6).
        CollisionMask = 1 << 1;  // Detect player (layer 2).
        Monitoring = true;
        Monitorable = true;

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        InteractableReady();
    }

    /// <summary>Override in subclasses for additional setup.</summary>
    protected virtual void InteractableReady() { }

    /// <summary>
    /// Called when the player presses the interact button while in range and focused.
    /// Override this to implement specific behavior.
    /// </summary>
    public virtual void OnInteract(Node2D interactor)
    {
        EmitSignal(SignalName.Interacted, interactor);

        if (OneShot)
            IsEnabled = false;
    }

    /// <summary>Set focus state (called by InteractionManager).</summary>
    public void SetFocused(bool focused)
    {
        if (IsFocused == focused) return;
        IsFocused = focused;

        if (focused)
            EmitSignal(SignalName.Focused);
        else
            EmitSignal(SignalName.Unfocused);
    }

    /// <summary>Get the prompt text to display.</summary>
    public virtual string GetPromptText()
    {
        return $"[E] {ActionVerb} {DisplayName}";
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("Player"))
        {
            PlayerInRange = true;
            InteractionManager.Instance?.RegisterNearbyInteractable(this);
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup("Player"))
        {
            PlayerInRange = false;
            SetFocused(false);
            InteractionManager.Instance?.UnregisterNearbyInteractable(this);
        }
    }
}
