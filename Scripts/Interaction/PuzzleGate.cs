using Godot;
using System.Collections.Generic;

namespace BloodInk.Interaction;

/// <summary>
/// A gate/barrier that opens when all linked puzzle conditions are met.
/// Can require: all switches pressed, all levers pulled, or a combination.
/// Zelda-style: solve the room puzzle → gate opens with a satisfying jingle.
/// </summary>
public partial class PuzzleGate : Node2D
{
    [Signal] public delegate void GateOpenedEventHandler();
    [Signal] public delegate void GateClosedEventHandler();

    /// <summary>Whether the gate is currently open (passable).</summary>
    public bool IsOpen { get; private set; } = false;

    /// <summary>How many conditions must be satisfied to open.</summary>
    [Export] public int RequiredConditions { get; set; } = 1;

    /// <summary>If true, gate stays open permanently once opened.</summary>
    [Export] public bool StayOpen { get; set; } = false;

    /// <summary>Color of the gate when closed.</summary>
    [Export] public Color ClosedColor { get; set; } = new(0.6f, 0.1f, 0.1f, 0.9f);

    /// <summary>Color of the gate when open.</summary>
    [Export] public Color OpenColor { get; set; } = new(0.1f, 0.1f, 0.1f, 0.15f);

    /// <summary>Width of the gate barrier in pixels.</summary>
    [Export] public float GateWidth { get; set; } = 32f;

    /// <summary>Height of the gate barrier in pixels.</summary>
    [Export] public float GateHeight { get; set; } = 16f;

    /// <summary>Whether the gate is oriented vertically (tall and narrow).</summary>
    [Export] public bool IsVertical { get; set; } = false;

    private StaticBody2D? _collisionBody;
    private ColorRect? _visual;
    private int _satisfiedConditions = 0;

    public override void _Ready()
    {
        float w = IsVertical ? GateHeight : GateWidth;
        float h = IsVertical ? GateWidth : GateHeight;

        // Collision body that blocks passage.
        _collisionBody = new StaticBody2D { Name = "GateBody" };
        _collisionBody.CollisionLayer = 1; // World layer.
        _collisionBody.CollisionMask = 0;

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(w, h) };
        _collisionBody.AddChild(shape);
        AddChild(_collisionBody);

        // Visual indicator.
        _visual = new ColorRect
        {
            Color = ClosedColor,
            Position = new Vector2(-w / 2, -h / 2),
            Size = new Vector2(w, h),
            ZIndex = 3
        };
        AddChild(_visual);
    }

    // ─── Connection API ───────────────────────────────────────────

    /// <summary>
    /// Connect a FloorSwitch to this gate. When pressed, it satisfies one condition.
    /// When released (if not StayPressed), it unsatisfies.
    /// </summary>
    public void LinkSwitch(FloorSwitch sw)
    {
        sw.Activated += () => OnConditionMet();
        sw.Deactivated += () => OnConditionLost();
    }

    /// <summary>
    /// Connect a Lever to this gate. When toggled ON, it satisfies one condition.
    /// When toggled OFF, it unsatisfies.
    /// </summary>
    public void LinkLever(Lever lever)
    {
        lever.Toggled += (isOn) =>
        {
            if (isOn) OnConditionMet();
            else OnConditionLost();
        };
    }

    /// <summary>
    /// Satisfy a condition programmatically (e.g., enemy killed, item used).
    /// </summary>
    public void SatisfyCondition()
    {
        OnConditionMet();
    }

    // ─── Internal ─────────────────────────────────────────────────

    private void OnConditionMet()
    {
        _satisfiedConditions++;
        GD.Print($"[PuzzleGate] {Name}: {_satisfiedConditions}/{RequiredConditions} conditions met.");

        if (_satisfiedConditions >= RequiredConditions && !IsOpen)
        {
            Open();
        }
    }

    private void OnConditionLost()
    {
        if (StayOpen && IsOpen) return;

        _satisfiedConditions = System.Math.Max(0, _satisfiedConditions - 1);
        GD.Print($"[PuzzleGate] {Name}: {_satisfiedConditions}/{RequiredConditions} conditions met.");

        if (_satisfiedConditions < RequiredConditions && IsOpen)
        {
            Close();
        }
    }

    private void Open()
    {
        IsOpen = true;

        if (_collisionBody != null)
            _collisionBody.ProcessMode = ProcessModeEnum.Disabled;

        if (_visual != null)
            _visual.Color = OpenColor;

        EmitSignal(SignalName.GateOpened);
        GD.Print($"[PuzzleGate] {Name} OPENED!");
    }

    private void Close()
    {
        IsOpen = false;

        if (_collisionBody != null)
            _collisionBody.ProcessMode = ProcessModeEnum.Inherit;

        if (_visual != null)
            _visual.Color = ClosedColor;

        EmitSignal(SignalName.GateClosed);
        GD.Print($"[PuzzleGate] {Name} CLOSED.");
    }

    /// <summary>Force open (e.g., via key or narrative event).</summary>
    public void ForceOpen()
    {
        _satisfiedConditions = RequiredConditions;
        Open();
    }
}
