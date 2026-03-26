using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloodInk.Progression;

/// <summary>
/// Alert levels within a kingdom — determines guard behaviour and difficulty.
/// </summary>
public enum AlertLevel
{
    Unaware,      // Guards patrol normally.
    Suspicious,   // Extra patrols, some doors locked.
    Alerted,      // Active search, reinforced positions.
    Lockdown      // All-out hunt, maximum security.
}

/// <summary>
/// Tracks the state of a single kingdom — alert level, killed targets,
/// discovered areas, and narrative flags.
/// </summary>
public partial class KingdomState : Node
{
    [Signal] public delegate void AlertLevelChangedEventHandler(int kingdomIndex, int alertLevel);
    [Signal] public delegate void TargetKilledEventHandler(string targetId);
    [Signal] public delegate void KingdomCompletedEventHandler(int kingdomIndex);

    /// <summary>Kingdom index (0-5).</summary>
    [Export] public int KingdomIndex { get; set; } = 0;

    /// <summary>Display name.</summary>
    [Export] public string KingdomName { get; set; } = "";

    /// <summary>Current alert level.</summary>
    public AlertLevel Alert { get; private set; } = AlertLevel.Unaware;

    /// <summary>Alert "heat" value 0-100. Thresholds trigger level changes.</summary>
    public float AlertHeat { get; private set; } = 0f;

    /// <summary>Set of killed target IDs.</summary>
    private readonly HashSet<string> _killedTargets = new();

    /// <summary>Set of discovered area/room IDs.</summary>
    private readonly HashSet<string> _discoveredAreas = new();

    /// <summary>Narrative flags set during this kingdom.</summary>
    private readonly HashSet<string> _narrativeFlags = new();

    /// <summary>All registered targets in this kingdom.</summary>
    private readonly Dictionary<string, TargetData> _targets = new();

    /// <summary>Whether the Edictbearer has been killed.</summary>
    public bool EdictbearerSlain { get; private set; } = false;

    /// <summary>Whether the kingdom is considered complete.</summary>
    public bool IsCompleted { get; private set; } = false;

    // ─── Target Registration ──────────────────────────────────────

    public void RegisterTarget(TargetData target)
    {
        _targets[target.Id] = target;
    }

    // ─── Alert System ─────────────────────────────────────────────

    /// <summary>
    /// Raise alert heat by an amount. Automatically escalates alert level.
    /// Called when bodies are found, alarms triggered, etc.
    /// </summary>
    public void RaiseAlert(float amount)
    {
        AlertHeat = Math.Min(100f, AlertHeat + amount);
        UpdateAlertLevel();
    }

    /// <summary>
    /// Reduce alert heat (time passing, diversions, etc.).
    /// </summary>
    public void ReduceAlert(float amount)
    {
        AlertHeat = Math.Max(0f, AlertHeat - amount);
        UpdateAlertLevel();
    }

    private void UpdateAlertLevel()
    {
        AlertLevel newLevel = AlertHeat switch
        {
            >= 80f => AlertLevel.Lockdown,
            >= 50f => AlertLevel.Alerted,
            >= 25f => AlertLevel.Suspicious,
            _ => AlertLevel.Unaware
        };

        if (newLevel != Alert)
        {
            Alert = newLevel;
            EmitSignal(SignalName.AlertLevelChanged, KingdomIndex, (int)Alert);
            GD.Print($"Kingdom {KingdomName}: Alert level → {Alert} (Heat: {AlertHeat:F0})");
        }
    }

    /// <summary>Natural heat decay per turn/cycle.</summary>
    public void DecayAlert(float amount = 5f)
    {
        ReduceAlert(amount);
    }

    // ─── Target Tracking ──────────────────────────────────────────

