using Godot;
using BloodInk.BloodEchoes;

namespace BloodInk.Content;

/// <summary>
/// Blood Echo definitions — memory fragments absorbed from
/// Edictbearer blood-ink. Each is a short playable flashback.
/// </summary>
public static class EchoRegistry
{
    public static EchoData EchoCowl() => new()
    {
        Id          = "echo_cowl",
        DisplayName = "The Governor's Garden",
        Description = "Relive Lord Cowl's last peaceful morning — breakfast "
                    + "on the sunlit balcony, watching the wheat fields he built "
                    + "on orc graves. He never looked down.",
        EdictbearerName = "Lord Harlan Cowl",
        KingdomIndex   = 0,
        ScenePath      = "res://Scenes/Echoes/echo_cowl.tscn",
        Genre          = EchoGenre.WalkingSim,
        EstimatedMinutes = 15,
        UnlockedByTattooId = "shadow_step",
        IntelUnlocked  = new[] { "cowl_ancestry", "shaman_bargain_hint" },
        WhisperText    = "I never saw the dark as empty. I thought it was full of things that loved me."
    };

    public static EchoData EchoKeelan() => new()
    {
        Id          = "echo_keelan",
        DisplayName = "The Undrowned's Keel",
        Description = "Relive Admiral Keelan's first dive — lowered into black "
                    + "water alongside the orcs he would later drown by the hundreds. "
                    + "He was afraid, once.",
        EdictbearerName = "Admiral Voss Keelan",
        KingdomIndex   = 1,
        ScenePath      = "res://Scenes/Echoes/echo_keelan.tscn",
        Genre          = EchoGenre.WalkingSim,
        EstimatedMinutes = 20,
        UnlockedByTattooId = "blood_rage",
        IntelUnlocked  = new[] { "pearl_focus_origin", "edict_formula_hint" },
        WhisperText    = "They sink so quietly. Like they were always meant to be down there."
    };

    public static EchoData EchoMyre() => new()
    {
        Id          = "echo_myre",
        DisplayName = "Specimen Zero",
        Description = "Relive Provost Myre's first dissection. She was twenty-two. "
                    + "The subject was alive. Her hands didn't shake.",
        EdictbearerName = "Provost Ilian Myre",
        KingdomIndex   = 2,
        ScenePath      = "res://Scenes/Echoes/echo_myre.tscn",
        Genre          = EchoGenre.Puzzle,
        EstimatedMinutes = 20,
        UnlockedByTattooId = "wall_cling",
        IntelUnlocked  = new[] { "edict_formula_full", "ashroot_tradition_match" },
        WhisperText    = "I catalogued every sound they made. For science."
    };

    public static EchoData EchoAshford() => new()
    {
        Id          = "echo_ashford",
        DisplayName = "First Blood",
        Description = "Relive Huntmaster Ashford's first orc hunt at age fourteen. "
                    + "Her mother handed her the bow. The prey begged in a language "
                    + "she almost understood.",
        EdictbearerName = "Huntmaster General Brielle Ashford",
        KingdomIndex   = 3,
        ScenePath      = "res://Scenes/Echoes/echo_ashford.tscn",
        Genre          = EchoGenre.Stealth,
        EstimatedMinutes = 25,
        UnlockedByTattooId = "enemy_sense",
        IntelUnlocked  = new[] { "needlewise_physical_desc", "shaman_meeting_record" },
        WhisperText    = "The best prey is the kind that almost gets away."
    };

    public static EchoData EchoMorvain() => new()
    {
        Id          = "echo_morvain",
        DisplayName = "The Sermon of Ash",
        Description = "Relive the Ashen Father's ordination. He knelt in volcanic ash "
                    + "and prayed until his knees bled. He heard a voice. "
                    + "It might have been his own.",
        EdictbearerName = "Ashen Father Morvain",
        KingdomIndex   = 4,
        ScenePath      = "res://Scenes/Echoes/echo_morvain.tscn",
        Genre          = EchoGenre.WalkingSim,
        EstimatedMinutes = 15,
        UnlockedByTattooId = "stone_heart",
        IntelUnlocked  = new[] { "edict_pact_document", "orc_signature_revealed" },
        WhisperText    = "For the children who will never have to fight."
    };

    public static EchoData EchoAccord() => new()
    {
        Id          = "echo_accord",
        DisplayName = "The Unanimous Vote",
        Description = "Relive the founding of the Council of Scales — six diplomats "
                    + "voting to distribute the Edict's anchor. They were afraid of "
                    + "one person holding that much power. They were right.",
        EdictbearerName = "The Council of Scales",
        KingdomIndex   = 5,
        ScenePath      = "res://Scenes/Echoes/echo_accord.tscn",
        Genre          = EchoGenre.Dialogue,
        EstimatedMinutes = 20,
        UnlockedByTattooId = "mask_of_ash",
        IntelUnlocked  = new[] { "needlewise_identity_confirmed", "tattoo_contingency" },
        WhisperText    = "Neutrality is a mask. We all wear one."
    };

    public static EchoData[] GetAll() => new[]
    {
        EchoCowl(), EchoKeelan(), EchoMyre(),
        EchoAshford(), EchoMorvain(), EchoAccord()
    };
}
