using Godot;

namespace BloodInk.Stealth;

/// <summary>
/// Defines a patrol route as a series of Path2D waypoints.
/// Attach a Path2D as a child and this component will provide iteration.
/// Guards walk between the points in sequence (optionally looping or ping-ponging).
/// </summary>
public partial class PatrolRoute : Node2D
{
    public enum PatrolMode
    {
        Loop,       // 0 → 1 → 2 → 0 → 1 → ...
        PingPong,   // 0 → 1 → 2 → 1 → 0 → 1 → ...
        Once        // 0 → 1 → 2 → stop
    }

    [Export] public PatrolMode Mode { get; set; } = PatrolMode.Loop;

    /// <summary>Wait time at each waypoint (seconds).</summary>
    [Export] public float WaitTimeAtPoint { get; set; } = 1.5f;

    /// <summary>Packed waypoints as an array of local positions.</summary>
    [Export] public Vector2[] Waypoints { get; set; } = System.Array.Empty<Vector2>();

    private int _currentIndex = 0;
    private int _direction = 1; // 1 = forward, -1 = backward (for PingPong).

    /// <summary>Whether the patrol has completed (only for Once mode).</summary>
    public bool IsComplete { get; private set; } = false;

    /// <summary>Get the current waypoint in global coordinates.</summary>
    public Vector2 GetCurrentWaypoint()
    {
        if (Waypoints.Length == 0) return GlobalPosition;
        return GlobalPosition + Waypoints[_currentIndex];
    }

    /// <summary>Advance to the next waypoint. Returns the new waypoint position (global).</summary>
    public Vector2 AdvanceToNext()
    {
        if (Waypoints.Length <= 1) return GetCurrentWaypoint();

        switch (Mode)
        {
            case PatrolMode.Loop:
                _currentIndex = (_currentIndex + 1) % Waypoints.Length;
                break;

            case PatrolMode.PingPong:
                _currentIndex += _direction;
                if (_currentIndex >= Waypoints.Length - 1)
                    _direction = -1;
                else if (_currentIndex <= 0)
                    _direction = 1;
                break;

            case PatrolMode.Once:
                if (_currentIndex < Waypoints.Length - 1)
                    _currentIndex++;
                else
                    IsComplete = true;
                break;
        }

        return GetCurrentWaypoint();
    }

    /// <summary>Reset to the first waypoint.</summary>
    public void Reset()
    {
        _currentIndex = 0;
        _direction = 1;
        IsComplete = false;
    }

    /// <summary>Total number of waypoints.</summary>
    public int Count => Waypoints.Length;

    // ─── Debug Drawing ────────────────────────────────────────────

    public override void _Draw()
    {
        if (!OS.IsDebugBuild() || Waypoints.Length < 2) return;

        var routeColor = new Color(0.3f, 0.6f, 1f, 0.3f);
        var pointColor = new Color(0.3f, 0.6f, 1f, 0.6f);

        for (int i = 0; i < Waypoints.Length; i++)
        {
            DrawCircle(Waypoints[i], 3f, pointColor);
            int next = (i + 1) % Waypoints.Length;
            if (next != 0 || Mode == PatrolMode.Loop)
                DrawLine(Waypoints[i], Waypoints[next], routeColor, 1f);
        }
    }
}
