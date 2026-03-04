using Godot;

namespace BloodInk.Dialogue;

/// <summary>
/// A single line of dialogue — who speaks, what they say, and what
/// comes next (choice branches, conditionals, or just the next line).
/// </summary>
[GlobalClass]
public partial class DialogueLine : Resource
{
    /// <summary>Unique ID for this line (used by branches and conditions).</summary>
    [Export] public string Id { get; set; } = "";

    /// <summary>Speaker name displayed in the dialogue box. Empty = narrator.</summary>
    [Export] public string Speaker { get; set; } = "";

    /// <summary>Portrait key for the speaker (used to load the correct sprite).</summary>
    [Export] public string PortraitKey { get; set; } = "";

    /// <summary>The actual text the character says. Supports {variable} substitution.</summary>
    [Export(PropertyHint.MultilineText)]
    public string Text { get; set; } = "";

    /// <summary>
    /// If non-empty, this line presents choices. Each entry is "choiceLabel|nextLineId".
    /// Example: "Spare him|spare_wrack" or "Kill him|kill_wrack".
    /// </summary>
    [Export] public string[] Choices { get; set; } = System.Array.Empty<string>();

    /// <summary>
    /// The ID of the next line to show (ignored if Choices is non-empty).
    /// Empty = end of conversation.
    /// </summary>
    [Export] public string NextLineId { get; set; } = "";

    /// <summary>
    /// Condition to check before showing this line.
    /// Format: "flag_name" (flag must be true) or "!flag_name" (must be false).
    /// Empty = always shown.
    /// </summary>
    [Export] public string Condition { get; set; } = "";

    /// <summary>
    /// Flag to set when this line is reached.
    /// Format: "flag_name" or "flag_name=value".
    /// </summary>
    [Export] public string SetFlag { get; set; } = "";

    /// <summary>
    /// Signal/event to fire when this line is reached.
    /// Used for triggering gameplay events mid-dialogue (e.g., "give_item:key_goldmanor").
    /// </summary>
    [Export] public string Event { get; set; } = "";

    /// <summary>Whether this is the start line of a conversation.</summary>
    [Export] public bool IsEntry { get; set; } = false;

    // ─── Helpers ──────────────────────────────────────────────────

    public bool HasChoices => Choices.Length > 0;
    public bool IsEnd => !HasChoices && string.IsNullOrEmpty(NextLineId);
}
