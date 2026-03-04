using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloodInk.Campaigns.Grael;

/// <summary>
/// Manages Grael's warband — recruitment, upgrades, morale, and raid preparation.
/// Grael's campaign is about building a strong warband and smashing through
/// kingdom defences to create openings for Vetch.
/// </summary>
public partial class WarbandManager : Node
{
    [Signal] public delegate void WarriorRecruitedEventHandler(string warriorId);
    [Signal] public delegate void WarbandMoraleChangedEventHandler(float averageMorale);

    /// <summary>All warriors in the warband.</summary>
    private readonly Dictionary<string, WarriorData> _warriors = new();

    /// <summary>Maximum warband size. Increases as Grael's reputation grows.</summary>
    public int MaxWarbandSize { get; set; } = 8;

    /// <summary>Grael's reputation / renown. Affects recruitment and morale.</summary>
    public int Renown { get; set; } = 0;

    /// <summary>Resources available for equipping warriors.</summary>
    public int WarSupplies { get; set; } = 0;

    // ─── Recruitment ──────────────────────────────────────────────

    /// <summary>Recruit a warrior into the warband.</summary>
    public bool RecruitWarrior(WarriorData warrior)
    {
        int livingCount = _warriors.Values.Count(w => !w.IsDead);
        if (livingCount >= MaxWarbandSize)
        {
            GD.PrintErr("Warband is full.");
            return false;
        }

        _warriors[warrior.Id] = warrior;
        EmitSignal(SignalName.WarriorRecruited, warrior.Id);
        GD.Print($"Warrior recruited: {warrior.Name} ({warrior.Role})");
        return true;
    }

    /// <summary>Remove a dead or deserted warrior from the roster.</summary>
    public void DismissWarrior(string warriorId)
    {
        _warriors.Remove(warriorId);
    }

    // ─── Query ────────────────────────────────────────────────────

    public WarriorData? GetWarrior(string id) =>
        _warriors.TryGetValue(id, out var w) ? w : null;

    public IEnumerable<WarriorData> GetLivingWarriors() =>
        _warriors.Values.Where(w => !w.IsDead);

    public IEnumerable<WarriorData> GetWarriorsByRole(WarriorRole role) =>
        _warriors.Values.Where(w => !w.IsDead && w.Role == role);

    public int LivingCount => _warriors.Values.Count(w => !w.IsDead);

    public float AverageMorale =>
        GetLivingWarriors().Any()
            ? (float)GetLivingWarriors().Average(w => w.Morale)
            : 0f;

    // ─── Upgrades ─────────────────────────────────────────────────

    /// <summary>
    /// Spend war supplies to train a warrior, boosting a stat.
    /// Cost: 10 supplies per training session.
    /// </summary>
    public bool TrainWarrior(string warriorId, string stat)
    {
        if (WarSupplies < 10)
        {
            GD.PrintErr("Not enough war supplies for training.");
            return false;
        }

        if (!_warriors.TryGetValue(warriorId, out var warrior) || warrior.IsDead)
            return false;

        WarSupplies -= 10;

        switch (stat.ToLower())
        {
            case "strength":
                warrior.Strength = Math.Min(100, warrior.Strength + 8);
                break;
            case "endurance":
                warrior.Endurance = Math.Min(100, warrior.Endurance + 8);
                break;
            case "morale":
                warrior.Morale = Math.Min(100, warrior.Morale + 15);
                break;
            default:
                GD.PrintErr($"Unknown training stat: {stat}");
                WarSupplies += 10; // Refund.
                return false;
        }

        GD.Print($"Trained {warrior.Name}: {stat} improved.");
        return true;
    }

    // ─── Rest & Morale ────────────────────────────────────────────

    /// <summary>
    /// Rest the warband between raids. Restores morale based on renown.
    /// </summary>
    public void RestWarband()
    {
        int moraleGain = 10 + Renown / 10;
        foreach (var w in GetLivingWarriors())
            w.Morale = Math.Min(100, w.Morale + moraleGain);

        float avg = AverageMorale;
        EmitSignal(SignalName.WarbandMoraleChanged, avg);
        GD.Print($"Warband rested. Average morale: {avg:F0}");
    }

    /// <summary>Apply renown gain from a successful raid.</summary>
    public void GainRenown(int amount)
    {
        Renown += amount;
        // Every 50 renown increases max warband size.
        MaxWarbandSize = 8 + Renown / 50;
        GD.Print($"Renown gained: +{amount} (Total: {Renown}, Max warband: {MaxWarbandSize})");
    }

    // ─── Raid Preparation ─────────────────────────────────────────

    /// <summary>Start a raid with the current warband.</summary>
    public RaidController PrepareRaid(int defenderStrength, float fortLevel)
    {
        var raid = new RaidController();
        AddChild(raid);
        raid.BeginRaid(GetLivingWarriors(), defenderStrength, fortLevel);
        return raid;
    }

    // ─── Serialization ────────────────────────────────────────────

    public Dictionary<string, object> Serialize()
    {
        var warriorStates = new Dictionary<string, Dictionary<string, object>>();
        foreach (var (id, w) in _warriors)
        {
            warriorStates[id] = new Dictionary<string, object>
            {
                ["name"] = w.Name,
                ["role"] = (int)w.Role,
                ["str"] = w.Strength,
                ["end"] = w.Endurance,
                ["morale"] = w.Morale,
                ["dead"] = w.IsDead,
                ["raids"] = w.RaidsSurvived
            };
        }

        return new Dictionary<string, object>
        {
            ["warriors"] = warriorStates,
            ["renown"] = Renown,
            ["supplies"] = WarSupplies,
            ["maxSize"] = MaxWarbandSize
        };
    }
}
