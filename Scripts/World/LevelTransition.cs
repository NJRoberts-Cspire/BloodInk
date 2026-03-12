using Godot;

namespace BloodInk.World;

/// <summary>
/// Direction the player exits / enters a room (used for spawn placement).
/// </summary>
public enum TransitionDirection
{
    North, South, East, West, Up, Down, Custom
}

/// <summary>
/// Place at room edges. When the player enters the trigger zone,
/// transitions to the target scene and spawns them at the matching
/// entry point on the other side.
/// </summary>
public partial class LevelTransition : Area2D
{
    /// <summary>Scene to load.</summary>
    [Export(PropertyHint.File, "*.tscn")]
    public string TargetScene { get; set; } = "";

    /// <summary>
    /// Spawn point name in the target scene (a Node2D with this name
    /// under the "SpawnPoints" group or as a direct child of the scene root).
    /// </summary>
    [Export] public string TargetSpawnPoint { get; set; } = "Default";

    /// <summary>Which direction this exit faces (for animation/facing).</summary>
    [Export] public TransitionDirection Direction { get; set; } = TransitionDirection.East;

    /// <summary>Whether to fade-to-black during transition.</summary>
    [Export] public bool UseFade { get; set; } = true;

    /// <summary>Fade duration in seconds.</summary>
    [Export] public float FadeDuration { get; set; } = 0.3f;

    /// <summary>If true, this transition is currently locked (needs a key/event).</summary>
    [Export] public bool IsLocked { get; set; } = false;

    private bool _transitioning = false;

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 1 << 1; // Player layer.
        Monitoring = true;
        Monitorable = false;

        BodyEntered += OnBodyEntered;
    }

    private async void OnBodyEntered(Node2D body)
    {
        if (_transitioning) return;
        if (!body.IsInGroup("Player")) return;
        if (IsLocked)
        {
            GD.Print("This way is locked.");
            return;
        }

        if (string.IsNullOrEmpty(TargetScene))
        {
            GD.PrintErr("LevelTransition: No target scene set.");
            return;
        }

        _transitioning = true;

        // Disable player input during transition to prevent actions/double-transitions.
        var player = body as CharacterBody2D;
        var sm = player?.GetNodeOrNull<Core.StateMachine>("StateMachine");
        if (sm != null) sm.ProcessMode = ProcessModeEnum.Disabled;
        player?.SetProcessUnhandledInput(false);

        // Store spawn info for the RoomManager to read after loading.
        RoomManager.PendingSpawnPoint = TargetSpawnPoint;
        RoomManager.PendingDirection = Direction;

        if (UseFade)
        {
            // Fade out.
            var fade = GetTree().Root.GetNodeOrNull<CanvasLayer>("FadeLayer");
            var rect = fade?.GetNodeOrNull<ColorRect>("ColorRect");
            if (rect != null)
            {
                try
                {
                    var tween = CreateTween();
                    tween.TweenProperty(rect, "color:a", 1.0f, FadeDuration);
                    await ToSignal(tween, Tween.SignalName.Finished);
                }
                catch (System.Exception)
                {
                    // Node freed during fade — abort gracefully.
                    return;
                }
            }
        }

        GetTree().ChangeSceneToFile(TargetScene);
    }
}
