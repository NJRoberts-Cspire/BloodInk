using Godot;

namespace BloodInk.VFX;

/// <summary>
/// Flash a CanvasItem white (or any tint) on damage.
/// Works by modulating the sprite, then tweening back.
/// Attach to the entity that gets hit, or call statically.
/// </summary>
public partial class HitFlash : Node
{
    /// <summary>Flash color — pure white for a damage flash.</summary>
    [Export] public Color FlashColor { get; set; } = Colors.White;

    /// <summary>Flash duration in seconds.</summary>
    [Export] public float FlashDuration { get; set; } = 0.12f;

    /// <summary>Number of flashes (for multi-blink on low health).</summary>
    [Export] public int FlashCount { get; set; } = 1;

    private CanvasItem? _target;
    private Color _originalModulate;
    private bool _flashing;

    public override void _Ready()
    {
        _target = GetParent() as CanvasItem;
        if (_target != null)
            _originalModulate = _target.Modulate;
    }

    /// <summary>Trigger the flash effect.</summary>
    public void Flash()
    {
        if (_target == null || _flashing) return;
        _flashing = true;

        var tween = CreateTween();
        for (int i = 0; i < FlashCount; i++)
        {
            tween.TweenProperty(_target, "modulate", FlashColor, FlashDuration * 0.3f);
            tween.TweenProperty(_target, "modulate", _originalModulate, FlashDuration * 0.7f);
        }
        tween.TweenCallback(Callable.From(() => _flashing = false));
    }

    /// <summary>Flash with a custom color (red for critical, green for heal).</summary>
    public void FlashTint(Color color)
    {
        if (_target == null || _flashing) return;
        _flashing = true;

        var tween = CreateTween();
        tween.TweenProperty(_target, "modulate", color, FlashDuration * 0.3f);
        tween.TweenProperty(_target, "modulate", _originalModulate, FlashDuration * 0.7f);
        tween.TweenCallback(Callable.From(() => _flashing = false));
    }

    // ─── Static Utility ───────────────────────────────────────────

    /// <summary>Flash any CanvasItem without needing a HitFlash component.</summary>
    public static void FlashNode(CanvasItem node, Color color, float duration = 0.12f)
    {
        if (node == null) return;
        var original = node.Modulate;

        var tween = node.CreateTween();
        tween.TweenProperty(node, "modulate", color, duration * 0.3f);
        tween.TweenProperty(node, "modulate", original, duration * 0.7f);
    }
}
