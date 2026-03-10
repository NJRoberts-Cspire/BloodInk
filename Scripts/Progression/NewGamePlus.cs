using Godot;
using System.Collections.Generic;

namespace BloodInk.Progression;

/// <summary>
/// Manages the Second Mark — New Game+ system.
/// On NG+, the world changes based on previous playthrough choices:
/// - Tattoo temperament carries over at 50%
/// - Edictbearer ghosts appear (killed targets haunt new positions)
/// - New dialogue acknowledges previous actions
/// - Difficulty modifiers from ink temperament
/// - Lorne's base tremor increases
/// </summary>
public partial class NewGamePlus : Node
{
    /// <summary>How many NG+ cycles have been completed.</summary>
    public int CycleCount { get; private set; } = 0;

    /// <summary>Whether this is currently a NG+ run.</summary>
    public bool IsNewGamePlus => CycleCount > 0;

    /// <summary>Previous run's ending alignment.</summary>
    public EndingAlignment PreviousEnding { get; private set; } = EndingAlignment.BitterFreedom;

    /// <summary>Previous run's mercy score.</summary>
    public int PreviousMercy { get; private set; } = 0;

    /// <summary>Previous run's cruelty score.</summary>
    public int PreviousCruelty { get; private set; } = 0;

    /// <summary>Edictbearers killed in previous run (IDs) — they haunt as ghosts.</summary>
    private readonly HashSet<string> _previousKills = new();

    /// <summary>Tattoo temperament scores from previous run (carried at 50%).</summary>
    private readonly Dictionary<Ink.InkTemperament, float> _carryTemperament = new();

    /// <summary>Narrative flags carried forward from previous run.</summary>
    private readonly HashSet<string> _carryFlags = new();

    // ─── NG+ Setup ────────────────────────────────────────────────

    /// <summary>
    /// Called at the end of a completed run to snapshot data for NG+.
    /// </summary>
    public void SnapshotForNewGamePlus(
        PlayerChoices choices,
        Ink.TattooSystem tattoos,
        List<string> killedEdictbearerIds)
    {
        CycleCount++;
        PreviousEnding = choices.GetEndingAlignment();
        PreviousMercy = choices.Mercy;
        PreviousCruelty = choices.Cruelty;

        _previousKills.Clear();
        foreach (var id in killedEdictbearerIds)
            _previousKills.Add(id);

        // Snapshot temperament at 50%.
        _carryTemperament.Clear();
        var previousScores = new Dictionary<Ink.InkTemperament, int>();
        foreach (Ink.InkTemperament t in System.Enum.GetValues<Ink.InkTemperament>())
        {
            _carryTemperament[t] = tattoos.GetTemperamentScore(t) * 0.5f;
            previousScores[t] = tattoos.GetTemperamentScore(t);
        }

        // Carry select narrative flags.
        _carryFlags.Clear();
        if (choices.KnowsEdictTruth) _carryFlags.Add("knows_edict_truth");
        if (choices.SidedWithThresh) _carryFlags.Add("sided_with_thresh");
        if (!choices.SennaSurvived) _carryFlags.Add("senna_died");
        if (choices.EdictBroken) _carryFlags.Add("edict_broken");

        GD.Print($"NG+ Snapshot — Cycle {CycleCount}, Ending: {PreviousEnding}");
        GD.Print($"  Mercy: {PreviousMercy}, Cruelty: {PreviousCruelty}");
        GD.Print($"  Edictbearers slain: {_previousKills.Count}");
    }

    /// <summary>
    /// Apply NG+ modifications to a new game's systems.
    /// Call this during new game initialization after systems are created.
    /// </summary>
    public void ApplyToNewGame(
        Ink.TattooSystem tattoos,
        PlayerChoices choices,
        Campaigns.Lorne.TremorSystem? tremor)
    {
        if (!IsNewGamePlus) return;

        // Import temperament at 50%.
        var intScores = new Dictionary<Ink.InkTemperament, int>();
        foreach (var (t, v) in _carryTemperament)
            intScores[t] = (int)v;
        tattoos.ImportTemperamentsForNewGamePlus(intScores, 1.0f);
        GD.Print("NG+: Temperament imported at 50% carryover.");

        // Carry narrative flags.
        foreach (var flag in _carryFlags)
        {
            switch (flag)
            {
                case "knows_edict_truth": choices.KnowsEdictTruth = true; break;
                case "senna_died": choices.SennaSurvived = false; break;
                case "sided_with_thresh": choices.SidedWithThresh = true; break;
                case "edict_broken": choices.EdictBroken = true; break;
            }
        }

        // Lorne's tremor worsens each cycle.
        tremor?.IncreaseBaseTremor(5f * CycleCount);

        // Difficulty scaling.
        float difficultyMult = GetDifficultyMultiplier();
        GD.Print($"NG+: Difficulty multiplier {difficultyMult:F2}x (Cycle {CycleCount})");
    }

