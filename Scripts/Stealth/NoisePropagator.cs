using Godot;
using System.Collections.Generic;
using System.Linq;
using BloodInk.Abilities;

namespace BloodInk.Stealth;

/// <summary>
/// Global noise propagation system. When the player (or anything) makes noise,
/// this system notifies all DetectionSensors within range.
/// Attach as an autoload or to the GameManager.
/// </summary>
public partial class NoisePropagator : Node
{
    public static NoisePropagator? Instance { get; private set; }

    /// <summary>All registered detection sensors in the current scene.</summary>
    private readonly List<DetectionSensor> _sensors = new();

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Register a sensor (called by DetectionSensor._Ready).</summary>
    public void RegisterSensor(DetectionSensor sensor)
    {
        if (!_sensors.Contains(sensor))
            _sensors.Add(sensor);
    }

    /// <summary>Unregister a sensor (called by DetectionSensor._ExitTree).</summary>
    public void UnregisterSensor(DetectionSensor sensor)
    {
        _sensors.Remove(sensor);
    }

    /// <summary>
    /// Propagate a noise event to all sensors. Each sensor decides if it can hear it.
    /// Raycasts to check for wall occlusion — noise is blocked by solid geometry.
    /// </summary>
    /// <param name="position">World position of the noise source.</param>
    /// <param name="radius">Radius in pixels the noise can travel.</param>
    /// <param name="type">Type of noise (optional; used for future filtering by noise type).</param>
    public void PropagateNoise(Vector2 position, float radius, NoiseType type = NoiseType.Movement)
    {
        if (radius <= 0f) return;

        // Clean up any freed sensors.
        _sensors.RemoveAll(s => s == null || !IsInstanceValid(s));

        // Iterate a snapshot to avoid modification during iteration.
        var snapshot = _sensors.ToArray();
        float radiusSq = radius * radius;
        foreach (var sensor in snapshot)
        {
            if (!IsInstanceValid(sensor)) continue;

            // Fast reject: skip sensors that can't possibly hear this noise.
            if (sensor.GlobalPosition.DistanceSquaredTo(position) > radiusSq) continue;

            // Wall occlusion: raycast from noise source to sensor.
            // If a wall blocks the path, attenuate the effective radius.
            float effectiveRadius = GetOccludedRadius(position, sensor.GlobalPosition, radius);
            if (effectiveRadius > 0f)
                sensor.OnNoiseAtPosition(position, effectiveRadius);
        }
    }

    /// <summary>
    /// Return all valid registered sensors within <paramref name="radius"/> of <paramref name="center"/>.
    /// Uses a squared-distance check — no raycasts involved.
    /// </summary>
    public IEnumerable<DetectionSensor> GetSensorsInRadius(Vector2 center, float radius)
    {
        float radiusSq = radius * radius;
        foreach (var sensor in _sensors)
            if (IsInstanceValid(sensor) && sensor.GlobalPosition.DistanceSquaredTo(center) <= radiusSq)
                yield return sensor;
    }

    /// <summary>
    /// Check how much a wall occludes noise between two points.
    /// Returns the effective radius after wall attenuation, or 0 if fully blocked.
    /// Each wall the ray passes through reduces effective radius by WallAttenuation.
    /// </summary>
    private float GetOccludedRadius(Vector2 from, Vector2 to, float baseRadius)
    {
        var spaceState = GetTree()?.Root?.GetWorld2D()?.DirectSpaceState;
        if (spaceState == null) return baseRadius;

        float distance = from.DistanceTo(to);
        if (distance > baseRadius) return 0f;

        // Cast a ray on the world collision layer (bit 0).
        var query = PhysicsRayQueryParameters2D.Create(from, to, 1); // Layer 1 = world.
        var result = spaceState.IntersectRay(query);

        if (result == null || result.Count == 0)
            return baseRadius; // No wall obstruction.

        // Wall hit — attenuate. Each wall reduces effective radius by 60%.
        return baseRadius * WallAttenuation;
    }

    /// <summary>
    /// Multiplier applied to noise radius when a wall blocks line-of-sight.
    /// 0.4 means walls block 60% of noise.
    /// </summary>
    public static float WallAttenuation { get; set; } = 0.4f;

    /// <summary>
    /// Raise an alarm — all sensors in the specified radius are force-engaged.
    /// </summary>
    public void RaiseAlarm(Vector2 position, float alarmRadius = 400f)
    {
        _sensors.RemoveAll(s => s == null || !IsInstanceValid(s));

        // Iterate a snapshot to avoid modification during iteration.
        var snapshot = _sensors.ToArray();
        foreach (var sensor in snapshot)
        {
            if (IsInstanceValid(sensor) && sensor.GlobalPosition.DistanceTo(position) <= alarmRadius)
            {
                sensor.ForceEngage();
            }
        }

        // An alarm blows any active disguise — guards recognize the threat.
        var player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
        if (player != null && player.GlobalPosition.DistanceTo(position) <= alarmRadius)
        {
            foreach (var child in player.GetChildren())
            {
                if (child is Abilities.MaskOfAshAbility mask)
                {
                    mask.BreakMask();
                    break;
                }
            }
        }

        GD.Print($"ALARM raised at {position} — radius {alarmRadius}");
    }
}
