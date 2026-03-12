using Godot;

namespace BloodInk.Interaction;

/// <summary>
/// A wall-mounted lever the player can interact with to toggle state.
/// Connects to PuzzleGates, doors, and other puzzle elements via signals.
/// Zelda-style: pull to toggle, can be reset or permanent.
/// </summary>
public partial class Lever : Interactable
{
    [Signal] public delegate void ToggledEventHandler(bool isOn);

    /// <summary>Whether the lever is currently in the ON position.</summary>
    [Export] public bool IsOn { get; set; } = false;

    /// <summary>If true, the lever can only be pulled once (one-way).</summary>
    [Export] public bool OneWay { get; set; } = false;

    /// <summary>Color when OFF.</summary>
    [Export] public Color OffColor { get; set; } = new(0.6f, 0.3f, 0.1f, 1.0f);

    /// <summary>Color when ON.</summary>
    [Export] public Color OnColor { get; set; } = new(0.1f, 0.7f, 0.3f, 1.0f);

    private ColorRect? _handleVisual;
    private bool _hasBeenPulled = false;

    protected override void InteractableReady()
    {
        ActionVerb = IsOn ? "Push" : "Pull";
        DisplayName = "Lever";

        // Base rectangle (lever mount).
        var mount = new ColorRect
        {
            Color = new Color(0.3f, 0.25f, 0.2f, 1.0f),
            Position = new Vector2(-4, -10),
            Size = new Vector2(8, 20),
            ZIndex = 1
        };
        AddChild(mount);

        // Handle — moves up/down to show state.
        _handleVisual = new ColorRect
        {
            Color = IsOn ? OnColor : OffColor,
            Position = IsOn ? new Vector2(-5, -12) : new Vector2(-5, 4),
            Size = new Vector2(10, 8),
            ZIndex = 2
        };
        AddChild(_handleVisual);
    }

    public override void OnInteract(Node2D interactor)
    {
        if (OneWay && _hasBeenPulled) return;

        IsOn = !IsOn;
        _hasBeenPulled = true;
        ActionVerb = IsOn ? "Push" : "Pull";

        UpdateVisual();
        EmitSignal(SignalName.Toggled, IsOn);

        base.OnInteract(interactor);
        GD.Print($"[Lever] {Name} → {(IsOn ? "ON" : "OFF")}");
    }

    private void UpdateVisual()
    {
        if (_handleVisual != null)
        {
            _handleVisual.Color = IsOn ? OnColor : OffColor;
            _handleVisual.Position = IsOn ? new Vector2(-5, -12) : new Vector2(-5, 4);
        }
    }

    public override string GetPromptText()
    {
        if (OneWay && _hasBeenPulled) return "[E] (Already pulled)";
        return base.GetPromptText();
    }
}
