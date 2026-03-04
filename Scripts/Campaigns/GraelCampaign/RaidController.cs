using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloodInk.Campaigns.Grael;

/// <summary>
/// Outcome of a raid phase.
/// </summary>
public enum RaidOutcome
{
    Victory,
    Pyrrhic,    // Won but heavy losses
    Stalemate,
    Retreat,
    Rout        // Total defeat
}

/// <summary>
/// Controls Grael's raid encounters — the warband assaults a fortified position.
/// The player issues orders and the system resolves combat in phases.
/// </summary>
public partial class RaidController : Node
{
    [Signal] public delegate void RaidPhaseResolvedEventHandler(int phase, int outcome);
    [Signal] public delegate void WarriorFallenEventHandler(string warriorId);
    [Signal] public delegate void RaidCompleteEventHandler(int outcome);

    /// <summary>Warband warriors participating in this raid.</summary>
    private readonly List<WarriorData> _attackers = new();

    /// <summary>Defending strength (abstract number representing garrison).</summary>
    private int _defenderStrength = 50;

    /// <summary>Defensive fortification level. Reduces attacker effectiveness.</summary>
    private float _fortificationLevel = 1.0f;

    /// <summary>Current raid phase (0-based).</summary>
    public int CurrentPhase { get; private set; } = 0;

    /// <summary>Maximum phases before the raid auto-resolves.</summary>
    public int MaxPhases { get; set; } = 5;

    /// <summary>Whether the raid is still in progress.</summary>
    public bool IsActive { get; private set; } = false;

    private readonly Random _rng = new();

    // ─── Setup ────────────────────────────────────────────────────

    /// <summary>
    /// Initialize a raid with the given warband against a target.
    /// </summary>
    public void BeginRaid(IEnumerable<WarriorData> warband, int defenderStr, float fortLevel)
    {
        _attackers.Clear();
        _attackers.AddRange(warband.Where(w => !w.IsDead));
        _defenderStrength = defenderStr;
        _fortificationLevel = Math.Max(0.1f, fortLevel);
        CurrentPhase = 0;
        IsActive = true;

        GD.Print($"RAID BEGINS — {_attackers.Count} warriors vs garrison (Str:{_defenderStrength}, Fort:{_fortificationLevel:F1})");
    }

    // ─── Phase Resolution ─────────────────────────────────────────

    /// <summary>
    /// Resolve one phase of the raid. Call this each "turn".
    /// Returns the outcome of this phase.
    /// </summary>
    public RaidOutcome ResolvePhase()
    {
        if (!IsActive) return RaidOutcome.Stalemate;

        CurrentPhase++;

        // Calculate attacker power.
        float attackPower = 0f;
        foreach (var w in _attackers.Where(w => !w.IsDead))
        {
            float rolePower = w.Role switch
            {
                WarriorRole.Brawler => w.Strength * 1.0f,
                WarriorRole.ShieldBearer => w.Strength * 0.6f + w.Endurance * 0.4f,
                WarriorRole.Flanker => w.Strength * 1.2f,
                WarriorRole.Skirmisher => w.Strength * 0.8f,
                WarriorRole.WarChanter => w.Strength * 0.3f,
                WarriorRole.Breaker => w.Strength * 0.5f,
                _ => w.Strength
            };

            // Morale affects effectiveness.
            float moraleModifier = w.Morale / 100f;
            attackPower += rolePower * moraleModifier;
        }

        // Breakers reduce fortification.
        int breakerCount = _attackers.Count(w => !w.IsDead && w.Role == WarriorRole.Breaker);
        float effectiveFort = Math.Max(0.1f, _fortificationLevel - breakerCount * 0.15f);

        // War chanters boost morale.
        int chanterCount = _attackers.Count(w => !w.IsDead && w.Role == WarriorRole.WarChanter);

        // Compare forces.
        float effectiveDefence = _defenderStrength * effectiveFort;
        float ratio = attackPower / Math.Max(1f, effectiveDefence);

        // Casualties — both sides take losses.
        float casualtyChance = Math.Clamp(1f - ratio, 0.05f, 0.6f);
        foreach (var w in _attackers.Where(w => !w.IsDead).ToList())
        {
            // Shield bearers reduce casualty chance.
            float shieldReduction = _attackers.Any(s => !s.IsDead && s.Role == WarriorRole.ShieldBearer) ? 0.1f : 0f;
            float finalChance = Math.Max(0.02f, casualtyChance - shieldReduction);

            if (_rng.NextDouble() < finalChance)
            {
                // Check endurance for survival.
                if (_rng.Next(100) > w.Endurance)
                {
                    w.IsDead = true;
                    EmitSignal(SignalName.WarriorFallen, w.Id);
                    GD.Print($"  Warrior fallen: {w.Name} ({w.Role})");
                }
            }

            // Morale shifts.
            w.Morale = Math.Clamp(w.Morale + (ratio > 1f ? 5 : -10) + chanterCount * 3, 0, 100);
        }

        // Defender losses.
        float defenderCasualty = ratio * 0.2f;
        _defenderStrength = Math.Max(0, (int)(_defenderStrength * (1f - defenderCasualty)));
        _fortificationLevel = Math.Max(0f, _fortificationLevel - breakerCount * 0.05f);

        // Determine phase outcome.
        RaidOutcome outcome;
        int livingAttackers = _attackers.Count(w => !w.IsDead);
        int routedCount = _attackers.Count(w => !w.IsDead && w.Morale <= 10);

        if (_defenderStrength <= 0)
        {
            outcome = livingAttackers <= _attackers.Count / 3 ? RaidOutcome.Pyrrhic : RaidOutcome.Victory;
            IsActive = false;
        }
        else if (livingAttackers == 0 || routedCount >= livingAttackers)
        {
            outcome = RaidOutcome.Rout;
            IsActive = false;
        }
        else if (CurrentPhase >= MaxPhases)
        {
            outcome = ratio > 0.8f ? RaidOutcome.Stalemate : RaidOutcome.Retreat;
            IsActive = false;
        }
        else
        {
            outcome = ratio > 1f ? RaidOutcome.Victory : RaidOutcome.Stalemate;
            // Raid continues — only terminal outcomes end the raid mid-loop.
            if (outcome == RaidOutcome.Victory)
            {
                // Partial victory this phase but defenders still standing.
                outcome = RaidOutcome.Stalemate;
            }
        }

        EmitSignal(SignalName.RaidPhaseResolved, CurrentPhase, (int)outcome);
        GD.Print($"Phase {CurrentPhase}: Attack={attackPower:F0} vs Defence={effectiveDefence:F0} → {outcome} (Defenders remaining: {_defenderStrength})");

        if (!IsActive)
        {
            foreach (var w in _attackers.Where(w => !w.IsDead))
                w.RaidsSurvived++;
            EmitSignal(SignalName.RaidComplete, (int)outcome);
            GD.Print($"RAID ENDED: {outcome} — {livingAttackers}/{_attackers.Count} warriors survived");
        }

        return outcome;
    }

    /// <summary>Order a retreat, ending the raid.</summary>
    public void OrderRetreat()
    {
        if (!IsActive) return;
        IsActive = false;

        // Morale penalty for retreating.
        foreach (var w in _attackers.Where(w => !w.IsDead))
            w.Morale = Math.Max(0, w.Morale - 15);

        EmitSignal(SignalName.RaidComplete, (int)RaidOutcome.Retreat);
        GD.Print("RAID RETREAT ordered by commander.");
    }
}
