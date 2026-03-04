using Godot;

namespace BloodInk.Campaigns;

/// <summary>
/// Enum identifying the three playable campaign hands.
/// </summary>
public enum CampaignHand
{
    /// <summary>Vetch — the main Edictbearer assassin campaign.</summary>
    Vetch,

    /// <summary>Rukh — the Bone-Scrawl spymaster campaign.</summary>
    Rukh,

    /// <summary>Grael — the Bull-Blood warband commander campaign.</summary>
    Grael,

    /// <summary>Lorne — the Needle-Kin tattooist/crafter campaign.</summary>
    Lorne
}

/// <summary>
/// Resource that describes a campaign's metadata and unlock requirements.
/// </summary>
[GlobalClass]
public partial class CampaignData : Resource
{
    /// <summary>Which hand this campaign belongs to.</summary>
    [Export] public CampaignHand Hand { get; set; } = CampaignHand.Vetch;

    /// <summary>Display name shown in the menu.</summary>
    [Export] public string DisplayName { get; set; } = "";

    /// <summary>Short description of the campaign's gameplay focus.</summary>
    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = "";

    /// <summary>Kingdoms this campaign covers (indices 0-5).</summary>
    [Export] public int[] KingdomIndices { get; set; } = System.Array.Empty<int>();

    /// <summary>
    /// Which kingdom in Vetch's main campaign must be completed to unlock this.
    /// -1 means available from the start (Vetch's own).
    /// </summary>
    [Export] public int UnlockAfterKingdom { get; set; } = -1;

    /// <summary>Starting scene for this campaign.</summary>
    [Export] public string StartScenePath { get; set; } = "";

    /// <summary>Whether completing this campaign unlocks a unique tattoo.</summary>
    [Export] public bool GrantsUniqueTattoo { get; set; } = false;

    /// <summary>ID of the tattoo granted on completion, if any.</summary>
    [Export] public string CompletionTattooId { get; set; } = "";

    /// <summary>Flavour text shown on unlock.</summary>
    [Export(PropertyHint.MultilineText)]
    public string UnlockLore { get; set; } = "";
}
