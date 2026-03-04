using Godot;
using System.Collections.Generic;

namespace BloodInk.Tools;

/// <summary>
/// Utility class for slicing a sprite sheet into <see cref="SpriteFrames"/> animations.
/// Each animation is defined by a row, column range, frame size, speed, and loop flag.
/// Works at runtime (called from _Ready) or from [Tool] scripts in the editor.
/// </summary>
public static class SpriteSheetAnimator
{
    /// <summary>Describes one animation extracted from a sprite sheet.</summary>
    public record AnimationDef(
        string Name,
        int Row,
        int StartCol,
        int FrameCount,
        float Speed = 8f,
        bool Loop = true,
        int FrameWidth = 64,
        int FrameHeight = 64,
        int OffsetX = 0,
        int OffsetY = 0
    );

    /// <summary>
    /// Build a <see cref="SpriteFrames"/> resource from a texture and a list of animation
    /// definitions.  Each definition maps a named animation to a horizontal strip of frames
    /// inside the sheet.
    /// </summary>
    public static SpriteFrames Build(Texture2D sheet, IEnumerable<AnimationDef> animations)
    {
        var frames = new SpriteFrames();

        // Remove the auto-created "default" animation.
        if (frames.HasAnimation("default"))
            frames.RemoveAnimation("default");

        foreach (var anim in animations)
        {
            frames.AddAnimation(anim.Name);
            frames.SetAnimationSpeed(anim.Name, anim.Speed);
            frames.SetAnimationLoop(anim.Name, anim.Loop);

            for (int i = 0; i < anim.FrameCount; i++)
            {
                var atlas = new AtlasTexture
                {
                    Atlas = sheet,
                    Region = new Rect2(
                        anim.OffsetX + (anim.StartCol + i) * anim.FrameWidth,
                        anim.OffsetY + anim.Row * anim.FrameHeight,
                        anim.FrameWidth,
                        anim.FrameHeight
                    ),
                    FilterClip = true
                };
                frames.AddFrame(anim.Name, atlas);
            }
        }

        return frames;
    }

    /// <summary>
    /// Convenience: load a texture from a resource path and build the SpriteFrames.
    /// Returns null if the texture cannot be loaded.
    /// </summary>
    public static SpriteFrames? BuildFromPath(string texturePath, IEnumerable<AnimationDef> animations)
    {
        var tex = GD.Load<Texture2D>(texturePath);
        if (tex == null)
        {
            GD.PushWarning($"SpriteSheetAnimator: Could not load texture at '{texturePath}'");
            return null;
        }
        return Build(tex, animations);
    }
}
