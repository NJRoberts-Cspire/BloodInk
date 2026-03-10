using BloodInk.Campaigns.Lorne;

namespace BloodInk.Content;

/// <summary>
/// Static factory for all crafting recipe definitions.
/// Mirrors TattooRegistry / EchoRegistry pattern.
/// </summary>
public static class CraftingRecipeRegistry
{
    // ─── Salves ───────────────────────────────────────────────────

    public static CraftingRecipe HealingSalve() => new()
    {
        Id = "salve_healing",
        ResultName = "Blood-Root Salve",
        Description = "A thick paste of herb and blood-ink that knits flesh and dulls pain.",
        ItemType = CraftedItemType.Salve,
        RequiredMaterialTypes = new[] { (int)MaterialType.HerbBundle, (int)MaterialType.BloodInk },
        RequiredMaterialCounts = new[] { 2, 1 },
        Difficulty = 2,
        SteadinessRequired = 20,
        Lore = "Lorne learned this recipe from a battlefield, not a book."
    };

    public static CraftingRecipe FortifyingSalve() => new()
    {
        Id = "salve_fortify",
        ResultName = "Ironbark Salve",
        Description = "Hardens skin temporarily. Smells of peat and old iron.",
        ItemType = CraftedItemType.Salve,
        RequiredMaterialTypes = new[] { (int)MaterialType.HerbBundle, (int)MaterialType.MetalScrap },
        RequiredMaterialCounts = new[] { 1, 1 },
        Difficulty = 3,
        SteadinessRequired = 30,
        Lore = "The ore-folk at Irontide mixed metal dust into everything. Even their food."
    };

    // ─── Poisons ──────────────────────────────────────────────────

    public static CraftingRecipe SleepPoison() => new()
    {
        Id = "poison_sleep",
        ResultName = "Duskhollow Extract",
        Description = "One drop on a blade and the target sleeps for an hour. Two drops and they sleep forever.",
        ItemType = CraftedItemType.Poison,
        RequiredMaterialTypes = new[] { (int)MaterialType.HerbBundle, (int)MaterialType.RareReagent },
        RequiredMaterialCounts = new[] { 2, 1 },
        Difficulty = 5,
        SteadinessRequired = 50,
        RequiresDiscovery = true,
        Lore = "Extracted from cave-blooming nightlace. The Duskhollow shamans used it for visions."
    };

    // ─── Needles ──────────────────────────────────────────────────

    public static CraftingRecipe BoneNeedle() => new()
    {
        Id = "needle_bone",
        ResultName = "Bone-Tooth Needle",
        Description = "A tattooing needle carved from sharpened bone. Basic but reliable.",
        ItemType = CraftedItemType.Needle,
        RequiredMaterialTypes = new[] { (int)MaterialType.BoneFragment },
        RequiredMaterialCounts = new[] { 3 },
        Difficulty = 3,
        SteadinessRequired = 35,
        Lore = "Every orc tattooist starts with bone. The expensive ones end with it too."
    };

    public static CraftingRecipe RuneNeedle() => new()
    {
        Id = "needle_rune",
        ResultName = "Rune-Etched Needle",
        Description = "A precision needle etched with steadying runes. Reduces tremor during tattooing.",
        ItemType = CraftedItemType.Needle,
        RequiredMaterialTypes = new[] { (int)MaterialType.BoneFragment, (int)MaterialType.MetalScrap, (int)MaterialType.RareReagent },
        RequiredMaterialCounts = new[] { 1, 2, 1 },
        Difficulty = 7,
        SteadinessRequired = 60,
        RequiresDiscovery = true,
        Lore = "The runes are Needlewise's design. Lorne found them scratched into a prison wall."
    };

    // ─── Gadgets ──────────────────────────────────────────────────

    public static CraftingRecipe SmokeBomb() => new()
    {
        Id = "gadget_smoke",
        ResultName = "Ashbloom Bomb",
        Description = "Bursts into thick smoke on impact. Covers a retreat or masks a kill.",
        ItemType = CraftedItemType.Gadget,
        RequiredMaterialTypes = new[] { (int)MaterialType.HerbBundle, (int)MaterialType.MetalScrap },
        RequiredMaterialCounts = new[] { 1, 2 },
        Difficulty = 4,
        SteadinessRequired = 40,
        Lore = "The Thornwall resistance perfected these. For people who fight from the shadows."
    };

    // ─── Bandages ─────────────────────────────────────────────────

    public static CraftingRecipe FieldDressing() => new()
    {
        Id = "bandage_field",
        ResultName = "Blood-Ink Dressing",
        Description = "A cloth wrap soaked in diluted blood-ink. Stops bleeding and speeds mending.",
        ItemType = CraftedItemType.Bandage,
        RequiredMaterialTypes = new[] { (int)MaterialType.BindingCloth, (int)MaterialType.BloodInk },
        RequiredMaterialCounts = new[] { 2, 1 },
        Difficulty = 1,
        SteadinessRequired = 10,
        Lore = "Simple enough that even Grael can do it. Though he rarely bothers."
    };

    // ─── Tattoo Designs ───────────────────────────────────────────

    public static CraftingRecipe TattooInkMix() => new()
    {
        Id = "tattoo_ink_mix",
        ResultName = "Purified Tattoo Ink",
        Description = "Blood-ink refined with rare reagents. Required for Major-grade tattoos.",
        ItemType = CraftedItemType.TattooDesign,
        RequiredMaterialTypes = new[] { (int)MaterialType.BloodInk, (int)MaterialType.RareReagent },
        RequiredMaterialCounts = new[] { 3, 2 },
        Difficulty = 8,
        SteadinessRequired = 70,
        RequiresDiscovery = true,
        Lore = "Needlewise whispers the proportions. Lorne's hands remember them."
    };

    // ─── Registry ─────────────────────────────────────────────────

    private static CraftingRecipe[]? _all;

    /// <summary>All crafting recipes in the game (cached).</summary>
    public static CraftingRecipe[] GetAll() => _all ??= new[]
    {
        HealingSalve(), FortifyingSalve(),
        SleepPoison(),
        BoneNeedle(), RuneNeedle(),
        SmokeBomb(),
        FieldDressing(),
        TattooInkMix()
    };
}
