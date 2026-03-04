namespace BloodInk.Ink;

/// <summary>
/// The four ink temperaments that tattoos evolve along based on playstyle.
/// </summary>
public enum InkTemperament
{
    /// <summary>Stealth kills, undetected completions, ghost runs. Flowing smoke-like lines.</summary>
    Shadow,

    /// <summary>Open combat, aggressive play, short alarm responses. Sharp angular tribal marks.</summary>
    Fang,

    /// <summary>Mercy choices, sparing targets, freeing slaves. Organic vine-like patterns.</summary>
    Root,

    /// <summary>Environmental kills, trap usage, creative solutions. Geometric skeletal patterns.</summary>
    Bone
}

/// <summary>
/// Categories of ink harvested from targets.
/// </summary>
public enum InkGrade
{
    /// <summary>From Edictbearers. Unlocks new tattoo categories.</summary>
    Major,

    /// <summary>From secondary targets. Fills out tattoos within categories.</summary>
    Lesser,

    /// <summary>From optional targets. Minor upgrades and cosmetic variations.</summary>
    Trace
}

/// <summary>
/// Tattoo body locations — determines what kind of abilities the slot grants.
/// </summary>
public enum TattooSlot
{
    /// <summary>Arms — stealth abilities (invisibility, silent movement, shadow dash).</summary>
    Arms_Shadow,

    /// <summary>Chest/Torso — combat power (damage, speed, counter-attacks).</summary>
    Chest_Fang,

    /// <summary>Legs — movement (sprint, climb, water breathing).</summary>
    Legs_Vein,

    /// <summary>Face/Head — perception (enemy detection, trap sense, dark vision).</summary>
    Head_Skull,

    /// <summary>Back — resistance (damage reduction, poison immunity, curse resistance).</summary>
    Back_Spine,

    /// <summary>Hands/Fingers — interaction (lockpicking, pickpocket, disguise).</summary>
    Hands_Whisper
}
