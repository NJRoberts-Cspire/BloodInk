using Godot;
using BloodInk.Ink;

namespace BloodInk.Content;

/// <summary>
/// Static factory that creates all tattoo definitions in code.
/// In a full pipeline these would be .tres resource files — this serves as
/// the canonical registry until the editor pipeline is ready.
/// </summary>
public static class TattooRegistry
{
    // ═══════════════════════════════════════════════════════════════
    //  SHADOW MARKS  (Arms — stealth)
    // ═══════════════════════════════════════════════════════════════

    public static TattooData ShadowStep() => new()
    {
        Id              = "shadow_step",
        DisplayName     = "Shadow Step",
        Description     = "Short-range teleport between shadows. The ink remembers darkness.",
        Slot            = TattooSlot.Arms_Shadow,
        RequiredGrade   = InkGrade.Major,
        InkCost         = 1,
        RequiredKingdomIndex = 0,
        PrimaryTemperament   = InkTemperament.Shadow,
        TemperamentWeight    = 20,
        StealthBonus         = 0.15f,
        GrantsActiveAbility  = true,
        AbilityScenePath     = "res://Scenes/Abilities/ShadowStep.tscn",
        WhisperText          = "\"I never saw the dark as empty. It was always full of things waiting.\" — Lord Cowl's last thought",
        BloodEchoId          = "echo_cowl"
    };

    public static TattooData SilentSole() => new()
    {
        Id              = "silent_sole",
        DisplayName     = "Silent Sole",
        Description     = "Footstep noise reduced to near silence. Walk on graves without waking the dead.",
        Slot            = TattooSlot.Arms_Shadow,
        RequiredGrade   = InkGrade.Lesser,
        InkCost         = 1,
        PrimaryTemperament   = InkTemperament.Shadow,
        TemperamentWeight    = 10,
        StealthBonus         = 0.1f,
        DetectionRadiusModifier = -0.3f,
        WhisperText          = "\"The dogs didn't bark. That's what I remember.\" — Reeve Maren"
    };

    public static TattooData SmokeSkin() => new()
    {
        Id              = "smoke_skin",
        DisplayName     = "Smoke Skin",
        Description     = "Visibility drops in dim light. Your edges blur like a half-remembered face.",
        Slot            = TattooSlot.Arms_Shadow,
        RequiredGrade   = InkGrade.Lesser,
        InkCost         = 1,
        PrimaryTemperament   = InkTemperament.Shadow,
        TemperamentWeight    = 8,
        StealthBonus         = 0.2f,
        WhisperText          = "\"There was something in the wheat. I think.\" — Greenhold farmhand"
    };

    // ═══════════════════════════════════════════════════════════════
    //  FANG LINES  (Chest — combat)
    // ═══════════════════════════════════════════════════════════════

    public static TattooData IronTuskBite() => new()
    {
        Id              = "irontusk_bite",
        DisplayName     = "Irontusk Bite",
        Description     = "Blade strikes hit harder. The old war-songs hum through your tendons.",
        Slot            = TattooSlot.Chest_Fang,
        RequiredGrade   = InkGrade.Lesser,
        InkCost         = 1,
        PrimaryTemperament   = InkTemperament.Fang,
        TemperamentWeight    = 15,
        DamageBonus          = 0.25f,
        WhisperText          = "\"He went down swinging. They all do.\" — Captain Thorne"
    };

    public static TattooData BloodRage() => new()
    {
        Id              = "blood_rage",
        DisplayName     = "Blood Rage",
        Description     = "Attack speed surges when wounded. Pain becomes fuel.",
        Slot            = TattooSlot.Chest_Fang,
        RequiredGrade   = InkGrade.Major,
        InkCost         = 1,
        RequiredKingdomIndex = 1,
        PrimaryTemperament   = InkTemperament.Fang,
        TemperamentWeight    = 25,
        DamageBonus          = 0.15f,
        SpeedBonus           = 0.2f,
        GrantsActiveAbility  = true,
        AbilityScenePath     = "res://Scenes/Abilities/BloodRage.tscn",
        WhisperText          = "\"They don't die like people. They die like storms.\" — Admiral Keelan",
        BloodEchoId          = "echo_keelan"
    };

    public static TattooData CounterMark() => new()
    {
        Id              = "counter_mark",
        DisplayName     = "Counter Mark",
        Description     = "Parry window doubled. Your muscles read the swing before your eyes do.",
        Slot            = TattooSlot.Chest_Fang,
        RequiredGrade   = InkGrade.Lesser,
        InkCost         = 1,
        PrimaryTemperament   = InkTemperament.Fang,
        TemperamentWeight    = 10,
        DamageBonus          = 0.1f,
        WhisperText          = "\"Trained twelve years. They learn in an afternoon.\" — Ranger Captain Fenn"
    };

