using Godot;

namespace BloodInk.Campaigns.Lorne;

/// <summary>
/// Types of materials used in Lorne's crafting.
/// </summary>
public enum MaterialType
{
    /// <summary>Harvested blood-ink (primary resource).</summary>
    BloodInk,

    /// <summary>Bone fragments for needle crafting.</summary>
    BoneFragment,

    /// <summary>Herbs and plant matter for salves/poultices.</summary>
    HerbBundle,

    /// <summary>Metal scraps for tool repair.</summary>
    MetalScrap,

    /// <summary>Rare reagent found only in specific kingdoms.</summary>
    RareReagent,

    /// <summary>Binding cloth for bandages and wraps.</summary>
    BindingCloth
}

/// <summary>
/// Resource defining a single crafting recipe.
/// </summary>
[GlobalClass]
public partial class CraftingRecipe : Resource
{
    /// <summary>Unique recipe identifier.</summary>
    [Export] public string Id { get; set; } = "";

    /// <summary>What this recipe produces.</summary>
    [Export] public string ResultName { get; set; } = "";

    /// <summary>Description of the crafted item.</summary>
    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = "";

    /// <summary>Category of item produced.</summary>
    [Export] public CraftedItemType ItemType { get; set; } = CraftedItemType.Salve;

    /// <summary>Required material types (as int indices of MaterialType enum).</summary>
    [Export] public int[] RequiredMaterialTypes { get; set; } = System.Array.Empty<int>();
    [Export] public int[] RequiredMaterialCounts { get; set; } = System.Array.Empty<int>();

    /// <summary>
    /// Difficulty 1-10. Higher difficulty = more tremor during crafting.
    /// </summary>
    [Export(PropertyHint.Range, "1,10")]
    public int Difficulty { get; set; } = 3;

    /// <summary>
    /// Minimum steadiness score (0-100) needed to craft without defects.
    /// </summary>
    [Export(PropertyHint.Range, "0,100")]
    public int SteadinessRequired { get; set; } = 40;

    /// <summary>Whether this recipe must be discovered before it can be used.</summary>
    [Export] public bool RequiresDiscovery { get; set; } = false;

    /// <summary>ID of the tattoo this recipe creates, if it's a tattoo recipe.</summary>
    [Export] public string TattooResultId { get; set; } = "";

    /// <summary>Flavour text about the recipe's origin.</summary>
    [Export(PropertyHint.MultilineText)]
    public string Lore { get; set; } = "";
}

/// <summary>
/// Categories of craftable items.
/// </summary>
public enum CraftedItemType
{
    /// <summary>Healing salve.</summary>
    Salve,

    /// <summary>Poison or combat coating.</summary>
    Poison,

    /// <summary>Trap component.</summary>
    TrapPart,

    /// <summary>Needle/tool for tattooing.</summary>
    Needle,

    /// <summary>A new tattoo design.</summary>
    TattooDesign,

    /// <summary>Bandage or field dressing.</summary>
    Bandage,

    /// <summary>Smoke bomb or distraction tool.</summary>
    Gadget
}
