using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloodInk.Ink;

/// <summary>
/// Core tattoo management system. Tracks applied tattoos, temperament scores,
/// evolution, ink conflicts, and stat modifiers.
/// Attach as a child of the player or as an autoload.
/// </summary>
public partial class TattooSystem : Node
{
    [Signal] public delegate void TattooAppliedEventHandler(string tattooId, int slot);
    [Signal] public delegate void TemperamentChangedEventHandler(int temperament, int score);
    [Signal] public delegate void TattooEvolvedEventHandler(string oldId, string newId);
    [Signal] public delegate void InkConflictTriggeredEventHandler(string conflictType);

    /// <summary>Temperament scores (0–100 each). Driven by player actions.</summary>
    private readonly Dictionary<InkTemperament, int> _temperamentScores = new()
    {
        { InkTemperament.Shadow, 0 },
        { InkTemperament.Fang, 0 },
        { InkTemperament.Root, 0 },
        { InkTemperament.Bone, 0 }
    };

    /// <summary>All tattoos currently applied, keyed by slot.</summary>
    private readonly Dictionary<TattooSlot, List<TattooData>> _appliedTattoos = new();

    /// <summary>Total number of tattoos applied (for side-effect scaling).</summary>
    public int TotalTattoosApplied => _appliedTattoos.Values.Sum(list => list.Count);

    // --- Aggregated stat bonuses, recalculated on change ---
    public float StealthBonus { get; private set; }
    public float DamageBonus { get; private set; }
    public float SpeedBonus { get; private set; }
    public float HealthBonus { get; private set; }
    public float DetectionRadiusModifier { get; private set; }
    public float TrapEffectivenessBonus { get; private set; }
    public float HealingBonus { get; private set; }
    public float ResistanceBonus { get; private set; }

    public override void _Ready()
    {
        // Initialize empty slot lists.
        foreach (TattooSlot slot in Enum.GetValues<TattooSlot>())
        {
            _appliedTattoos[slot] = new List<TattooData>();
        }
    }

    // ─── Temperament Tracking ──────────────────────────────────────

    /// <summary>
    /// Called by game systems whenever the player performs a categorized action.
    /// E.g., stealth kill → RecordAction(Shadow, 5)
    /// </summary>
    public void RecordAction(InkTemperament temperament, int weight = 1)
    {
        _temperamentScores[temperament] = Math.Min(100, _temperamentScores[temperament] + weight);
        EmitSignal(SignalName.TemperamentChanged, (int)temperament, _temperamentScores[temperament]);
        CheckForEvolutions();
        CheckForConflicts();
    }

    public int GetTemperamentScore(InkTemperament temperament) => _temperamentScores[temperament];

    /// <summary>Returns the dominant temperament (highest score).</summary>
    public InkTemperament GetDominantTemperament()
    {
        return _temperamentScores.OrderByDescending(kv => kv.Value).First().Key;
    }

    /// <summary>Returns the temperament balance as a normalized dictionary (0.0–1.0).</summary>
    public Dictionary<InkTemperament, float> GetTemperamentBalance()
    {
        var total = Math.Max(1, _temperamentScores.Values.Sum());
        return _temperamentScores.ToDictionary(kv => kv.Key, kv => (float)kv.Value / total);
    }

    // ─── Tattoo Application ───────────────────────────────────────

    /// <summary>Apply a tattoo. Requires sufficient ink in the inventory.</summary>
    public bool ApplyTattoo(TattooData tattoo, InkInventory inventory)
    {
        if (!inventory.CanAfford(tattoo.RequiredGrade, tattoo.InkCost))
        {
            GD.Print($"Cannot afford tattoo '{tattoo.DisplayName}': need {tattoo.InkCost} {tattoo.RequiredGrade} ink.");
            return false;
        }

        inventory.SpendInk(tattoo.RequiredGrade, tattoo.InkCost);
        _appliedTattoos[tattoo.Slot].Add(tattoo);

        // Push temperament from the tattoo itself.
        RecordAction(tattoo.PrimaryTemperament, tattoo.TemperamentWeight);

        RecalculateStats();
        EmitSignal(SignalName.TattooApplied, tattoo.Id, (int)tattoo.Slot);
        GD.Print($"Tattoo applied: {tattoo.DisplayName} ({tattoo.Slot})");
        return true;
    }

    /// <summary>Get all tattoos in a specific slot.</summary>
    public IReadOnlyList<TattooData> GetTattoosInSlot(TattooSlot slot) => _appliedTattoos[slot];

    /// <summary>Get all applied tattoos across all slots.</summary>
    public IEnumerable<TattooData> GetAllTattoos() => _appliedTattoos.Values.SelectMany(t => t);

    /// <summary>Check if a specific tattoo has been applied.</summary>
    public bool HasTattoo(string tattooId) =>
        _appliedTattoos.Values.Any(list => list.Any(t => t.Id == tattooId));

    // ─── Evolution ────────────────────────────────────────────────

    private void CheckForEvolutions()
    {
        foreach (var (slot, tattoos) in _appliedTattoos)
        {
            for (int i = 0; i < tattoos.Count; i++)
            {
                var tattoo = tattoos[i];
                if (tattoo.EvolvedForm != null &&
                    _temperamentScores[tattoo.PrimaryTemperament] >= tattoo.EvolutionThreshold)
                {
                    var oldId = tattoo.Id;
                    tattoos[i] = tattoo.EvolvedForm;
                    RecalculateStats();
                    EmitSignal(SignalName.TattooEvolved, oldId, tattoo.EvolvedForm.Id);
                    GD.Print($"Tattoo evolved: {oldId} → {tattoo.EvolvedForm.Id}");
                }
            }
        }
    }

