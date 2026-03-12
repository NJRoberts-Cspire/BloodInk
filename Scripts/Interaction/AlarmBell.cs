using Godot;

namespace BloodInk.Interaction;

/// <summary>
/// Environmental alarm that guards or the player can trigger.
/// When activated, raises the mission alert level and propagates a large noise.
/// Can be disabled/sabotaged by the player before it's triggered.
///
/// Place in guard posts, gates, and restricted areas.
/// </summary>
public partial class AlarmBell : Interactable
{
    /// <summary>Radius of the alarm noise propagation.</summary>
    [Export] public float AlarmRadius { get; set; } = 400f;

    /// <summary>Whether the alarm has been sabotaged by the player (can't be rung).</summary>
    public bool IsSabotaged { get; private set; }

    /// <summary>Whether the alarm is currently ringing.</summary>
    public bool IsRinging { get; private set; }

    /// <summary>How long the alarm rings before stopping.</summary>
    [Export] public float RingDuration { get; set; } = 10f;

    /// <summary>Interval between noise pulses while ringing.</summary>
    [Export] public float PulseInterval { get; set; } = 2f;

    private float _ringTimer;
    private float _pulseTimer;

    protected override void InteractableReady()
    {
        DisplayName = "Alarm Bell";
        ActionVerb = "Sabotage";
    }

    public override void _Process(double delta)
    {
        if (!IsRinging) return;

        _ringTimer += (float)delta;
        _pulseTimer += (float)delta;

        if (_pulseTimer >= PulseInterval)
        {
            _pulseTimer = 0;
            Stealth.NoisePropagator.Instance?.RaiseAlarm(GlobalPosition, AlarmRadius);
        }

        if (_ringTimer >= RingDuration)
        {
            IsRinging = false;
            GD.Print("[Alarm] Bell stopped ringing.");
        }
    }

    /// <summary>Player interaction — sabotage the bell.</summary>
    public override void OnInteract(Node2D interactor)
    {
        if (IsSabotaged)
        {
            GD.Print("[Alarm] Already sabotaged.");
            return;
        }

        IsSabotaged = true;
        ActionVerb = "Sabotaged";
        IsEnabled = false;

        // Visual feedback.
        Modulate = new Color(0.5f, 0.5f, 0.5f, 0.7f);

        GD.Print("[Alarm] Bell sabotaged — guards can't ring it.");
        base.OnInteract(interactor);
    }

    /// <summary>
    /// Called by guards when they want to ring the alarm.
    /// </summary>
    public void Ring()
    {
        if (IsSabotaged || IsRinging) return;

        IsRinging = true;
        _ringTimer = 0;
        _pulseTimer = 0;

        // Immediate alert.
        Stealth.MissionAlertManager.Instance?.ReportAlarm();
        Stealth.NoisePropagator.Instance?.RaiseAlarm(GlobalPosition, AlarmRadius);

        // Visual feedback — flash red.
        Modulate = new Color(1f, 0.3f, 0.3f);

        GD.Print("[Alarm] Bell ringing!");
    }

    public override string GetPromptText()
    {
        if (IsSabotaged) return "Sabotaged";
        if (IsRinging) return "Ringing...";
        return "[E] Sabotage Alarm";
    }
}
