using Godot;
using System.Collections.Generic;
using BloodInk.Tools;
using static BloodInk.Tools.SpriteSheetAnimator;

namespace BloodInk.Player;

/// <summary>
/// Auto-configures the player's <see cref="AnimatedSprite2D"/> from the sprite sheet.
/// Attach as a child of the Player node. On _Ready it slices the sheet into animations
/// and assigns the resulting SpriteFrames.
/// <para>
/// The sprite sheet is expected at <c>res://Assets/Sprites/player_sheet.png</c>.
/// Adjust <see cref="FrameW"/> / <see cref="FrameH"/> and the animation table if
/// your sheet uses a different layout.
/// </para>
/// </summary>
[Tool]
public partial class PlayerAnimationSetup : Node
{
    /// <summary>Path to the player sprite sheet texture.</summary>
    [Export(PropertyHint.File, "*.png")]
    public string SheetPath { get; set; } = "res://Assets/Sprites/player_sheet.png";

    /// <summary>Width of a single character frame in pixels.</summary>
    [Export] public int FrameW { get; set; } = 64;

    /// <summary>Height of a single character frame in pixels.</summary>
    [Export] public int FrameH { get; set; } = 64;

    /// <summary>
    /// If true the setup runs every time in _Ready (useful while iterating on layout).
    /// Set to false once the animations look correct so it only builds on first load.
    /// </summary>
    [Export] public bool RebuildEveryRun { get; set; } = true;

    /// <summary>Editor-only trigger: toggle this in the Inspector to rebuild now.</summary>
    [Export]
    public bool EditorRebuild
    {
        get => false;
        set { if (value) BuildAndApply(); }
    }

    public override void _Ready()
    {
        if (RebuildEveryRun || Engine.IsEditorHint())
            BuildAndApply();
    }

    /// <summary>Builds the SpriteFrames and applies them to the sibling AnimatedSprite2D.</summary>
    public void BuildAndApply()
    {
        var sprite = GetParentOrNull<Node>()?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite == null)
        {
            GD.PushWarning("PlayerAnimationSetup: No sibling AnimatedSprite2D found.");
            return;
        }

        var anims = GetPlayerAnimations();
        var frames = SpriteSheetAnimator.BuildFromPath(SheetPath, anims);
        if (frames != null)
        {
            sprite.SpriteFrames = frames;
            sprite.Animation = "idle";
            sprite.Play();
            GD.Print($"PlayerAnimationSetup: Built {anims.Count} animations from '{SheetPath}'.");
        }
        else
        {
            // Fallback to placeholder sprites when sheet doesn't exist.
            var placeholder = PlaceholderSprites.GetFrames("player_frames");
            if (placeholder != null)
            {
                sprite.SpriteFrames = placeholder;
                sprite.Animation = "idle";
                sprite.Play();
                GD.Print("PlayerAnimationSetup: Using placeholder sprites (no sheet found).");
            }
            else
            {
                GD.PushWarning("PlayerAnimationSetup: No sprite sheet and no placeholders available.");
            }
        }
    }

    // ── Animation table ─────────────────────────────────────────────
    // Adjust rows, columns, and frame counts to match your sheet.
    // The sprite sheet layout (each row = one animation strip):
    //
    //  Row 0: Idle            6 frames
    //  Row 1: Hurt / Hit      4 frames
    //  Row 2: Attack 1        6 frames   (sword swing)
    //  Row 3: Attack Heavy    6 frames   (overhead / energy strike)
    //  Row 4: Thrust          5 frames   (forward lunge)
    //  Row 5: Dodge / Roll    4 frames
    //  Row 6: Air / Jump Atk  6 frames
    //  Row 7: Cast            6 frames   (spell cast pose)
    //  Row 8: Staff Attack    6 frames
    //  Row 9: Run             8 frames
    // Row 10: Death           6 frames
    //
    // If your sheet differs, just edit the numbers below.
    // ────────────────────────────────────────────────────────────────
    private List<AnimationDef> GetPlayerAnimations()
    {
        int w = FrameW, h = FrameH;
        return new List<AnimationDef>
        {
            //                         Name           Row  Col  Cnt   Spd   Loop   W  H
            new AnimationDef("idle",           0,   0,   6,   8f,  true,  w, h),
            new AnimationDef("hurt",           1,   0,   4,  10f,  false, w, h),
            new AnimationDef("attack",         2,   0,   6,  14f,  false, w, h),
            new AnimationDef("attack_heavy",   3,   0,   6,  12f,  false, w, h),
            new AnimationDef("thrust",         4,   0,   5,  14f,  false, w, h),
            new AnimationDef("dodge",          5,   0,   4,  14f,  false, w, h),
            new AnimationDef("air_attack",     6,   0,   6,  14f,  false, w, h),
            new AnimationDef("cast",           7,   0,   6,  10f,  false, w, h),
            new AnimationDef("staff_attack",   8,   0,   6,  14f,  false, w, h),
            new AnimationDef("run",            9,   0,   8,  10f,  true,  w, h),
            new AnimationDef("death",         10,   0,   6,   8f,  false, w, h),
        };
    }
}
