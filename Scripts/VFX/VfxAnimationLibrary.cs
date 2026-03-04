using Godot;
using System.Collections.Generic;
using BloodInk.Tools;
using static BloodInk.Tools.SpriteSheetAnimator;

namespace BloodInk.VFX;

/// <summary>
/// Builds VFX / spell-effect animations from the same (or a separate) sprite sheet.
/// Attach to any node; it creates child <see cref="AnimatedSprite2D"/> nodes on demand
/// and pre-loads effects so they can be triggered instantly.
/// <para>
/// To fire an effect: <c>VfxLibrary.Play("vfx_purple_orb", globalPos);</c>
/// </para>
/// </summary>
public partial class VfxAnimationLibrary : Node2D
{
    [Export(PropertyHint.File, "*.png")]
    public string SheetPath { get; set; } = "res://Assets/Sprites/player_sheet.png";

    private SpriteFrames? _frames;

    // ── VFX animation table ─────────────────────────────────────────
    // These are the spell / projectile / impact effects found on the
    // RIGHT side of the sprite sheet.  Sizes and positions vary per
    // effect, so each entry has its own FrameWidth/Height and an
    // optional pixel offset.
    //
    // Tweak coordinates after dropping the actual sheet into the project.
    // ────────────────────────────────────────────────────────────────
    private static List<AnimationDef> GetVfxAnimations()
    {
        return new List<AnimationDef>
        {
            //                                    Name               Row Col Cnt  Spd  Loop   W    H    OffX  OffY
            new AnimationDef("vfx_purple_orb",       0,  4,  6,  10f, true,  48, 48, OffsetX: 256, OffsetY: 0),
            new AnimationDef("vfx_star_burst",       0,  0,  5,  12f, false, 48, 48, OffsetX: 576, OffsetY: 0),
            new AnimationDef("vfx_diamond_particles",1,  0,  5,  10f, false, 32, 32, OffsetX: 256, OffsetY: 64),
            new AnimationDef("vfx_crescent_slash",   2,  0,  5,  14f, false, 48, 48, OffsetX: 256, OffsetY: 128),
            new AnimationDef("vfx_fire_burst",       3,  0,  3,  12f, false, 64, 64, OffsetX: 256, OffsetY: 192),
            new AnimationDef("vfx_dark_energy",      3,  0,  3,  12f, false, 64, 64, OffsetX: 384, OffsetY: 192),
            new AnimationDef("vfx_energy_beam",      4,  0,  3,  10f, false,128, 64, OffsetX: 256, OffsetY: 256),
            new AnimationDef("vfx_ice_crystal",      6,  0,  4,  10f, false, 80, 80, OffsetX: 256, OffsetY: 384),
            new AnimationDef("vfx_rock_debris",      6,  0,  6,  10f, false, 48, 48, OffsetX: 480, OffsetY: 384),
            new AnimationDef("vfx_purple_claw",      7,  0,  5,  14f, false, 48, 48, OffsetX: 256, OffsetY: 448),
            new AnimationDef("vfx_arrow",            7,  0,  7,  14f, false, 32, 32, OffsetX: 512, OffsetY: 448),
            new AnimationDef("vfx_purple_shield",    8,  0,  7,  10f, true,  64, 64, OffsetX: 320, OffsetY: 512),
        };
    }

    public override void _Ready()
    {
        _frames = SpriteSheetAnimator.BuildFromPath(SheetPath, GetVfxAnimations());
        if (_frames != null)
            GD.Print($"VfxAnimationLibrary: Loaded {GetVfxAnimations().Count} VFX animations.");
    }

    /// <summary>
    /// Spawn a one-shot VFX animation at <paramref name="worldPos"/> and auto-free it
    /// when the animation finishes.
    /// </summary>
    public void Play(string animName, Vector2 worldPos)
    {
        if (_frames == null || !_frames.HasAnimation(animName))
        {
            GD.PushWarning($"VfxAnimationLibrary: Unknown animation '{animName}'.");
            return;
        }

        var sprite = new AnimatedSprite2D
        {
            SpriteFrames = _frames,
            GlobalPosition = worldPos,
            ZIndex = 10
        };
        sprite.AnimationFinished += () => sprite.QueueFree();

        GetTree().CurrentScene.AddChild(sprite);
        sprite.Play(animName);
    }

    /// <summary>
    /// Spawn a looping VFX and return the node so the caller can manage its lifetime.
    /// </summary>
    public AnimatedSprite2D? PlayLooping(string animName, Vector2 worldPos)
    {
        if (_frames == null || !_frames.HasAnimation(animName))
        {
            GD.PushWarning($"VfxAnimationLibrary: Unknown animation '{animName}'.");
            return null;
        }

        var sprite = new AnimatedSprite2D
        {
            SpriteFrames = _frames,
            GlobalPosition = worldPos,
            ZIndex = 10
        };

        GetTree().CurrentScene.AddChild(sprite);
        sprite.Play(animName);
        return sprite;
    }
}
