using Godot;
using System.Collections.Generic;

namespace BloodInk.Progression;

/// <summary>
/// Tracks player moral choices throughout the game.
/// These choices affect endings, NPC reactions, and NG+ variations.
/// </summary>
public partial class PlayerChoices : Node
{
    [Signal] public delegate void ChoiceMadeEventHandler(string choiceId, int optionIndex);
    [Signal] public delegate void MoralityShiftedEventHandler(int mercy, int cruelty);

    /// <summary>All choices made: choiceId → selected option index.</summary>
    private readonly Dictionary<string, int> _choices = new();

    /// <summary>Mercy score — accumulated from sparing, helping, showing restraint.</summary>
    public int Mercy { get; private set; } = 0;

    /// <summary>Cruelty score — accumulated from killing unnecessarily, torture, betrayal.</summary>
    public int Cruelty { get; private set; } = 0;

    /// <summary>Total number of optional (non-mandatory) kills.</summary>
    public int OptionalKills { get; private set; } = 0;

    /// <summary>Total number of targets spared when given the option.</summary>
    public int TargetsSpared { get; private set; } = 0;

    /// <summary>Whether the player chose to learn the truth about the Edict.</summary>
    public bool KnowsEdictTruth { get; set; } = false;

    /// <summary>Whether the player sided with Old Thresh's rebellion.</summary>
    public bool SidedWithThresh { get; set; } = false;

    /// <summary>Whether Senna survived the story.</summary>
    public bool SennaSurvived { get; set; } = true;

    /// <summary>Whether the player broke the Edict in the final act.</summary>
    public bool EdictBroken { get; set; } = false;

    // ─── Choice Recording ─────────────────────────────────────────

    /// <summary>Record a narrative choice.</summary>
    public void MakeChoice(string choiceId, int optionIndex)
    {
        _choices[choiceId] = optionIndex;
        EmitSignal(SignalName.ChoiceMade, choiceId, optionIndex);
    }

    /// <summary>Check what option was chosen for a given choice point.</summary>
    public int GetChoice(string choiceId) =>
        _choices.TryGetValue(choiceId, out var opt) ? opt : -1;

    /// <summary>Check if a choice has been made.</summary>
    public bool HasMadeChoice(string choiceId) => _choices.ContainsKey(choiceId);

    // ─── Morality Tracking ────────────────────────────────────────

    public void AddMercy(int amount)
    {
        Mercy += amount;
        EmitSignal(SignalName.MoralityShifted, Mercy, Cruelty);
    }

    public void AddCruelty(int amount)
    {
        Cruelty += amount;
        EmitSignal(SignalName.MoralityShifted, Mercy, Cruelty);
    }

    public void RecordOptionalKill()
    {
        OptionalKills++;
        AddCruelty(3);
    }

    public void RecordTargetSpared()
    {
        TargetsSpared++;
        AddMercy(5);
    }

    /// <summary>
    /// Determine ending alignment based on accumulated choices.
    /// </summary>
    public EndingAlignment GetEndingAlignment()
    {
        int balance = Mercy - Cruelty;

        if (EdictBroken && balance > 10)
            return EndingAlignment.Liberation;
        else if (!EdictBroken && Cruelty > Mercy)
            return EndingAlignment.DarkEdictbearer;
        else
            return EndingAlignment.BitterFreedom;
    }

    // ─── Serialization ────────────────────────────────────────────

    public Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            ["choices"] = new Dictionary<string, int>(_choices),
            ["mercy"] = Mercy,
            ["cruelty"] = Cruelty,
            ["optKills"] = OptionalKills,
            ["spared"] = TargetsSpared,
            ["edictTruth"] = KnowsEdictTruth,
            ["thresh"] = SidedWithThresh,
            ["senna"] = SennaSurvived,
            ["edictBroken"] = EdictBroken
        };
    }

    public void Deserialize(Dictionary<string, object> data)
    {
        _choices.Clear();

        if (data.TryGetValue("mercy", out var m) && m is int mi) Mercy = mi;
        if (data.TryGetValue("cruelty", out var c) && c is int ci) Cruelty = ci;
        if (data.TryGetValue("optKills", out var ok) && ok is int oki) OptionalKills = oki;
        if (data.TryGetValue("spared", out var sp) && sp is int spi) TargetsSpared = spi;
        if (data.TryGetValue("edictTruth", out var et) && et is bool etb) KnowsEdictTruth = etb;
        if (data.TryGetValue("thresh", out var th) && th is bool thb) SidedWithThresh = thb;
        if (data.TryGetValue("senna", out var sn) && sn is bool snb) SennaSurvived = snb;
        if (data.TryGetValue("edictBroken", out var eb) && eb is bool ebb) EdictBroken = ebb;

        if (data.TryGetValue("choices", out var ch) && ch is Dictionary<string, object> chd)
            foreach (var (k, v) in chd) { if (v is int vi) _choices[k] = vi; }
    }
}

/// <summary>
/// The three possible ending alignments.
/// </summary>
public enum EndingAlignment
{
    /// <summary>Edict broken, mercy > cruelty — the orcs are truly free.</summary>
    Liberation,

    /// <summary>Edict intact, cruelty > mercy — Vetch becomes what he hunted.</summary>
    DarkEdictbearer,

    /// <summary>Mixed outcome — freedom with scars.</summary>
    BitterFreedom
}
