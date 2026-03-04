using Godot;
using BloodInk.Content;
using BloodInk.Core;
using BloodInk.Dialogue;

namespace BloodInk.Interaction;

/// <summary>
/// The Mission Board in camp. When interacted, presents available targets
/// and lets the player choose a mission to deploy to.
/// For the vertical slice, only Lord Cowl's mission is available.
/// </summary>
public partial class MissionBoard : Interactable
{
    /// <summary>Whether the player has completed the Cowl mission.</summary>
    private bool _cowlCompleted = false;

    protected override void InteractableReady()
    {
        DisplayName = "Mission Board";
        ActionVerb = "Check";
    }

    public override void OnInteract(Node2D interactor)
    {
        // Check if Cowl is already dead.
        var gm = GameManager.Instance;
        if (gm != null)
        {
            var greenhold = gm.Kingdoms[0];
            _cowlCompleted = greenhold?.IsTargetKilled("cowl") ?? false;
        }

        if (_cowlCompleted)
        {
            // Show completion dialogue.
            ShowCompletionDialogue();
        }
        else
        {
            // Show mission briefing with deploy option.
            ShowMissionBriefing();
        }

        base.OnInteract(interactor);
    }

    private void ShowMissionBriefing()
    {
        var dialogue = new DialogueData
        {
            ConversationId = "mission_board_cowl",
            Lines = new DialogueLine[]
            {
                new() { Id = "1", Speaker = "", Text = "═══ MISSION BOARD ═══", IsEntry = true, NextLineId = "2" },
                new() { Id = "2", Speaker = "", Text = "TARGET: Lord Harlan Cowl\nLOCATION: Goldmanor, The Greenhold\nDIFFICULTY: ★★★★★★★★☆☆", NextLineId = "3" },
                new() { Id = "3", Speaker = "", Text = "INTEL: Evening walk on west balcony. Wine cellar entry. Servant entrance after sundown.", NextLineId = "4" },
                new() { Id = "4", Speaker = "", Text = "REWARD: Major Blood-Ink (Shadow Step tattoo)", NextLineId = "5" },
                new() { Id = "5", Speaker = "", Text = "Deploy to Goldmanor?", Choices = new[]
                {
                    "Deploy.|deploy",
                    "Not yet.|cancel"
                }},
                new() { Id = "deploy", Speaker = "", Text = "Preparing for deployment...", Event = "deploy_goldmanor" },
                new() { Id = "cancel", Speaker = "", Text = "The board awaits your decision." },
            }
        };

        DialogueManager.Instance?.StartConversation(dialogue);

        // Listen for the deploy event.
        if (DialogueManager.Instance != null)
        {
            // Disconnect previous if any.
            if (DialogueManager.Instance.IsConnected(DialogueManager.SignalName.DialogueEventFired, Callable.From<string>(OnDialogueEvent)))
                DialogueManager.Instance.Disconnect(DialogueManager.SignalName.DialogueEventFired, Callable.From<string>(OnDialogueEvent));

            DialogueManager.Instance.Connect(
                DialogueManager.SignalName.DialogueEventFired,
                Callable.From<string>(OnDialogueEvent),
                (uint)GodotObject.ConnectFlags.OneShot
            );
        }
    }

    private void ShowCompletionDialogue()
    {
        var dialogue = new DialogueData
        {
            ConversationId = "mission_board_complete",
            Lines = new DialogueLine[]
            {
                new() { Id = "1", Speaker = "", Text = "═══ MISSION BOARD ═══", IsEntry = true, NextLineId = "2" },
                new() { Id = "2", Speaker = "", Text = "TARGET: Lord Harlan Cowl — ELIMINATED ✓\nThe Greenhold's Edict anchor is broken.", NextLineId = "3" },
                new() { Id = "3", Speaker = "", Text = "Visit the Needlewise to receive your tattoo." },
            }
        };

        DialogueManager.Instance?.StartConversation(dialogue);
    }

    private void OnDialogueEvent(string eventKey)
    {
        if (eventKey == "deploy_goldmanor")
        {
            // Wait for dialogue to end, then deploy.
            CallDeferred(MethodName.DeployToGoldmanor);
        }
    }

    private void DeployToGoldmanor()
    {
        // Small delay to let dialogue close.
        var timer = GetTree().CreateTimer(0.5f);
        timer.Timeout += () =>
        {
            GD.Print("Deploying to Goldmanor...");
            GetTree().ChangeSceneToFile("res://Scenes/Missions/Greenhold/Goldmanor.tscn");
        };
    }
}