    // ─── Ink Conflicts ────────────────────────────────────────────

    /// <summary>
    /// Opposing temperament pairs that create conflicts when both are high.
    /// Shadow+Fang → "Ink Bleed" (position revealed randomly)
    /// Root+Bone → "Ink Calm" (reflexes suppressed, perception enhanced)
    /// </summary>
    private void CheckForConflicts()
    {
        const int ConflictThreshold = 40;

        if (_temperamentScores[InkTemperament.Shadow] >= ConflictThreshold &&
            _temperamentScores[InkTemperament.Fang] >= ConflictThreshold)
        {
            EmitSignal(SignalName.InkConflictTriggered, "InkBleed");
        }

        if (_temperamentScores[InkTemperament.Root] >= ConflictThreshold &&
            _temperamentScores[InkTemperament.Bone] >= ConflictThreshold)
        {
            EmitSignal(SignalName.InkConflictTriggered, "InkCalm");
        }
    }

    /// <summary>Probability (0.0–1.0) that an Ink Bleed event fires this frame.</summary>
    public float GetInkBleedChance()
    {
        var shadow = _temperamentScores[InkTemperament.Shadow];
        var fang = _temperamentScores[InkTemperament.Fang];
        var overlap = Math.Min(shadow, fang);
        return overlap > 40 ? (overlap - 40) / 200f : 0f; // Max ~30% at full overlap.
    }

    /// <summary>Reflex penalty (0.0–1.0) from Ink Calm. Higher = slower reaction prompts.</summary>
    public float GetInkCalmPenalty()
    {
        var root = _temperamentScores[InkTemperament.Root];
        var bone = _temperamentScores[InkTemperament.Bone];
        var overlap = Math.Min(root, bone);
        return overlap > 40 ? (overlap - 40) / 150f : 0f; // Max ~40% at full overlap.
    }

    /// <summary>Perception bonus from Ink Calm. Higher = better detection abilities.</summary>
    public float GetInkCalmPerceptionBonus()
    {
        return GetInkCalmPenalty() * 1.5f; // Tradeoff: slower reflexes but sharper senses.
    }

    // ─── Stat Aggregation ─────────────────────────────────────────

    private void RecalculateStats()
    {
        StealthBonus = 0;
        DamageBonus = 0;
        SpeedBonus = 0;
        HealthBonus = 0;
        DetectionRadiusModifier = 0;
        TrapEffectivenessBonus = 0;
        HealingBonus = 0;
        ResistanceBonus = 0;

        foreach (var tattoo in GetAllTattoos())
        {
            StealthBonus += tattoo.StealthBonus;
            DamageBonus += tattoo.DamageBonus;
            SpeedBonus += tattoo.SpeedBonus;
            HealthBonus += tattoo.HealthBonus;
            DetectionRadiusModifier += tattoo.DetectionRadiusModifier;
            TrapEffectivenessBonus += tattoo.TrapEffectivenessBonus;
            HealingBonus += tattoo.HealingBonus;
            ResistanceBonus += tattoo.ResistanceBonus;
        }

        // Temperament-driven passive bonuses/penalties.
        var dominant = GetDominantTemperament();
        var dominantScore = _temperamentScores[dominant] / 100f;

        switch (dominant)
        {
            case InkTemperament.Shadow:
                StealthBonus += dominantScore * 0.3f;
                DetectionRadiusModifier -= dominantScore * 0.2f; // Harder to detect.
                DamageBonus -= dominantScore * 0.1f; // Combat penalty.
                break;
            case InkTemperament.Fang:
                DamageBonus += dominantScore * 0.3f;
                SpeedBonus += dominantScore * 0.15f;
                DetectionRadiusModifier += dominantScore * 0.25f; // Easier to detect (blood smell).
                break;
            case InkTemperament.Root:
                HealingBonus += dominantScore * 0.3f;
                ResistanceBonus += dominantScore * 0.2f;
                DamageBonus -= dominantScore * 0.15f;
                break;
            case InkTemperament.Bone:
                TrapEffectivenessBonus += dominantScore * 0.35f;
                StealthBonus += dominantScore * 0.1f;
                DamageBonus -= dominantScore * 0.2f; // Direct combat narrows.
                break;
        }
    }

    // ─── Serialization ────────────────────────────────────────────

    public Dictionary<string, object> Serialize()
    {
        var tattooIds = new List<string>();
        foreach (var tattoo in GetAllTattoos())
            tattooIds.Add(tattoo.Id);

        var temps = new Dictionary<string, int>();
        foreach (var kv in _temperamentScores)
            temps[kv.Key.ToString()] = kv.Value;

        return new Dictionary<string, object>
        {
            ["tattoos"] = tattooIds,
            ["temperaments"] = temps,
            ["totalApplied"] = TotalTattoosApplied
        };
    }

    /// <summary>For NG+: carry over temperament scores at reduced intensity.</summary>
    public void ImportTemperamentsForNewGamePlus(Dictionary<InkTemperament, int> previousScores, float carryOverRate = 0.5f)
    {
        foreach (var kv in previousScores)
        {
            _temperamentScores[kv.Key] = (int)(kv.Value * carryOverRate);
        }
        RecalculateStats();
    }
}
