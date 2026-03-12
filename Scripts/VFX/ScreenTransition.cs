using Godot;

namespace BloodInk.VFX;

/// <summary>
/// Manages screen transitions (fade to black, fade to white, etc.).
/// Uses the FadeLayer CanvasLayer autoload.
/// </summary>
public partial class ScreenTransition : Node
{
    public static ScreenTransition? Instance { get; private set; }

    private ColorRect? _fadeRect;
    private Tween? _activeTween;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        // Try to find the FadeLayer autoload's ColorRect.
        var fadeLayer = GetTree().Root.GetNodeOrNull("FadeLayer");
        _fadeRect = fadeLayer?.GetNodeOrNull<ColorRect>("ColorRect");
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Fade to black over the given duration.</summary>
    public void FadeToBlack(float duration = 0.5f)
    {
        FadeTo(Colors.Black, duration);
    }

    /// <summary>Fade from black to clear.</summary>
    public void FadeFromBlack(float duration = 0.5f)
    {
        FadeFrom(Colors.Black, duration);
    }

    /// <summary>Fade to white (for death/flash effects).</summary>
    public void FadeToWhite(float duration = 0.3f)
    {
        FadeTo(Colors.White, duration);
    }

    /// <summary>Flash white briefly (e.g., stealth kill emphasis).</summary>
    public void FlashWhite(float duration = 0.15f)
    {
        if (_fadeRect == null) return;

        _activeTween?.Kill();
        _fadeRect.Color = new Color(1, 1, 1, 0.6f);
        _activeTween = _fadeRect.CreateTween();
        _activeTween.TweenProperty(_fadeRect, "color:a", 0.0f, duration);
    }

    /// <summary>Red pulse (player takes damage).</summary>
    public void FlashRed(float duration = 0.2f)
    {
        if (_fadeRect == null) return;

        _activeTween?.Kill();
        _fadeRect.Color = new Color(0.7f, 0.05f, 0.05f, 0.3f);
        _activeTween = _fadeRect.CreateTween();
        _activeTween.TweenProperty(_fadeRect, "color:a", 0.0f, duration);
    }

    // ─── Internal ─────────────────────────────────────────────────

    private void FadeTo(Color color, float duration)
    {
        if (_fadeRect == null) return;

        _activeTween?.Kill();
        _fadeRect.Color = new Color(color.R, color.G, color.B, 0);
        _activeTween = _fadeRect.CreateTween();
        _activeTween.TweenProperty(_fadeRect, "color:a", 1.0f, duration);
    }

    private void FadeFrom(Color color, float duration)
    {
        if (_fadeRect == null) return;

        _activeTween?.Kill();
        _fadeRect.Color = new Color(color.R, color.G, color.B, 1);
        _activeTween = _fadeRect.CreateTween();
        _activeTween.TweenProperty(_fadeRect, "color:a", 0.0f, duration);
    }
}
