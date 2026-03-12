using Godot;

namespace BloodInk.Stealth;

/// <summary>
/// Mission-level alert system. Tracks a global alert level that escalates
/// as the player is detected, corpses are found, or alarms are triggered.
/// Alert level affects guard behavior, reinforcements, and mission outcomes.
///
/// Alert levels:
///   0 = Unaware — normal patrols
///   1 = Suspicious — guards patrol faster, check hiding spots
///   2 = Alerted — guards actively search, restricted zones enforced
///   3 = Hunted — all guards in pursuit mode, doors locked
///   4 = Siege — reinforcements arrive, Edictbearer retreats to safe room
///
/// Add as a child of each mission root node (e.g., LaborCamp, Goldmanor).
/// </summary>
public partial class MissionAlertManager : Node
{
    [Signal] public delegate void AlertLevelChangedEventHandler(int oldLevel, int newLevel);
    [Signal] public delegate void ReinforcementsCalledEventHandler();

    /// <summary>Current global alert level (0–4).</summary>
    public int AlertLevel { get; private set; } = 0;

    /// <summary>Maximum alert level for this mission.</summary>
    [Export] public int MaxAlertLevel { get; set; } = 4;

    /// <summary>Alert points per detection event.</summary>
    [Export] public int DetectionAlertPoints { get; set; } = 25;

    /// <summary>Alert points per corpse discovered.</summary>
    [Export] public int CorpseAlertPoints { get; set; } = 40;

    /// <summary>Alert points per alarm triggered.</summary>
    [Export] public int AlarmAlertPoints { get; set; } = 60;

    /// <summary>Points needed to raise alert by one level.</summary>
    [Export] public int PointsPerLevel { get; set; } = 50;

    /// <summary>Alert decays at this rate per second when no events occur.</summary>
    [Export] public float DecayRatePerSecond { get; set; } = 2f;

    /// <summary>Seconds after last event before decay begins.</summary>
    [Export] public float DecayCooldown { get; set; } = 15f;

    /// <summary>Current singleton — one per active mission scene.</summary>
    public static MissionAlertManager? Instance { get; private set; }

    private float _alertPoints;
    private float _timeSinceLastEvent;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    public override void _Process(double delta)
    {
        _timeSinceLastEvent += (float)delta;

        // Decay alert points after cooldown (but never below current level threshold).
        if (_timeSinceLastEvent >= DecayCooldown && _alertPoints > 0)
        {
            float floor = AlertLevel > 0 ? (AlertLevel - 1) * PointsPerLevel : 0;
            _alertPoints = Mathf.Max(floor, _alertPoints - DecayRatePerSecond * (float)delta);
            RecalculateLevel();
        }
    }

    /// <summary>Report a player detection event.</summary>
    public void ReportDetection()
    {
        AddAlertPoints(DetectionAlertPoints);
    }

    /// <summary>Report a corpse discovery.</summary>
    public void ReportCorpseFound()
    {
        AddAlertPoints(CorpseAlertPoints);
    }

    /// <summary>Report an alarm activation.</summary>
    public void ReportAlarm()
    {
        AddAlertPoints(AlarmAlertPoints);
    }

    /// <summary>Manually set alert level (for scripted events).</summary>
    public void SetAlertLevel(int level)
    {
        int clamped = Mathf.Clamp(level, 0, MaxAlertLevel);
        if (clamped == AlertLevel) return;
        int old = AlertLevel;
        AlertLevel = clamped;
        _alertPoints = clamped * PointsPerLevel;
        EmitSignal(SignalName.AlertLevelChanged, old, AlertLevel);
    }

    private void AddAlertPoints(int points)
    {
        _alertPoints += points;
        _timeSinceLastEvent = 0f;
        RecalculateLevel();
    }

    private void RecalculateLevel()
    {
        int newLevel = Mathf.Clamp((int)(_alertPoints / PointsPerLevel), 0, MaxAlertLevel);
        if (newLevel != AlertLevel)
        {
            int old = AlertLevel;
            AlertLevel = newLevel;
            EmitSignal(SignalName.AlertLevelChanged, old, newLevel);

            if (newLevel >= 4)
                EmitSignal(SignalName.ReinforcementsCalled);

            GD.Print($"[Alert] Mission alert level: {old} → {newLevel} ({_alertPoints:F0} pts)");
        }
    }

    /// <summary>
    /// Get patrol speed multiplier for current alert level.
    /// Guards use this to scale their patrol/search speed.
    /// </summary>
    public float GetSpeedMultiplier() => AlertLevel switch
    {
        0 => 1.0f,
        1 => 1.15f,
        2 => 1.3f,
        3 => 1.5f,
        _ => 1.5f
    };
}