    // ═══════════════════════════════════════════════════════════════
    //  VEIN SCRIPTS  (Legs — movement)
    // ═══════════════════════════════════════════════════════════════

    public static TattooData AshSprint() => new()
    {
        Id              = "ash_sprint",
        DisplayName     = "Ash Sprint",
        Description     = "Sprint speed increased. The ground blurs. You leave ash-prints.",
        Slot            = TattooSlot.Legs_Vein,
        RequiredGrade   = InkGrade.Lesser,
        InkCost         = 1,
        PrimaryTemperament   = InkTemperament.Bone,
        TemperamentWeight    = 8,
        SpeedBonus           = 0.2f,
        WhisperText          = "\"They can run. When it matters.\" — Divemaster Grell"
    };

    public static TattooData WallCling() => new()
    {
        Id              = "wall_cling",
        DisplayName     = "Wall Cling",
        Description     = "Briefly grip vertical surfaces. Fingers dig into stone like roots.",
        Slot            = TattooSlot.Legs_Vein,
        RequiredGrade   = InkGrade.Major,
        InkCost         = 1,
        RequiredKingdomIndex = 2,
        PrimaryTemperament   = InkTemperament.Bone,
        TemperamentWeight    = 15,
        SpeedBonus           = 0.1f,
        GrantsActiveAbility  = true,
        AbilityScenePath     = "res://Scenes/Abilities/WallCling.tscn",
        WhisperText          = "\"Specimen 44 climbed the glass. Impossible. I have the footage.\" — Provost Myre",
        BloodEchoId          = "echo_myre"
    };

    // ═══════════════════════════════════════════════════════════════
    //  SKULL WARDS  (Head — perception)
    // ═══════════════════════════════════════════════════════════════

    public static TattooData DarkVision() => new()
    {
        Id              = "dark_vision",
        DisplayName     = "Dark Vision",
        Description     = "See in total darkness. Your eyes drink the last photon and stretch it.",
        Slot            = TattooSlot.Head_Skull,
        RequiredGrade   = InkGrade.Lesser,
        InkCost         = 1,
        PrimaryTemperament   = InkTemperament.Shadow,
        TemperamentWeight    = 10,
        StealthBonus         = 0.05f,
        WhisperText          = "\"They're watching us. I can feel it.\" — Lensmaster Obel"
    };

    public static TattooData TrapSense() => new()
    {
        Id              = "trap_sense",
        DisplayName     = "Trap Sense",
        Description     = "Trip wires, pressure plates, and arcane wards shimmer in your vision.",
        Slot            = TattooSlot.Head_Skull,
        RequiredGrade   = InkGrade.Lesser,
        InkCost         = 1,
        PrimaryTemperament   = InkTemperament.Bone,
        TemperamentWeight    = 12,
        TrapEffectivenessBonus = 0.3f,
        WhisperText          = "\"The forest is my trap. I just set the teeth.\" — Huntmaster Ashford"
    };

    public static TattooData EnemySense() => new()
    {
        Id              = "enemy_sense",
        DisplayName     = "Enemy Sense",
        Description     = "Nearby hostile intent registers as pressure in your skull. Limited range.",
        Slot            = TattooSlot.Head_Skull,
        RequiredGrade   = InkGrade.Major,
        InkCost         = 1,
        RequiredKingdomIndex = 3,
        PrimaryTemperament   = InkTemperament.Shadow,
        TemperamentWeight    = 18,
        DetectionRadiusModifier = 0.25f,
        GrantsActiveAbility  = true,
        AbilityScenePath     = "res://Scenes/Abilities/EnemySense.tscn",
        WhisperText          = "\"I can smell them. Always could. It's a gift.\" — Huntmaster Ashford",
        BloodEchoId          = "echo_ashford"
    };

    // ═══════════════════════════════════════════════════════════════
    //  SPINE CHAINS  (Back — resistance)
    // ═══════════════════════════════════════════════════════════════

    public static TattooData EdictResist() => new()
    {
        Id              = "edict_resist",
        DisplayName     = "Edict Resist",
        Description     = "The curse's grip weakens. Your bones stop aching for the first time in years.",
        Slot            = TattooSlot.Back_Spine,
        RequiredGrade   = InkGrade.Lesser,
        InkCost         = 1,
        PrimaryTemperament   = InkTemperament.Root,
        TemperamentWeight    = 10,
        ResistanceBonus      = 0.15f,
        HealthBonus          = 0.1f,
        WhisperText          = "\"The Edict is mercy. Without it they'd tear themselves apart.\" — The Ashen Father"
    };

