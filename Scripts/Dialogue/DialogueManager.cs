using Godot;
using System;
using System.Collections.Generic;

namespace BloodInk.Dialogue;

/// <summary>
/// Drives a conversation forward — resolves conditions, advances lines,
/// processes choices, sets flags, and fires events.
/// One instance per active conversation; handles all state.
/// </summary>
public partial class DialogueManager : Node
{
    // ─── Signals ──────────────────────────────────────────────────
    [Signal] public delegate void LineDisplayedEventHandler(string speaker, string portrait, string text);
    [Signal] public delegate void ChoicesPresentedEventHandler(string[] labels);
    [Signal] public delegate void ConversationStartedEventHandler(string conversationId);
    [Signal] public delegate void ConversationEndedEventHandler(string conversationId);
    [Signal] public delegate void DialogueEventFiredEventHandler(string eventKey);
    [Signal] public delegate void FlagChangedEventHandler(string flag, string value);

    public static DialogueManager? Instance { get; private set; }

    /// <summary>All dialogue flags that persist across conversations.</summary>
    private readonly Dictionary<string, string> _flags = new();

    /// <summary>Active conversation data.</summary>
    private DialogueData? _activeData;

    /// <summary>Tracks visited line IDs within a single advance chain to detect cycles.</summary>
    private readonly HashSet<string> _visitedLines = new();

    /// <summary>Current line being displayed.</summary>
    public DialogueLine? CurrentLine { get; private set; }

    /// <summary>Whether a conversation is in progress.</summary>
    public bool IsActive => _activeData != null;

    /// <summary>Tracks whether the game was already paused before dialogue started.</summary>
    private bool _wasPausedBeforeDialogue;

    public override void _Ready()
    {
        Instance = this;
    }

    // ─── Start / End ──────────────────────────────────────────────

    /// <summary>Begin a conversation.</summary>
    public void StartConversation(DialogueData data)
    {
        if (data == null || data.Lines.Length == 0)
        {
            GD.PrintErr("DialogueManager: Tried to start empty conversation.");
            return;
        }

        _activeData = data;
        _visitedLines.Clear();
        EmitSignal(SignalName.ConversationStarted, data.ConversationId);

        // Pause player input during dialogue, but remember previous pause state.
        _wasPausedBeforeDialogue = GetTree().Paused;
        GetTree().Paused = true;
        ProcessMode = ProcessModeEnum.Always;

        ShowLine(data.GetEntryLine());
    }

    /// <summary>End the current conversation.</summary>
    public void EndConversation()
    {
        var id = _activeData?.ConversationId ?? "";
        _activeData = null;
        CurrentLine = null;
        // Restore prior pause state instead of unconditionally unpausing.
        GetTree().Paused = _wasPausedBeforeDialogue;
        EmitSignal(SignalName.ConversationEnded, id);
    }

    // ─── Advance ──────────────────────────────────────────────────

    /// <summary>
    /// Advance to the next line. Call this when the player presses the continue button.
    /// </summary>
    public void Advance()
    {
        if (_activeData == null || CurrentLine == null) return;

        // Reset cycle detection for each new advance chain.
        _visitedLines.Clear();

        if (CurrentLine.IsEnd)
        {
            EndConversation();
            return;
        }

        if (CurrentLine.HasChoices)
        {
            // Choices are pending — don't advance until SelectChoice is called.
            return;
        }

        // Move to next line.
        var next = _activeData.GetLine(CurrentLine.NextLineId);
        if (next == null)
        {
            GD.PrintErr($"DialogueManager: Missing next line '{CurrentLine.NextLineId}'");
            EndConversation();
            return;
        }

        ShowLine(next);
    }

    /// <summary>
    /// Select a choice by index (0-based). Advances to the choice's target line.
    /// </summary>
    public void SelectChoice(int choiceIndex)
    {
        if (_activeData == null || CurrentLine == null) return;
        if (choiceIndex < 0 || choiceIndex >= CurrentLine.Choices.Length) return;

        var choiceEntry = CurrentLine.Choices[choiceIndex];
        var parts = choiceEntry.Split('|');
        if (parts.Length < 2)
        {
            GD.PrintErr($"DialogueManager: Malformed choice '{choiceEntry}'. Expected 'label|nextId'.");
            EndConversation();
            return;
        }

        var targetId = parts[1].Trim();
        var next = _activeData.GetLine(targetId);
        if (next == null)
        {
            GD.PrintErr($"DialogueManager: Choice target '{targetId}' not found.");
            EndConversation();
            return;
        }

        // Record choice flag: "choice_<conversationId>_<lineId> = choiceLabel"
        SetFlag($"choice_{_activeData.ConversationId}_{CurrentLine.Id}", parts[0].Trim());

        ShowLine(next);
    }

