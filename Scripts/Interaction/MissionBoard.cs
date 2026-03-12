using Godot;
using BloodInk.Core;
using BloodInk.Dialogue;
using BloodInk.Progression;
using System.Collections.Generic;
using System.Linq;

namespace BloodInk.Interaction;

/// <summary>
/// The Mission Board in camp. When interacted, presents all living targets
/// from the current kingdom and lets the player choose a mission to deploy to.
/// Dynamically generates dialogue from KingdomState target data.
/// </summary>
public partial class MissionBoard : Interactable
{
    /// <summary>Cached callable to prevent identity mismatch on IsConnected/Disconnect.</summary>
    private readonly Callable _onDialogueEventCallable;

    public MissionBoard()
    {
        _onDialogueEventCallable = Callable.From<string>(OnDialogueEvent);
    }

    protected override void InteractableReady()
    {
        DisplayName = "Mission Board";
        ActionVerb = "Check";
    }

    public override void OnInteract(Node2D interactor)
    {
        ShowMissionList();
        base.OnInteract(interactor);
    }

    private void ShowMissionList()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // For the vertical slice, work with Kingdom 0 (Greenhold).
        var kingdom = gm.Kingdoms[0];
        var living = kingdom.GetLivingTargets().ToList();
        var killed = kingdom.GetKilledTargets().ToList();

        var lines = new List<DialogueLine>();

        // ── Header ────────────────────────────────────────────
        lines.Add(new DialogueLine
        {
            Id = "header",
            Speaker = "",
            Text = "═══ MISSION BOARD ═══\nThe Greenhold — Active Targets",
            IsEntry = true,
            NextLineId = killed.Count > 0 ? "status" : (living.Count > 0 ? "choose" : "done")
        });

        // ── Show killed targets summary if any ────────────────
        if (killed.Count > 0)
        {
            string statusText = string.Join("\n",
                killed.Select(t => $"  ✓ {t.Name} — ELIMINATED"));
            lines.Add(new DialogueLine
            {
                Id = "status",
                Speaker = "",
                Text = statusText,
                NextLineId = living.Count > 0 ? "choose" : "done"
            });
        }

        // ── All targets dead ──────────────────────────────────
        if (living.Count == 0)
        {
            lines.Add(new DialogueLine
            {
                Id = "done",
                Speaker = "",
                Text = "All Greenhold targets eliminated.\nThe Edict's grip on this kingdom is broken.\nVisit the Needlewise to receive your tattoo."
            });
        }
        else
        {
            // ── Build target choices ──────────────────────────
            var choices = new List<string>();
            foreach (var target in living.OrderBy(t => t.Difficulty))
            {
                // Only show targets whose mission scene exists.
                bool sceneExists = ResourceLoader.Exists(target.MissionScenePath);
                string stars = new string('★', target.Difficulty)
                             + new string('☆', 10 - target.Difficulty);
                string label;
                if (target.IsEdictbearer)
                    label = $"{target.Name} ({stars}) [EDICTBEARER]";
                else
                    label = $"{target.Name} ({stars})";

                if (sceneExists)
                    choices.Add($"{label}|brief_{target.Id}");
                else
                    choices.Add($"{label} [INTEL NEEDED]|unavail_{target.Id}");
            }
            choices.Add("Not yet.|cancel");

            lines.Add(new DialogueLine
            {
                Id = "choose",
                Speaker = "",
                Text = "Select a target:",
                Choices = choices.ToArray()
            });

            // ── Generate briefing + deploy for each target ────
            foreach (var target in living)
            {
                bool sceneExists = ResourceLoader.Exists(target.MissionScenePath);

                if (!sceneExists)
                {
                    // Unavailable target — intel not yet gathered.
                    lines.Add(new DialogueLine
                    {
                        Id = $"unavail_{target.Id}",
                        Speaker = "",
                        Text = $"TARGET: {target.Name}\n{target.Title}\n\nRukh's spies haven't mapped this location yet.\nComplete other missions to gather more intel.",
                        NextLineId = "choose"
                    });
                    continue;
                }

                // Briefing line.
                string diffStars = new string('★', target.Difficulty) + new string('☆', 10 - target.Difficulty);
                lines.Add(new DialogueLine
                {
                    Id = $"brief_{target.Id}",
                    Speaker = "",
                    Text = $"TARGET: {target.Name}\n{target.Title}\nDIFFICULTY: {diffStars}",
                    NextLineId = $"intel_{target.Id}"
                });

                // Intel + deploy choice.
                string gradeText = target.InkDrop.ToString();
                string echoBonus = target.IsEdictbearer ? "\nBlood Echo: Yes" : "";
                lines.Add(new DialogueLine
                {
                    Id = $"intel_{target.Id}",
                    Speaker = "",
                    Text = $"REWARD: {target.InkAmount}× {gradeText} Blood-Ink{echoBonus}",
                    Choices = new[]
                    {
                        $"Deploy.|deploy_{target.Id}",
                        "Back.|choose"
                    }
                });

                // Deploy confirmation.
                lines.Add(new DialogueLine
                {
                    Id = $"deploy_{target.Id}",
                    Speaker = "",
                    Text = "Preparing for deployment...",
                    Event = $"deploy:{target.MissionScenePath}"
                });
            }

            lines.Add(new DialogueLine
            {
                Id = "cancel",
                Speaker = "",
                Text = "The board awaits your decision."
            });
        }

        var dialogue = new DialogueData
        {
            ConversationId = "mission_board",
            Lines = lines.ToArray()
        };

        DialogueManager.Instance?.StartConversation(dialogue);

        // Listen for deploy events.
        if (DialogueManager.Instance != null)
        {
            if (DialogueManager.Instance.IsConnected(
                    DialogueManager.SignalName.DialogueEventFired,
                    _onDialogueEventCallable))
            {
                DialogueManager.Instance.Disconnect(
                    DialogueManager.SignalName.DialogueEventFired,
                    _onDialogueEventCallable);
            }

            DialogueManager.Instance.Connect(
                DialogueManager.SignalName.DialogueEventFired,
                _onDialogueEventCallable,
                (uint)GodotObject.ConnectFlags.OneShot
            );
        }
    }

    private void OnDialogueEvent(string eventKey)
    {
        if (eventKey.StartsWith("deploy:"))
        {
            string scenePath = eventKey.Substring("deploy:".Length);
            CallDeferred(MethodName.DeployToMission, scenePath);
        }
    }

    private void DeployToMission(string scenePath)
    {
        // Capture tree reference before timer to avoid ObjectDisposedException if node freed.
        var tree = GetTree();
        var timer = tree.CreateTimer(0.5f);
        timer.Timeout += () =>
        {
            GD.Print($"Deploying to {scenePath}...");
            tree.ChangeSceneToFile(scenePath);
        };
    }
}
