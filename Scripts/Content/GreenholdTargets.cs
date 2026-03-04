using Godot;
using BloodInk.Progression;
using BloodInk.Ink;

namespace BloodInk.Content;

/// <summary>
/// All assassination targets for Kingdom 1: The Greenhold.
/// Static factory until .tres resources are hooked up.
/// </summary>
public static class GreenholdTargets
{
    // ═══ PRIMARY TARGET (Edictbearer) ═══════════════════════════

    public static TargetData LordHarlanCowl() => new()
    {
        Id           = "cowl",
        Name         = "Lord Harlan Cowl",
        Title        = "Governor of the Greenhold",
        KingdomIndex = 0,
        IsEdictbearer = true,
        IsMandatory  = true,
        Difficulty   = 8,
        InkDrop      = InkGrade.Major,
        InkAmount    = 1,
        BloodEchoId  = "echo_cowl",
        Weaknesses   = new[] { "wine_cellar", "evening_walk", "west_balcony" },
        MissionScenePath = "res://Scenes/Missions/Greenhold/Goldmanor.tscn",
        IntelBrief   = "Lord Cowl governs from Goldmanor, a sprawling estate at the heart of the Greenhold. "
                     + "Beloved by his people, feared by his servants. Believes orcs are subhuman with "
                     + "the sincerity of a man who has never questioned a single premise of his life. "
                     + "Carries the Edict anchor in his bloodline — not an object, but a birthright.",
        DeathWhisper = "\"I never saw the dark as empty. I thought it was full of things that loved me.\""
    };

    // ═══ SECONDARY TARGETS ══════════════════════════════════════

    public static TargetData ReeveMaren() => new()
    {
        Id           = "maren",
        Name         = "Reeve Maren",
        Title        = "Overseer of the Labor Camps",
        KingdomIndex = 0,
        IsEdictbearer = false,
        IsMandatory  = true,
        Difficulty   = 5,
        InkDrop      = InkGrade.Lesser,
        InkAmount    = 1,
        Weaknesses   = new[] { "nightly_inspection", "drinks_alone", "supply_wagon" },
        MissionScenePath = "res://Scenes/Missions/Greenhold/LaborCamp.tscn",
        IntelBrief   = "Maren runs the orc labor camps with bureaucratic efficiency. Knows the location of "
                     + "every imprisoned orc in the Greenhold. A small, precise man who has never raised "
                     + "his voice or his hand — he just writes the numbers down.",
        DeathWhisper = "\"The dogs didn't bark. That's what I can't understand.\""
    };

    public static TargetData SisterBlessing() => new()
    {
        Id           = "blessing",
        Name         = "Sister Blessing",
        Title        = "Head of the Greenhold Chapel",
        KingdomIndex = 0,
        IsEdictbearer = false,
        IsMandatory  = true,
        Difficulty   = 4,
        InkDrop      = InkGrade.Lesser,
        InkAmount    = 1,
        Weaknesses   = new[] { "morning_prayer", "relic_chamber", "unlocked_vestry" },
        MissionScenePath = "res://Scenes/Missions/Greenhold/Chapel.tscn",
        IntelBrief   = "Preaches that the Edict is divine mercy — 'a gentle pruning of God's garden.' "
                     + "Keeps a relic (an orc thighbone set in gold) that locally strengthens the curse. "
                     + "Destroying the relic weakens the Edict's grip on the region before Cowl's death.",
        DeathWhisper = "\"We prayed for them too. I want you to know that.\""
    };

    public static TargetData CaptainThorne() => new()
    {
        Id           = "thorne",
        Name         = "Captain Thorne",
        Title        = "Commander of the Greenguard",
        KingdomIndex = 0,
        IsEdictbearer = false,
        IsMandatory  = true,
        Difficulty   = 7,
        InkDrop      = InkGrade.Lesser,
        InkAmount    = 1,
        Weaknesses   = new[] { "patrol_route_change", "old_injury_left_knee", "trophy_room" },
        MissionScenePath = "res://Scenes/Missions/Greenhold/Barracks.tscn",
        IntelBrief   = "Veteran orc-hunter. Keeps tusks of his kills on a cord around his neck. "
                     + "Competent, brave, and utterly without doubt. Killing Thorne cripples "
                     + "the Greenguard's response and lowers alert level gain across the kingdom.",
        DeathWhisper = "\"He went down swinging. They all do. …Why didn't I?\""
    };

    public static TargetData TheAssessor() => new()
    {
        Id           = "assessor",
        Name         = "The Assessor",
        Title        = "Traveling Tax Collector & Slave Appraiser",
        KingdomIndex = 0,
        IsEdictbearer = false,
        IsMandatory  = false,
        Difficulty   = 3,
        InkDrop      = InkGrade.Trace,
        InkAmount    = 1,
        Weaknesses   = new[] { "road_ambush", "market_day", "greedy" },
        MissionScenePath = "res://Scenes/Missions/Greenhold/Roads.tscn",
        IntelBrief   = "A sniveling bureaucrat who 'assesses' orc captives for sale on a sliding scale: "
                     + "healthy ones for labor, weak ones for disposal. Travels with minimal guard. "
                     + "Killing him disrupts the slave trade pipeline temporarily.",
        DeathWhisper = "\"I just wrote down what they told me to write.\""
    };

    public static TargetData SilasRootwarden() => new()
    {
        Id           = "rootwarden",
        Name         = "Silas Rootwarden",
        Title        = "Farmer & Fighting Ring Operator",
        KingdomIndex = 0,
        IsEdictbearer = false,
        IsMandatory  = false,
        Difficulty   = 4,
        InkDrop      = InkGrade.Trace,
        InkAmount    = 1,
        Weaknesses   = new[] { "barn_entrance", "fight_night", "drunk_after_events" },
        MissionScenePath = "res://Scenes/Missions/Greenhold/RootwardenFarm.tscn",
        IntelBrief   = "Runs an underground orc-fighting ring in his barn. Captures wild orcs, "
                     + "starves them, then charges nobles to watch them fight to the death. "
                     + "Popular with minor gentry. Killing him liberates captive orcs.",
        DeathWhisper = "\"It's entertainment. They were going to die anyway.\""
    };

    // ═══ OPTIONAL (morally complex) ═════════════════════════════

    public static TargetData DameCowl() => new()
    {
        Id           = "dame_cowl",
        Name         = "Dame Cowl",
        Title        = "Lady of Goldmanor",
        KingdomIndex = 0,
        IsEdictbearer = false,
        IsMandatory  = false,
        Difficulty   = 6,
        InkDrop      = InkGrade.Trace,
        InkAmount    = 1,
        Weaknesses   = new[] { "secret_passage", "child_shelter_visits", "garden_walk" },
        MissionScenePath = "res://Scenes/Missions/Greenhold/Goldmanor.tscn",
        IntelBrief   = "Harlan's wife. Runs the estate's intelligence network — knows every spy, "
                     + "every smuggler, every hidden passage. Killing her cripples Goldmanor's "
                     + "security permanently. BUT she secretly shelters escaped orc children. "
                     + "Vetch may discover this before or after the kill.",
        DeathWhisper = "\"Tell them I hid as many as I could. Tell them I'm sorry it wasn't enough.\""
    };

    // ═══ REGISTRY ═══════════════════════════════════════════════

    /// <summary>All Greenhold targets in suggested kill order.</summary>
    public static TargetData[] GetAll() => new[]
    {
        TheAssessor(),
        SilasRootwarden(),
        ReeveMaren(),
        SisterBlessing(),
        CaptainThorne(),
        DameCowl(),
        LordHarlanCowl()
    };
}
