using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloodInk.Campaigns.Lorne;

/// <summary>
/// Result of a crafting attempt — quality depends on Lorne's steadiness.
/// </summary>
public enum CraftQuality
{
    /// <summary>Perfect execution. Full bonus effects.</summary>
    Masterwork,

    /// <summary>Clean work. Standard effects.</summary>
    Standard,

    /// <summary>Minor flaws. Slightly reduced effects.</summary>
    Flawed,

    /// <summary>Ruined by tremor. Materials wasted, item unusable.</summary>
    Ruined
}

/// <summary>
/// Manages Lorne's crafting system — material inventory, recipe knowledge,
/// and the crafting process itself (which interacts with TremorSystem).
/// </summary>
public partial class CraftingSystem : Node
{
    [Signal] public delegate void ItemCraftedEventHandler(string recipeId, int quality);
    [Signal] public delegate void RecipeLearnedEventHandler(string recipeId);
    [Signal] public delegate void MaterialGainedEventHandler(int materialType, int amount);

    /// <summary>Material inventory: type → count.</summary>
    private readonly Dictionary<MaterialType, int> _materials = new();

    /// <summary>Known recipes by ID.</summary>
    private readonly Dictionary<string, CraftingRecipe> _knownRecipes = new();

    /// <summary>All recipes (including undiscovered).</summary>
    private readonly Dictionary<string, CraftingRecipe> _allRecipes = new();

    /// <summary>Count of items crafted per recipe (for mastery tracking).</summary>
    private readonly Dictionary<string, int> _craftCount = new();

    /// <summary>Reference to the tremor system for steadiness checks.</summary>
    public TremorSystem? Tremor { get; set; }

    private readonly Random _rng = new();

    // ─── Material Management ──────────────────────────────────────

    public void AddMaterial(MaterialType type, int count)
    {
        if (!_materials.ContainsKey(type))
            _materials[type] = 0;
        _materials[type] += count;
        EmitSignal(SignalName.MaterialGained, (int)type, count);
    }

    public int GetMaterialCount(MaterialType type) =>
        _materials.TryGetValue(type, out var c) ? c : 0;

    public bool HasMaterials(CraftingRecipe recipe)
    {
        for (int i = 0; i < recipe.RequiredMaterialTypes.Length; i++)
        {
            var type = (MaterialType)recipe.RequiredMaterialTypes[i];
            int needed = i < recipe.RequiredMaterialCounts.Length ? recipe.RequiredMaterialCounts[i] : 1;
            if (GetMaterialCount(type) < needed)
                return false;
        }
        return true;
    }

    private void ConsumeMaterials(CraftingRecipe recipe)
    {
        for (int i = 0; i < recipe.RequiredMaterialTypes.Length; i++)
        {
            var type = (MaterialType)recipe.RequiredMaterialTypes[i];
            int needed = i < recipe.RequiredMaterialCounts.Length ? recipe.RequiredMaterialCounts[i] : 1;
            _materials[type] -= needed;
        }
    }

    // ─── Recipe Management ────────────────────────────────────────

    public void RegisterRecipe(CraftingRecipe recipe)
    {
        _allRecipes[recipe.Id] = recipe;
        if (!recipe.RequiresDiscovery)
            _knownRecipes[recipe.Id] = recipe;
    }

    public void DiscoverRecipe(string recipeId)
    {
        if (_allRecipes.TryGetValue(recipeId, out var recipe))
        {
            _knownRecipes[recipeId] = recipe;
            EmitSignal(SignalName.RecipeLearned, recipeId);
            GD.Print($"Recipe discovered: {recipe.ResultName}");
        }
    }

    public bool IsRecipeKnown(string recipeId) => _knownRecipes.ContainsKey(recipeId);

    public IEnumerable<CraftingRecipe> GetKnownRecipes() => _knownRecipes.Values;

    public IEnumerable<CraftingRecipe> GetCraftableRecipes() =>
        _knownRecipes.Values.Where(HasMaterials);

    // ─── Crafting ─────────────────────────────────────────────────

    /// <summary>
    /// Attempt to craft a recipe. Quality depends on Lorne's steadiness vs recipe difficulty.
    /// Returns the quality of the result, or null if crafting can't proceed.
    /// </summary>
    public CraftQuality? Craft(string recipeId)
    {
        if (!_knownRecipes.TryGetValue(recipeId, out var recipe))
        {
            GD.PrintErr($"Recipe '{recipeId}' is not known.");
            return null;
        }

        if (!HasMaterials(recipe))
        {
            GD.PrintErr($"Not enough materials for '{recipe.ResultName}'.");
            return null;
        }

        // Apply tremor stress from crafting.
        Tremor?.OnDelicateWork(recipe.Difficulty);

        float steadiness = Tremor?.Steadiness ?? 80f;

        // Mastery bonus: each time you've crafted this recipe, +2 effective steadiness.
        int mastery = _craftCount.TryGetValue(recipeId, out var mc) ? mc : 0;
        steadiness += mastery * 2f;
        steadiness = Math.Min(100f, steadiness);

        // Determine quality.
        float gap = steadiness - recipe.SteadinessRequired;
        CraftQuality quality;

        if (Tremor?.IsFlaring == true)
        {
            // Flaring almost always ruins the craft.
            quality = _rng.NextDouble() < 0.85 ? CraftQuality.Ruined : CraftQuality.Flawed;
        }
        else if (gap >= 30)
        {
            quality = _rng.NextDouble() < 0.4 ? CraftQuality.Masterwork : CraftQuality.Standard;
        }
        else if (gap >= 10)
        {
            quality = CraftQuality.Standard;
        }
        else if (gap >= -10)
        {
            quality = _rng.NextDouble() < 0.5 ? CraftQuality.Flawed : CraftQuality.Standard;
        }
        else
        {
            quality = _rng.NextDouble() < 0.6 ? CraftQuality.Ruined : CraftQuality.Flawed;
        }

        // Consume materials regardless of quality.
        ConsumeMaterials(recipe);

        // Track mastery.
        _craftCount[recipeId] = mastery + 1;

        EmitSignal(SignalName.ItemCrafted, recipeId, (int)quality);
        GD.Print($"Crafted '{recipe.ResultName}': {quality} (Steadiness: {steadiness:F0}, Required: {recipe.SteadinessRequired})");

        return quality;
    }

    // ─── Serialization ────────────────────────────────────────────

    public Dictionary<string, object> Serialize()
    {
        var mats = new Dictionary<string, int>();
        foreach (var (type, count) in _materials)
            mats[type.ToString()] = count;

        return new Dictionary<string, object>
        {
            ["materials"] = mats,
            ["knownRecipes"] = _knownRecipes.Keys.ToList(),
            ["craftCounts"] = new Dictionary<string, int>(_craftCount)
        };
    }
}
