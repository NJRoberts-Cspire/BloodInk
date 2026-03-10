using Godot;
using System;

namespace BloodInk.Campaigns.Lorne;

/// <summary>
/// Simulates Lorne's hand tremor — her hands shake from years of delicate tattoo work
/// and trauma. The tremor affects crafting quality and is a core mechanical tension
/// in her campaign. Players must manage tremor through rest, herbs, and pacing.
/// </summary>
public partial class TremorSystem : Node
{
    [Signal] public delegate void TremorChangedEventHandler(float currentTremor);
    [Signal] public delegate void TremorFlareEventHandler();
    [Signal] public delegate void HandSteadiedEventHandler();

    /// <summary>Current tremor intensity 0-100. Higher = shakier hands.</summary>
    public float TremorLevel { get; private set; } = 20f;

    /// <summary>Base tremor that the level can't drop below (permanent damage).</summary>
    public float BaseTremor { get; private set; } = 10f;

    /// <summary>Steadiness score derived from tremor. 100 - TremorLevel.</summary>
    public float Steadiness => 100f - TremorLevel;

    /// <summary>Whether Lorne is currently in a tremor flare (temporary spike).</summary>
    public bool IsFlaring { get; private set; } = false;

    /// <summary>Turns remaining on current flare.</summary>
    private int _flareTurnsRemaining = 0;

    /// <summary>Herb doses used (diminishing returns).</summary>
    private int _herbDosesUsed = 0;

    private readonly Random _rng = new();

    // ─── Tremor Events ────────────────────────────────────────────

    /// <summary>
    /// Called when Lorne performs delicate work (crafting, tattooing).
    /// Difficulty 1-10 affects how much tremor increases.
    /// </summary>
    public void OnDelicateWork(int difficulty)
    {
        float increase = difficulty * 2f + _rng.Next(0, difficulty);
        TremorLevel = Math.Min(100f, TremorLevel + increase);

        // Random chance of flare during difficult work.
        if (difficulty >= 6 && _rng.NextDouble() < 0.15f + (TremorLevel / 500f))
            TriggerFlare();

        EmitSignal(SignalName.TremorChanged, TremorLevel);
    }

    /// <summary>
    /// Trigger a tremor flare — sudden spike in shakiness.
    /// Lasts 2-3 turns. Makes crafting nearly impossible.
    /// </summary>
    public void TriggerFlare()
    {
        if (IsFlaring) return;

        IsFlaring = true;
        _flareTurnsRemaining = 2 + (_rng.NextDouble() < 0.4 ? 1 : 0);
        TremorLevel = Math.Min(100f, TremorLevel + 25f);

        EmitSignal(SignalName.TremorFlare);
        EmitSignal(SignalName.TremorChanged, TremorLevel);
        GD.Print($"TREMOR FLARE! Lorne's hands are shaking violently. ({_flareTurnsRemaining} turns)");
    }

    // ─── Recovery ─────────────────────────────────────────────────

    /// <summary>
    /// Rest to reduce tremor. Each rest reduces tremor but can't go below base.
    /// </summary>
    public void Rest()
    {
        float reduction = 15f;
        TremorLevel = Math.Max(BaseTremor, TremorLevel - reduction);

        if (IsFlaring)
        {
            _flareTurnsRemaining--;
            if (_flareTurnsRemaining <= 0)
            {
                IsFlaring = false;
                EmitSignal(SignalName.HandSteadied);
                GD.Print("Tremor flare subsided.");
            }
        }

        EmitSignal(SignalName.TremorChanged, TremorLevel);
        GD.Print($"Lorne rests. Tremor: {TremorLevel:F0} (Steadiness: {Steadiness:F0})");
    }

    /// <summary>
    /// Use calming herbs to reduce tremor. Diminishing returns per use.
    /// </summary>
    public void UseHerbs()
    {
        _herbDosesUsed++;
        // Diminishing returns: 20 → 15 → 10 → 7 → 5...
        float reduction = Math.Max(5f, 20f / (1f + _herbDosesUsed * 0.3f));
        TremorLevel = Math.Max(BaseTremor, TremorLevel - reduction);

        EmitSignal(SignalName.TremorChanged, TremorLevel);
        GD.Print($"Herbs used (dose #{_herbDosesUsed}). Tremor: {TremorLevel:F0} (reduced by {reduction:F0})");
    }

    /// <summary>Reset herb tolerance (e.g. between kingdoms or on long rest).</summary>
    public void ResetHerbTolerance()
    {
        _herbDosesUsed = 0;
        GD.Print("Herb tolerance reset.");
    }

    // ─── Advance Turn ─────────────────────────────────────────────

    /// <summary>Advance one turn for the tremor system.</summary>
    public void AdvanceTurn()
    {
        // Natural slight recovery each turn.
        TremorLevel = Math.Max(BaseTremor, TremorLevel - 3f);

        if (IsFlaring)
        {
            _flareTurnsRemaining--;
            if (_flareTurnsRemaining <= 0)
            {
                IsFlaring = false;
                EmitSignal(SignalName.HandSteadied);
                GD.Print("Tremor flare subsided naturally.");
            }
        }

        EmitSignal(SignalName.TremorChanged, TremorLevel);
    }

    // ─── NG+ / Progression ───────────────────────────────────────

    /// <summary>
    /// In NG+, Lorne's base tremor increases — she's getting worse.
    /// </summary>
    public void IncreaseBaseTremor(float amount)
    {
        BaseTremor = Math.Min(50f, BaseTremor + amount);
        TremorLevel = Math.Max(TremorLevel, BaseTremor);
        GD.Print($"Base tremor increased to {BaseTremor:F0}. The shaking never stops.");
    }

    // ─── Serialization ────────────────────────────────────────────

    public System.Collections.Generic.Dictionary<string, object> Serialize()
    {
        return new System.Collections.Generic.Dictionary<string, object>
        {
            ["tremor"] = TremorLevel,
            ["base"] = BaseTremor,
            ["flaring"] = IsFlaring,
            ["flareTurns"] = _flareTurnsRemaining,
            ["herbDoses"] = _herbDosesUsed
        };
    }

    public void Deserialize(System.Collections.Generic.Dictionary<string, object> data)
    {
        if (data.TryGetValue("tremor", out var t))
            TremorLevel = t switch { int i => i, float fv => fv, double d => (float)d, _ => TremorLevel };
        if (data.TryGetValue("base", out var b))
            BaseTremor = b switch { int i => i, float fv => fv, double d => (float)d, _ => BaseTremor };
        if (data.TryGetValue("flaring", out var fl) && fl is bool fb) IsFlaring = fb;
        if (data.TryGetValue("flareTurns", out var ft) && ft is int fti) _flareTurnsRemaining = fti;
        if (data.TryGetValue("herbDoses", out var hd) && hd is int hdi) _herbDosesUsed = hdi;
    }
}