    /// <summary>
    /// Mark a target as killed. Returns the target data for ink/echo processing.
    /// </summary>
    public TargetData? KillTarget(string targetId)
    {
        if (!_targets.TryGetValue(targetId, out var target)) return null;
        if (_killedTargets.Contains(targetId)) return null;

        _killedTargets.Add(targetId);
        EmitSignal(SignalName.TargetKilled, targetId);
        GD.Print($"Target killed: {target.Name} ({target.Title})");

        if (target.IsEdictbearer)
        {
            EdictbearerSlain = true;
            GD.Print($"  EDICTBEARER SLAIN in {KingdomName}!");
        }

        // Killing raises alert.
        RaiseAlert(target.IsEdictbearer ? 30f : 15f);

        // Check completion.
        CheckCompletion();

        return target;
    }

    public bool IsTargetKilled(string targetId) => _killedTargets.Contains(targetId);

    public IEnumerable<TargetData> GetLivingTargets() =>
        _targets.Values.Where(t => !_killedTargets.Contains(t.Id));

    public IEnumerable<TargetData> GetKilledTargets() =>
        _killedTargets.Where(id => _targets.ContainsKey(id)).Select(id => _targets[id]);

    private void CheckCompletion()
    {
        bool allMandatoryDead = _targets.Values
            .Where(t => t.IsMandatory)
            .All(t => _killedTargets.Contains(t.Id));

        if (allMandatoryDead && !IsCompleted)
        {
            IsCompleted = true;
            EmitSignal(SignalName.KingdomCompleted, KingdomIndex);
            GD.Print($"Kingdom {KingdomName} COMPLETED.");
        }
    }

    // ─── Area Discovery ───────────────────────────────────────────

    public void DiscoverArea(string areaId)
    {
        _discoveredAreas.Add(areaId);
    }

    public bool IsAreaDiscovered(string areaId) => _discoveredAreas.Contains(areaId);

    // ─── Narrative Flags ──────────────────────────────────────────

    public void SetFlag(string flag) => _narrativeFlags.Add(flag);
    public bool HasFlag(string flag) => _narrativeFlags.Contains(flag);
    public void ClearFlag(string flag) => _narrativeFlags.Remove(flag);

    // ─── Serialization ────────────────────────────────────────────

    public Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            ["kingdom"] = KingdomIndex,
            ["name"] = KingdomName,
            ["alertHeat"] = AlertHeat,
            ["killed"] = _killedTargets.ToList(),
            ["areas"] = _discoveredAreas.ToList(),
            ["flags"] = _narrativeFlags.ToList(),
            ["edictSlain"] = EdictbearerSlain,
            ["completed"] = IsCompleted
        };
    }

    public void Deserialize(Dictionary<string, object> data)
    {
        // Clear existing state to prevent accumulation on repeated loads.
        _killedTargets.Clear();
        _discoveredAreas.Clear();
        _narrativeFlags.Clear();

        if (data.TryGetValue("alertHeat", out var ah))
        {
            AlertHeat = ah switch { int i => i, float f => f, double d => (float)d, _ => AlertHeat };
            UpdateAlertLevel();
        }
        if (data.TryGetValue("edictSlain", out var es) && es is bool esb) EdictbearerSlain = esb;
        if (data.TryGetValue("completed", out var comp) && comp is bool cb) IsCompleted = cb;

        if (data.TryGetValue("killed", out var killed) && killed is List<object> kList)
            foreach (var id in kList) { if (id is string s) _killedTargets.Add(s); }
        if (data.TryGetValue("areas", out var areas) && areas is List<object> aList)
            foreach (var id in aList) { if (id is string s) _discoveredAreas.Add(s); }
        if (data.TryGetValue("flags", out var flags) && flags is List<object> fList)
            foreach (var f in fList) { if (f is string s) _narrativeFlags.Add(s); }

        // Re-evaluate completion in case saved data has kills but IsCompleted=false
        // (e.g., save written before a target was registered, then targets were re-registered).
        // Only emit the signal if the flag was NOT already true in the saved data —
        // CheckCompletion guards internally with the IsCompleted flag, so re-running it
        // will only emit if we just determined it should be true now.
        if (!IsCompleted)
            CheckCompletion();
    }
}