    public static TattooData StoneHeart() => new()
    {
        Id              = "stone_heart",
        DisplayName     = "Stone Heart",
        Description     = "Damage resistance up. Your chest tightens — not from pain, but from weight.",
        Slot            = TattooSlot.Back_Spine,
        RequiredGrade   = InkGrade.Major,
        InkCost         = 1,
        RequiredKingdomIndex = 4,
        PrimaryTemperament   = InkTemperament.Root,
        TemperamentWeight    = 20,
        ResistanceBonus      = 0.3f,
        HealthBonus          = 0.25f,
        GrantsActiveAbility  = true,
        AbilityScenePath     = "res://Scenes/Abilities/StoneHeart.tscn",
        WhisperText          = "\"My faith makes me unbreakable. What makes YOU endure?\" — Ashen Father Morvain",
        BloodEchoId          = "echo_morvain"
    };

    // ═══════════════════════════════════════════════════════════════
    //  WHISPER RINGS  (Hands — interaction)
    // ═══════════════════════════════════════════════════════════════

    public static TattooData FingerWork() => new()
    {
        Id              = "finger_work",
        DisplayName     = "Finger Work",
        Description     = "Lockpicking and pickpocketing improved. Your fingers find seams that don't exist.",
        Slot            = TattooSlot.Hands_Whisper,
        RequiredGrade   = InkGrade.Lesser,
        InkCost         = 1,
        PrimaryTemperament   = InkTemperament.Shadow,
        TemperamentWeight    = 8,
        StealthBonus         = 0.05f,
        WhisperText          = "\"Check the locks again.\" — Harbormaster Lenn"
    };

    public static TattooData MaskOfAsh() => new()
    {
        Id              = "mask_of_ash",
        DisplayName     = "Mask of Ash",
        Description     = "Disguise duration doubled. Your face shifts — not quite human, but close enough.",
        Slot            = TattooSlot.Hands_Whisper,
        RequiredGrade   = InkGrade.Major,
        InkCost         = 1,
        RequiredKingdomIndex = 5,
        PrimaryTemperament   = InkTemperament.Shadow,
        TemperamentWeight    = 22,
        StealthBonus         = 0.2f,
        GrantsActiveAbility  = true,
        AbilityScenePath     = "res://Scenes/Abilities/MaskOfAsh.tscn",
        WhisperText          = "\"Neutrality is a mask. We all wear one.\" — The Gray Warden",
        BloodEchoId          = "echo_accord"
    };

    public static TattooData MercyTouch() => new()
    {
        Id              = "mercy_touch",
        DisplayName     = "Mercy Touch",
        Description     = "Non-lethal takedowns available. The ink remembers how to hold back.",
        Slot            = TattooSlot.Hands_Whisper,
        RequiredGrade   = InkGrade.Lesser,
        InkCost         = 1,
        PrimaryTemperament   = InkTemperament.Root,
        TemperamentWeight    = 15,
        HealingBonus         = 0.1f,
        WhisperText          = "\"She hid the children. Even ours.\" — Dame Cowl's secret ledger"
    };

    // ═══════════════════════════════════════════════════════════════
    //  REGISTRY ACCESS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns all defined tattoos. This is the single source of truth
    /// for tattoo data until .tres resources are hooked up.
    /// </summary>
    public static TattooData[] GetAll() => new[]
    {
        // Shadow
        ShadowStep(), SilentSole(), SmokeSkin(),
        // Fang
        IronTuskBite(), BloodRage(), CounterMark(),
        // Vein
        AshSprint(), WallCling(),
        // Skull
        DarkVision(), TrapSense(), EnemySense(),
        // Spine
        EdictResist(), StoneHeart(),
        // Whisper
        FingerWork(), MaskOfAsh(), MercyTouch()
    };

    private static System.Collections.Generic.Dictionary<string, Ink.TattooData>? _byId;

    /// <summary>Find a tattoo by its ID. Returns null if not found.</summary>
    public static Ink.TattooData? FindById(string id)
    {
        if (_byId == null)
        {
            _byId = new();
            foreach (var t in GetAll())
                _byId[t.Id] = t;
        }
        return _byId.TryGetValue(id, out var result) ? result : null;
    }
}
