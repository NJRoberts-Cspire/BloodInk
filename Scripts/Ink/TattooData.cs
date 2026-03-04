using Godot;

namespace BloodInk.Ink;

/// <summary>
/// Resource defining a single tattoo: its slot, effects, ink cost, and temperament affinity.
/// Create instances in the editor or in code for each tattoo in the game.
/// </summary>
[GlobalClass]
public partial class TattooData : Resource
{
    [Export] public string Id { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";

    [ExportGroup("Slot & Cost")]
    [Export] public TattooSlot Slot { get; set; }
    [Export] public InkGrade RequiredGrade { get; set; } = InkGrade.Lesser;
    [Export] public int InkCost { get; set; } = 1;

    /// <summary>Which kingdom's Edictbearer must be dead to unlock this category (for Major grade).</summary>
    [Export] public int RequiredKingdomIndex { get; set; } = -1;

    [ExportGroup("Temperament")]
    /// <summary>Primary temperament this tattoo leans toward.</summary>
    [Export] public InkTemperament PrimaryTemperament { get; set; }
    /// <summary>How strongly this tattoo pushes toward its temperament (0-100).</summary>
    [Export] public int TemperamentWeight { get; set; } = 10;

    [ExportGroup("Stat Modifiers")]
    [Export] public float StealthBonus { get; set; }
    [Export] public float DamageBonus { get; set; }
    [Export] public float SpeedBonus { get; set; }
    [Export] public float HealthBonus { get; set; }
    [Export] public float DetectionRadiusModifier { get; set; }
    [Export] public float TrapEffectivenessBonus { get; set; }
    [Export] public float HealingBonus { get; set; }
    [Export] public float ResistanceBonus { get; set; }

    [ExportGroup("Special Abilities")]
    /// <summary>Scene path to the ability this tattoo grants (e.g., Shadow Dash).</summary>
    [Export] public string AbilityScenePath { get; set; } = "";
    /// <summary>If true, this tattoo grants an active ability. Otherwise it's passive.</summary>
    [Export] public bool GrantsActiveAbility { get; set; }

    [ExportGroup("Lore")]
    /// <summary>The Edictbearer memory fragment that whispers when this tattoo is applied.</summary>
    [Export(PropertyHint.MultilineText)] public string WhisperText { get; set; } = "";
    /// <summary>ID of the Blood Echo unlocked by this tattoo (if Major grade).</summary>
    [Export] public string BloodEchoId { get; set; } = "";

    [ExportGroup("Evolution")]
    /// <summary>Evolved version of this tattoo if temperament threshold is met.</summary>
    [Export] public TattooData? EvolvedForm { get; set; }
    /// <summary>Temperament score threshold to trigger evolution.</summary>
    [Export] public int EvolutionThreshold { get; set; } = 50;
}
