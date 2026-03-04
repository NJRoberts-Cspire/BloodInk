using Godot;

namespace BloodInk.World;

/// <summary>
/// A zone that updates the current area name, ambient music/tension,
/// and can trigger area-specific events (e.g., entering the labor camp,
/// entering Goldmanor grounds). Uses Area2D overlap with the player.
/// </summary>
public partial class AreaZone : Area2D
{
    [Signal] public delegate void PlayerEnteredAreaEventHandler(string areaName);
    [Signal] public delegate void PlayerExitedAreaEventHandler(string areaName);

    /// <summary>Display name for this area (shown in HUD).</summary>
    [Export] public string AreaName { get; set; } = "Unknown";

    /// <summary>Alert level modifier for this area (additive on room tension).</summary>
    [Export(PropertyHint.Range, "0,1,0.1")]
    public float TensionModifier { get; set; } = 0f;

    /// <summary>Whether this is a restricted zone (trespassing raises suspicion).</summary>
    [Export] public bool IsRestricted { get; set; } = false;

    /// <summary>Whether the player is currently in this zone.</summary>
    public bool PlayerInside { get; private set; } = false;

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 1 << 1; // Player layer.
        Monitoring = true;
        Monitorable = false;

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!body.IsInGroup("Player")) return;

        PlayerInside = true;
        EmitSignal(SignalName.PlayerEnteredArea, AreaName);
        GD.Print($"Entered area: {AreaName}{(IsRestricted ? " [RESTRICTED]" : "")}");
    }

    private void OnBodyExited(Node2D body)
    {
        if (!body.IsInGroup("Player")) return;

        PlayerInside = false;
        EmitSignal(SignalName.PlayerExitedArea, AreaName);
    }
}
