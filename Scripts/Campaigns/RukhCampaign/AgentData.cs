using Godot;

namespace BloodInk.Campaigns.Rukh;

/// <summary>
/// Defines an agent in Rukh's spy network.
/// Agents are placed in kingdoms to gather intel, mark targets, or create diversions.
/// </summary>
[GlobalClass]
public partial class AgentData : Resource
{
    /// <summary>Unique agent identifier.</summary>
    [Export] public string Id { get; set; } = "";

    /// <summary>Agent's code name.</summary>
    [Export] public string CodeName { get; set; } = "";

    /// <summary>Agent's cover identity / role in the kingdom.</summary>
    [Export] public string CoverRole { get; set; } = "";

    /// <summary>Which kingdom this agent is embedded in (index 0-5).</summary>
    [Export] public int KingdomIndex { get; set; } = 0;

    /// <summary>Agent's skill level 0-100. Higher = more reliable.</summary>
    [Export(PropertyHint.Range, "0,100")]
    public int SkillLevel { get; set; } = 30;

    /// <summary>Agent's current loyalty 0-100. Below 20 = risk of betrayal.</summary>
    [Export(PropertyHint.Range, "0,100")]
    public int Loyalty { get; set; } = 60;

    /// <summary>Whether the agent's cover has been blown.</summary>
    [Export] public bool IsCompromised { get; set; } = false;

    /// <summary>Whether the agent is currently on a mission.</summary>
    [Export] public bool IsOnMission { get; set; } = false;

    /// <summary>Flavour text about the agent's background.</summary>
    [Export(PropertyHint.MultilineText)]
    public string BackgroundLore { get; set; } = "";
}
