using Godot;
using BloodInk.Core;
using BloodInk.Dialogue;

namespace BloodInk.Interaction;

/// <summary>
/// A camp NPC whose dialogue updates based on the player's progress.
/// Set up to three dialogue tiers in the inspector; the highest unlocked
/// tier is shown at runtime.
///
///   DefaultDialogue         — shown before any kills (early game)
///   PostFirstMissionDialogue — shown after the first target is killed (Cruelty >= 3)
///   LateGameDialogue         — shown deep into the campaign      (Cruelty >= 12)
///
/// Call <see cref="RefreshDialogue"/> any time game state changes
/// (e.g., on returning to camp after a mission).
/// </summary>
public partial class CampNPC : DialogueNPC
{
    /// <summary>Dialogue shown before any missions are completed.</summary>
    [Export] public DialogueData? DefaultDialogue { get; set; }

    /// <summary>Dialogue unlocked after the player's first kill (Cruelty ≥ 3).</summary>
    [Export] public DialogueData? PostFirstMissionDialogue { get; set; }

    /// <summary>Dialogue unlocked deep in the campaign (Cruelty ≥ 12, ~4 kills).</summary>
    [Export] public DialogueData? LateGameDialogue { get; set; }

    protected override void InteractableReady()
    {
        base.InteractableReady();
        CallDeferred(MethodName.RefreshDialogue);
    }

    /// <summary>
    /// Pick and apply the highest-tier dialogue that the current game state unlocks.
    /// Call this whenever returning to camp after a mission.
    /// </summary>
    public void RefreshDialogue()
    {
        var choices = GameManager.Instance?.Choices;
        int cruelty  = choices?.Cruelty ?? 0;
        int mercy    = choices?.Mercy   ?? 0;

        // Use combined moral weight as a proxy for "how far into the story we are".
        int progress = cruelty + mercy;

        DialogueData? chosen =
            progress >= 12 && LateGameDialogue       != null ? LateGameDialogue
          : progress >= 3  && PostFirstMissionDialogue != null ? PostFirstMissionDialogue
          : DefaultDialogue;

        if (chosen != null)
            SetDialogue(chosen);
    }
}
