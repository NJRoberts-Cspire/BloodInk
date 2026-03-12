using Godot;

namespace BloodInk.Interaction;

/// <summary>
/// A floor switch / pressure plate that activates when the player (or a push block)
/// stands on it. Emits Activated/Deactivated signals that can be connected to
/// PuzzleGates, doors, or other puzzle elements.
/// Zelda-style: step on to press, step off and it can stay pressed or release.
/// </summary>
public partial class FloorSwitch : Area2D
{
    [Signal] public delegate void ActivatedEventHandler();
    [Signal] public delegate void DeactivatedEventHandler();

    /// <summary>If true, the switch stays pressed permanently once activated.</summary>
    [Export] public bool StayPressed { get; set; } = false;

    /// <summary>Visual color when unpressed.</summary>
    [Export] public Color UnpressedColor { get; set; } = new(0.5f, 0.5f, 0.5f, 0.8f);

    /// <summary>Visual color when pressed.</summary>
    [Export] public Color PressedColor { get; set; } = new(0.2f, 0.8f, 0.2f, 0.9f);

    /// <summary>Whether the switch is currently pressed.</summary>
    public bool IsPressed { get; private set; } = false;

    private ColorRect? _visual;
    private int _bodiesOnSwitch = 0;

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = (1 << 1) | (1 << 6); // Player layer + PushBlock layer (layer 7).
        Monitoring = true;
        Monitorable = false;

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        // Visual indicator — a colored plate on the floor.
        _visual = new ColorRect
        {
            Color = UnpressedColor,
            Position = new Vector2(-8, -8),
            Size = new Vector2(16, 16),
            ZIndex = -5
        };
        AddChild(_visual);

        // Collision shape.
        if (GetChildCount() == 1) // Only visual, no shape yet
        {
            var shape = new CollisionShape2D();
            shape.Shape = new RectangleShape2D { Size = new Vector2(14, 14) };
            AddChild(shape);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (IsPressed && StayPressed) return;

        _bodiesOnSwitch++;
        if (!IsPressed)
        {
            IsPressed = true;
            UpdateVisual();
            EmitSignal(SignalName.Activated);
            GD.Print($"[Switch] {Name} pressed!");
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (StayPressed && IsPressed) return;

        _bodiesOnSwitch = System.Math.Max(0, _bodiesOnSwitch - 1);
        if (_bodiesOnSwitch == 0 && IsPressed)
        {
            IsPressed = false;
            UpdateVisual();
            EmitSignal(SignalName.Deactivated);
            GD.Print($"[Switch] {Name} released!");
        }
    }

    private void UpdateVisual()
    {
        if (_visual != null)
            _visual.Color = IsPressed ? PressedColor : UnpressedColor;
    }
}
