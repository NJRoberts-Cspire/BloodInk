using Godot;
using System.Collections.Generic;

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
    /// </summary>
    public void PropagateNoise(Vector2 position, float radius)
    {
        if (radius <= 0f) return;

        // Clean up any freed sensors.
        _sensors.RemoveAll(s => s == null || !IsInstanceValid(s));

        foreach (var sensor in _sensors)
        {
            sensor.OnNoiseAtPosition(position, radius);
        }
    }

    /// <summary>
    /// Raise an alarm — all sensors in the specified radius are force-engaged.
    /// </summary>
    public void RaiseAlarm(Vector2 position, float alarmRadius = 400f)
    {
        _sensors.RemoveAll(s => s == null || !IsInstanceValid(s));

        foreach (var sensor in _sensors)
        {
            if (sensor.GlobalPosition.DistanceTo(position) <= alarmRadius)
            {
                sensor.ForceEngage();
            }
        }

        GD.Print($"ALARM raised at {position} — radius {alarmRadius}");
    }
}
