using Godot;

namespace BloodInk.Stealth;

/// <summary>
/// Player visibility states — determines how detectable the player is.
/// </summary>
public enum VisibilityLevel
{
    /// <summary>Fully hidden — in shadow zone or behind full cover.</summary>
    Hidden,

    /// <summary>Partially visible — crouching, dim light, partial cover.</summary>
    Low,

    /// <summary>Normal visibility — standing in the open.</summary>
    Normal,

    /// <summary>Highly visible — running, attacking, in bright light.</summary>
    Exposed
}

/// <summary>
/// Types of noise the player can generate.
/// </summary>
public enum NoiseType
{
    /// <summary>Silent — crouching, standing still.</summary>
    Silent,

    /// <summary>Quiet — slow walking, careful movement.</summary>
    Footstep,

    /// <summary>Moderate — normal movement, opening doors.</summary>
    Movement,

    /// <summary>Loud — running, combat, breaking objects.</summary>
    Loud,

    /// <summary>Very loud — explosions, alarms.</summary>
    Alarm
}

/// <summary>
/// Enemy awareness levels toward the player.
/// </summary>
public enum AwarenessLevel
{
    /// <summary>Unaware — no knowledge of player.</summary>
    Unaware,

    /// <summary>Suspicious — heard something, will investigate.</summary>
    Suspicious,

    /// <summary>Alerted — spotted the player briefly or found evidence.</summary>
    Alerted,

    /// <summary>Engaged — actively pursuing/attacking.</summary>
    Engaged,

    /// <summary>Searching — lost sight but actively looking.</summary>
    Searching
}
