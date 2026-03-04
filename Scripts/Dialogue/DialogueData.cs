using Godot;
using System.Collections.Generic;
using System.Linq;

namespace BloodInk.Dialogue;

/// <summary>
/// A conversation — an ordered collection of DialogueLines forming a dialogue tree.
/// Created as a .tres resource or built in code.
/// </summary>
[GlobalClass]
public partial class DialogueData : Resource
{
    /// <summary>Unique conversation identifier (e.g., "rukh_act1_briefing").</summary>
    [Export] public string ConversationId { get; set; } = "";

    /// <summary>All lines in this conversation.</summary>
    [Export] public DialogueLine[] Lines { get; set; } = System.Array.Empty<DialogueLine>();

    // ─── Runtime lookup ───────────────────────────────────────────
    private Dictionary<string, DialogueLine>? _lineMap;

    /// <summary>Build the lookup table (lazy init).</summary>
    private void EnsureMap()
    {
        if (_lineMap != null) return;
        _lineMap = new Dictionary<string, DialogueLine>();
        foreach (var line in Lines)
        {
            if (!string.IsNullOrEmpty(line.Id))
                _lineMap[line.Id] = line;
        }
    }

    /// <summary>Get a line by ID.</summary>
    public DialogueLine? GetLine(string id)
    {
        EnsureMap();
        return _lineMap!.TryGetValue(id, out var line) ? line : null;
    }

    /// <summary>Get the entry line (first line marked IsEntry, or first line).</summary>
    public DialogueLine? GetEntryLine()
    {
        EnsureMap();
        return Lines.FirstOrDefault(l => l.IsEntry) ?? Lines.FirstOrDefault();
    }
}
