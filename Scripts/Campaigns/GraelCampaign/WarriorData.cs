using Godot;

namespace BloodInk.Campaigns.Grael;

/// <summary>
/// Defines a warrior in Grael's warband.
/// </summary>
[GlobalClass]
public partial class WarriorData : Resource
{
    /// <summary>Unique warrior identifier.</summary>
    [Export] public string Id { get; set; } = "";

    /// <summary>Warrior name.</summary>
    [Export] public string Name { get; set; } = "";

    /// <summary>Class/role in the warband.</summary>
    [Export] public WarriorRole Role { get; set; } = WarriorRole.Brawler;

    /// <summary>Combat power rating 1-100.</summary>
    [Export(PropertyHint.Range, "1,100")]
    public int Strength { get; set; } = 20;

    /// <summary>Toughness / damage resistance.</summary>
    [Export(PropertyHint.Range, "1,100")]
    public int Endurance { get; set; } = 20;

    /// <summary>Morale 0-100. At 0, warrior routs.</summary>
    [Export(PropertyHint.Range, "0,100")]
    public int Morale { get; set; } = 70;

    /// <summary>Whether this warrior has fallen in battle.</summary>
    [Export] public bool IsDead { get; set; } = false;

    /// <summary>Number of raids survived.</summary>
    [Export] public int RaidsSurvived { get; set; } = 0;

    /// <summary>Flavour text.</summary>
    [Export(PropertyHint.MultilineText)]
    public string Lore { get; set; } = "";
}

/// <summary>
/// Warrior roles in Grael's warband.
/// </summary>
public enum WarriorRole
{
    /// <summary>Front-line melee fighter.</summary>
    Brawler,

    /// <summary>Heavy shield bearer, absorbs damage.</summary>
    ShieldBearer,

    /// <summary>Fast flanker, bonus on pincer attacks.</summary>
    Flanker,

    /// <summary>Ranged attacker with javelins/bows.</summary>
    Skirmisher,

    /// <summary>Healer/buffer with war-chants.</summary>
    WarChanter,

    /// <summary>Demolitions expert, breaks fortifications.</summary>
    Breaker
}