    // ─── Ghost System ─────────────────────────────────────────────

    /// <summary>
    /// Check if an Edictbearer was killed in a previous run.
    /// Used to spawn ghost encounters in NG+.
    /// </summary>
    public bool IsEdictbearerGhost(string targetId) =>
        IsNewGamePlus && _previousKills.Contains(targetId);

    /// <summary>Get all ghost Edictbearer IDs.</summary>
    public IEnumerable<string> GetGhostIds() => _previousKills;

    // ─── Difficulty ───────────────────────────────────────────────

    /// <summary>
    /// NG+ difficulty multiplier. Each cycle adds 15% enemy health/damage.
    /// </summary>
    public float GetDifficultyMultiplier() =>
        1.0f + CycleCount * 0.15f;

    /// <summary>
    /// Extra alert heat per action in NG+. Kingdoms remember.
    /// </summary>
    public float GetExtraAlertHeat() =>
        CycleCount * 3f;

    // ─── NG+ Dialogue Checks ─────────────────────────────────────

    public bool HasCarryFlag(string flag) => _carryFlags.Contains(flag);

    /// <summary>
    /// Get a dialogue modifier string based on previous run.
    /// NPCs react differently if they "remember" the previous cycle.
    /// </summary>
    public string GetDialogueModifier()
    {
        if (!IsNewGamePlus) return "first_run";

        return PreviousEnding switch
        {
            EndingAlignment.Liberation => "prev_liberation",
            EndingAlignment.DarkEdictbearer => "prev_dark",
            EndingAlignment.BitterFreedom => "prev_bitter",
            _ => "first_run"
        };
    }

    // ─── Serialization ────────────────────────────────────────────

    public Dictionary<string, object> Serialize()
    {
        var tempDict = new Dictionary<string, float>();
        foreach (var (t, v) in _carryTemperament)
            tempDict[t.ToString()] = v;

        return new Dictionary<string, object>
        {
            ["cycle"] = CycleCount,
            ["prevEnding"] = (int)PreviousEnding,
            ["prevMercy"] = PreviousMercy,
            ["prevCruelty"] = PreviousCruelty,
            ["kills"] = new List<string>(_previousKills),
            ["temperament"] = tempDict,
            ["flags"] = new List<string>(_carryFlags)
        };
    }

    public void Deserialize(Dictionary<string, object> data)
    {
        _previousKills.Clear();
        _carryFlags.Clear();
        _carryTemperament.Clear();

        if (data.TryGetValue("cycle", out var c) && c is int ci) CycleCount = ci;
        if (data.TryGetValue("prevEnding", out var pe) && pe is int pei) PreviousEnding = (EndingAlignment)pei;
        if (data.TryGetValue("prevMercy", out var pm) && pm is int pmi) PreviousMercy = pmi;
        if (data.TryGetValue("prevCruelty", out var pc) && pc is int pci) PreviousCruelty = pci;

        if (data.TryGetValue("kills", out var k) && k is List<object> ks)
            foreach (var id in ks) { if (id is string s) _previousKills.Add(s); }
        if (data.TryGetValue("flags", out var f) && f is List<object> fs)
            foreach (var fl in fs) { if (fl is string s) _carryFlags.Add(s); }
        if (data.TryGetValue("temperament", out var t) && t is Dictionary<string, object> td)
            foreach (var (key, val) in td)
                if (System.Enum.TryParse<Ink.InkTemperament>(key, out var temp))
                    _carryTemperament[temp] = val switch { int i => i, float fv => fv, double d => (float)d, _ => 0f };
    }
}
