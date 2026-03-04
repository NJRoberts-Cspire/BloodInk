using Godot;
using BloodInk.Dialogue;

namespace BloodInk.Interaction;

/// <summary>
/// An NPC you can talk to. Triggers a dialogue conversation
/// via the DialogueManager when interacted with.
/// </summary>
public partial class DialogueNPC : Interactable
{
    /// <summary>The dialogue resource for this NPC.</summary>
    [Export] public DialogueData? Dialogue { get; set; }

    /// <summary>Portrait key used for this NPC.</summary>
    [Export] public string PortraitKey { get; set; } = "";

    /// <summary>Whether this NPC has been talked to at least once.</summary>
    public bool HasBeenTalkedTo { get; private set; } = false;

    protected override void InteractableReady()
    {
        ActionVerb = "Talk to";
    }

    public override void OnInteract(Node2D interactor)
    {
        if (Dialogue == null)
        {
            GD.Print($"{DisplayName} has nothing to say.");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            GD.PrintErr("DialogueNPC: No DialogueManager instance.");
            return;
        }

        // Don't start a new conversation if one is already active.
        if (DialogueManager.Instance.IsActive) return;

        HasBeenTalkedTo = true;
        DialogueManager.Instance.StartConversation(Dialogue);

        base.OnInteract(interactor);
    }

    /// <summary>Swap the dialogue resource (e.g., after a story event).</summary>
    public void SetDialogue(DialogueData newDialogue)
    {
        Dialogue = newDialogue;
    }
}