    // ─── Display ──────────────────────────────────────────────────

    private void ShowLine(DialogueLine? line)
    {
        if (line == null)
        {
            EndConversation();
            return;
        }

        // Cycle detection: if we've already visited this line in the current
        // advance chain, stop to prevent StackOverflowException.
        if (!_visitedLines.Add(line.Id))
        {
            GD.PrintErr($"DialogueManager: Cycle detected at line '{line.Id}'. Ending conversation.");
            EndConversation();
            return;
        }

        // Check condition.
        if (!EvaluateCondition(line.Condition))
        {
            // Skip this line — go to next or end.
            if (!string.IsNullOrEmpty(line.NextLineId))
            {
                ShowLine(_activeData?.GetLine(line.NextLineId));
                return;
            }
            EndConversation();
            return;
        }

        CurrentLine = line;

        // Set flag if specified.
        if (!string.IsNullOrEmpty(line.SetFlag))
            ProcessSetFlag(line.SetFlag);

        // Fire event if specified.
        if (!string.IsNullOrEmpty(line.Event))
            EmitSignal(SignalName.DialogueEventFired, line.Event);

        // Substitute variables in text.
        string displayText = SubstituteVariables(line.Text);

        // Emit display signal.
        EmitSignal(SignalName.LineDisplayed, line.Speaker, line.PortraitKey, displayText);

        // If choices, present them.
        if (line.HasChoices)
        {
            var labels = new string[line.Choices.Length];
            for (int i = 0; i < line.Choices.Length; i++)
            {
                var parts = line.Choices[i].Split('|');
                labels[i] = parts[0].Trim();
            }
            EmitSignal(SignalName.ChoicesPresented, labels);
        }
    }

    // ─── Flags ────────────────────────────────────────────────────

    public void SetFlag(string flag, string value = "true")
    {
        _flags[flag] = value;
        EmitSignal(SignalName.FlagChanged, flag, value);
    }

    public string GetFlag(string flag)
    {
        return _flags.TryGetValue(flag, out var v) ? v : "";
    }

    public bool HasFlag(string flag)
    {
        return _flags.ContainsKey(flag) && _flags[flag] != "false";
    }

    public void ImportFlags(Dictionary<string, string> flags)
    {
        foreach (var kv in flags)
            _flags[kv.Key] = kv.Value;
    }

    public Dictionary<string, string> ExportFlags() => new(_flags);

    // ─── Conditions ───────────────────────────────────────────────

    private bool EvaluateCondition(string condition)
    {
        if (string.IsNullOrEmpty(condition)) return true;

        // "!flag_name" = flag must NOT be set.
        if (condition.StartsWith('!'))
            return !HasFlag(condition[1..]);

        // "flag_name=value" = flag must equal value.
        if (condition.Contains('='))
        {
            var parts = condition.Split('=', 2);
            return GetFlag(parts[0].Trim()) == parts[1].Trim();
        }

        // "flag_name" = flag must be set and truthy.
        return HasFlag(condition);
    }

    private void ProcessSetFlag(string setFlag)
    {
        if (setFlag.Contains('='))
        {
            var parts = setFlag.Split('=', 2);
            SetFlag(parts[0].Trim(), parts[1].Trim());
        }
        else
        {
            SetFlag(setFlag.Trim());
        }
    }

    // ─── Variable Substitution ────────────────────────────────────

    private string SubstituteVariables(string text)
    {
        // Replace {flag_name} with flag values.
        foreach (var kv in _flags)
        {
            text = text.Replace($"{{{kv.Key}}}", kv.Value);
        }

        // Built-in variables.
        text = text.Replace("{player_name}", "Vetch");
        return text;
    }
}
