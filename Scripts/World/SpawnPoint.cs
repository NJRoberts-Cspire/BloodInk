using Godot;

namespace BloodInk.World;

/// <summary>
/// Marks a position where the player can spawn when entering a room.
/// Place as a child of a "SpawnPoints" node or add to the "SpawnPoints" group.
/// The node NAME is the spawn point ID (e.g., "Default", "FromEast", "FromNorth").
/// </summary>
public partial class SpawnPoint : Marker2D
{
    /// <summary>Direction the player should face when spawning here.</summary>
    [Export] public TransitionDirection FacingDirection { get; set; } = TransitionDirection.South;

    public override void _Ready()
    {
        AddToGroup("SpawnPoints");
        // Invisible at runtime.
        Visible = false;
    }
}
