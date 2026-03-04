using Godot;

namespace BloodInk.BloodEchoes;

/// <summary>
/// Resource defining a Blood Echo: a playable memory from an Edictbearer's past.
/// Each Echo is a self-contained scene with its own controls/genre.
/// </summary>
[GlobalClass]
public partial class EchoData : Resource
{
    [Export] public string Id { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";

    /// <summary>The Edictbearer whose memory this echo represents.</summary>
    [Export] public string EdictbearerName { get; set; } = "";

    /// <summary>Kingdom index (0-5) this echo belongs to.</summary>
    [Export] public int KingdomIndex { get; set; }

    /// <summary>Path to the echo's playable scene.</summary>
    [Export(PropertyHint.File, "*.tscn")] public string ScenePath { get; set; } = "";

    /// <summary>Genre tag describing the echo's gameplay style.</summary>
    [Export] public EchoGenre Genre { get; set; }

    /// <summary>Estimated play time in minutes.</summary>
    [Export] public int EstimatedMinutes { get; set; } = 20;

    /// <summary>Intel fragments revealed after completing this echo.</summary>
    [Export] public string[] IntelUnlocked { get; set; } = [];

    /// <summary>Target weaknesses revealed (affect subsequent playthroughs).</summary>
    [Export] public string[] WeaknessesRevealed { get; set; } = [];

    /// <summary>Tattoo ID that unlocks this echo (must be a Major grade tattoo).</summary>
    [Export] public string UnlockedByTattooId { get; set; } = "";

    /// <summary>Intro narration text shown before the echo begins.</summary>
    [Export(PropertyHint.MultilineText)] public string IntroNarration { get; set; } = "";

    /// <summary>The "whisper" — the Edictbearer fragment that speaks as the echo begins.</summary>
    [Export(PropertyHint.MultilineText)] public string WhisperText { get; set; } = "";
}

/// <summary>
/// Genre types for Blood Echoes. Each echo plays differently.
/// </summary>
public enum EchoGenre
{
    /// <summary>Walking simulator — exploration and observation. No fail state.</summary>
    WalkingSim,

    /// <summary>Hunting/tracking — follow trails, identify targets.</summary>
    Hunting,

    /// <summary>Naval/tactical — command a ship or unit.</summary>
    Tactical,

    /// <summary>Puzzle — solve environmental or logic puzzles.</summary>
    Puzzle,

    /// <summary>Dialogue — conversation trees with no escape. Every option leads to the same end.</summary>
    Dialogue,

    /// <summary>Stealth — play as the Edictbearer witnessing orc operations.</summary>
    Stealth
}
