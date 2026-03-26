namespace BloodInk.Dialogue;

/// <summary>
/// Centralised constants for all dialogue flag names used across the codebase.
/// Always reference flags through this class — never use raw string literals.
/// </summary>
public static class DialogueFlags
{
    // ─── NG+ carry flags ──────────────────────────────────────────────────────
    /// <summary>Player learned the true purpose of the Edict in a previous run.</summary>
    public const string KnowsEdictTruth  = "knows_edict_truth";

    /// <summary>Player sided with Old Thresh's rebellion in a previous run.</summary>
    public const string SidedWithThresh  = "sided_with_thresh";

    /// <summary>Senna did not survive the previous run.</summary>
    public const string SennaDied        = "senna_died";

    /// <summary>Player broke the Edict in the final act of the previous run.</summary>
    public const string EdictBroken      = "edict_broken";

    // ─── NG+ dialogue modifiers ──────────────────────────────────────────────
    /// <summary>Dialogue modifier prefix: first-time playthrough.</summary>
    public const string DialogueFirstRun      = "first_run";

    /// <summary>Dialogue modifier prefix: previous run ended in Liberation.</summary>
    public const string DialoguePrevLiberation = "prev_liberation";

    /// <summary>Dialogue modifier prefix: previous run ended as Dark Edictbearer.</summary>
    public const string DialoguePrevDark       = "prev_dark";

    /// <summary>Dialogue modifier prefix: previous run ended in Bitter Freedom.</summary>
    public const string DialoguePrevBitter     = "prev_bitter";

    // ─── Demo / first-visit flags ─────────────────────────────────────────────
    /// <summary>Player has seen the opening narration sequence (DemoIntro).</summary>
    public const string SawIntro         = "saw_intro";

    // ─── Moral / binary story flags ───────────────────────────────────────────
    /// <summary>Player was aware of the Edict's true nature during the current run.</summary>
    public const string EdictTruthKnown  = "edict_truth_known";

    /// <summary>Senna has been confirmed alive at the current point in the run.</summary>
    public const string SennaAlive       = "senna_alive";
}
