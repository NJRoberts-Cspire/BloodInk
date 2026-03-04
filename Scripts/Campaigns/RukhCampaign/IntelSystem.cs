using Godot;

namespace BloodInk.Campaigns.Rukh;

/// <summary>
/// Types of intelligence that agents can gather.
/// </summary>
public enum IntelType
{
    /// <summary>Guard routes and schedules.</summary>
    PatrolRoute,

    /// <summary>Target daily habits and location.</summary>
    TargetSchedule,

    /// <summary>Building layout / secret passages.</summary>
    MapData,

    /// <summary>Political leverage / blackmail material.</summary>
    Blackmail,

    /// <summary>Weakness in fortifications (for Grael raids).</summary>
    Vulnerability,

    /// <summary>Supply chain info (for Lorne's crafting).</summary>
    SupplyInfo,

    /// <summary>Rumour or whisper — may be false.</summary>
    Rumour
}

/// <summary>
/// A single piece of intelligence gathered by an agent.
/// </summary>
[GlobalClass]
public partial class IntelData : Resource
{
    /// <summary>Unique identifier.</summary>
    [Export] public string Id { get; set; } = "";

    /// <summary>What kind of intel this is.</summary>
    [Export] public IntelType Type { get; set; } = IntelType.Rumour;

    /// <summary>Kingdom this intel pertains to.</summary>
    [Export] public int KingdomIndex { get; set; } = 0;

    /// <summary>Brief summary shown in the spy network UI.</summary>
    [Export] public string Summary { get; set; } = "";

    /// <summary>Full description with details.</summary>
    [Export(PropertyHint.MultilineText)]
    public string FullText { get; set; } = "";

    /// <summary>Whether this intel is verified (vs rumour that may be false).</summary>
    [Export] public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Gameplay effect key — interpreted by the mission/kingdom systems.
    /// E.g. "guard_route_palace_east", "target_weakness_poison".
    /// </summary>
    [Export] public string EffectKey { get; set; } = "";

    /// <summary>Agent who gathered this intel.</summary>
    [Export] public string SourceAgentId { get; set; } = "";
}
