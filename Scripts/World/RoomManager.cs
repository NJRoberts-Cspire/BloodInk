using Godot;
using System.Linq;

namespace BloodInk.World;

/// <summary>
/// Manages room loading: spawns the player at the correct entry point
/// after a scene transition, runs fade-in, and tracks the current area.
/// Attach to each room's root node (or use as autoload).
/// </summary>
public partial class RoomManager : Node
{
    // ─── Pending transition data (set by LevelTransition) ─────────
    public static string PendingSpawnPoint { get; set; } = "Default";
    public static TransitionDirection PendingDirection { get; set; } = TransitionDirection.East;

    /// <summary>Unique name for this room (for save state / revisit).</summary>
    [Export] public string RoomId { get; set; } = "";

    /// <summary>Fade-in duration after entering the room.</summary>
    [Export] public float FadeInDuration { get; set; } = 0.3f;

    /// <summary>Ambient alert level modifier for this room (0 = calm, 1 = tense).</summary>
    [Export(PropertyHint.Range, "0,1,0.1")]
    public float AmbientTension { get; set; } = 0f;

    public override void _Ready()
    {
        SpawnPlayer();
        FadeIn();
    }

    // ─── Player Spawn ─────────────────────────────────────────────

    private void SpawnPlayer()
    {
        var player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
        if (player == null)
        {
            GD.PrintErr("RoomManager: No player found in 'Player' group.");
            return;
        }

        // Find the spawn point.
        Node2D? spawnNode = FindSpawnPoint(PendingSpawnPoint);
        if (spawnNode == null)
        {
            spawnNode = FindSpawnPoint("Default");
        }

        if (spawnNode != null)
        {
            player.GlobalPosition = spawnNode.GlobalPosition;

            // Face the player in the correct direction.
            var facing = DirectionToVector(PendingDirection);
            if (player is Player.PlayerController pc)
            {
                pc.FacingDirection = facing;
            }
        }

        // Reset pending data.
        PendingSpawnPoint = "Default";
        PendingDirection = TransitionDirection.East;
    }

    private Node2D? FindSpawnPoint(string pointName)
    {
        // Check direct children named "SpawnPoints".
        var container = GetNodeOrNull("SpawnPoints");
        if (container != null)
        {
            var point = container.GetNodeOrNull<Node2D>(pointName);
            if (point != null) return point;
        }

        // Check "SpawnPoints" group across the tree.
        var spawnNodes = GetTree().GetNodesInGroup("SpawnPoints");
        foreach (var node in spawnNodes)
        {
            if (node is Node2D n2d && n2d.Name == pointName)
                return n2d;
        }

        // Fallback: any Node2D whose name matches.
        return GetNodeOrNull<Node2D>(pointName);
    }

    private static Vector2 DirectionToVector(TransitionDirection dir) => dir switch
    {
        TransitionDirection.North => Vector2.Up,
        TransitionDirection.South => Vector2.Down,
        TransitionDirection.East  => Vector2.Right,
        TransitionDirection.West  => Vector2.Left,
        _                         => Vector2.Down
    };

    // ─── Fade ─────────────────────────────────────────────────────

    private async void FadeIn()
    {
        var fade = GetTree().Root.GetNodeOrNull<CanvasLayer>("FadeLayer");
        var rect = fade?.GetNodeOrNull<ColorRect>("ColorRect");
        if (rect == null) return;

        rect.Color = new Color(0, 0, 0, 1); // start fully black
        var tween = CreateTween();
        tween.TweenProperty(rect, "color:a", 0.0f, FadeInDuration);
        try
        {
            await ToSignal(tween, Tween.SignalName.Finished);
        }
        catch (System.Exception)
        {
            // Tween or node freed during scene transition — safe to ignore.
        }
    }
}
