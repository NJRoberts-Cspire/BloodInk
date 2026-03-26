using Godot;

namespace BloodInk.Progression;

/// <summary>
/// Defines an assassination target within a kingdom.
/// </summary>
[GlobalClass]
public partial class TargetData : Resource
{
    /// <summary>Unique target identifier.</summary>
    [Export] public string Id { get; set; } = "";

    /// <summary>Target's name.</summary>
    [Export] public string Name { get; set; } = "";

    /// <summary>Target's role/title.</summary>
    [Export] public string Title { get; set; } = "";

    /// <summary>Which kingdom (index 0-5).</summary>
    [Export] public int KingdomIndex { get; set; } = 0;

    /// <summary>Whether this is the Edictbearer (primary target).</summary>
    [Export] public bool IsEdictbearer { get; set; } = false;

    /// <summary>Whether killing this target is required to advance.</summary>
    [Export] public bool IsMandatory { get; set; } = true;

    /// <summary>Target difficulty 1-10.</summary>
    [Export(PropertyHint.Range, "1,10")]
    public int Difficulty { get; set; } = 5;

    /// <summary>Scene path for the target's assassination mission.</summary>
    [Export] public string MissionScenePath { get; set; } = "";

    /// <summary>Blood-ink grade dropped on kill.</summary>
    [Export] public Ink.InkGrade InkDrop { get; set; } = Ink.InkGrade.Lesser;

    /// <summary>Amount of ink dropped.</summary>
    [Export] public int InkAmount { get; set; } = 1;

    /// <summary>Blood Echo ID unlocked on kill (if Major ink).</summary>
    [Export] public string BloodEchoId { get; set; } = "";

    /// <summary>Weaknesses revealed by intel (checked against Rukh's IntelSystem).</summary>
    [Export] public string[] Weaknesses { get; set; } = System.Array.Empty<string>();

    /// <summary>Description / intel brief.</summary>
    [Export(PropertyHint.MultilineText)]
    public string IntelBrief { get; set; } = "";

    /// <summary>Flavour text whispered by the blood.</summary>
    [Export(PropertyHint.MultilineText)]
    public string DeathWhisper { get; set; } = "";

    /// <summary>
    /// When true, MissionBoard shows this target as locked rather than deployable,
    /// even if a scene file exists.
    /// </summary>
    [Export] public bool IsLocked { get; set; } = false;

    /// <summary>Short reason shown to the player when the target is locked (e.g. "Intel Required").</summary>
    [Export] public string LockReason { get; set; } = "";
}
