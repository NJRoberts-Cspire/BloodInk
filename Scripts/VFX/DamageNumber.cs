using Godot;

namespace BloodInk.VFX;

/// <summary>
/// Floating damage number that rises and fades out.
/// Spawn via <c>DamageNumber.Spawn(position, amount)</c>.
/// </summary>
public partial class DamageNumber : Label
{
    /// <summary>How far the number drifts upward (pixels).</summary>
    private const float RiseDistance = 24f;

    /// <summary>Total lifetime in seconds.</summary>
    private const float Lifetime = 0.8f;

    /// <summary>Horizontal scatter range (pixels).</summary>
    private const float ScatterX = 8f;

    public override void _Ready()
    {
        // Prevent interaction.
        MouseFilter = MouseFilterEnum.Ignore;
        ZIndex = 20;

        // Tween: rise + fade + scale pop.
        float scatter = (float)GD.RandRange(-ScatterX, ScatterX);
        var startPos = Position;
        var endPos = startPos + new Vector2(scatter, -RiseDistance);

        // Scale pop: start slightly larger then normalize.
        Scale = new Vector2(1.4f, 1.4f);

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(this, "position", endPos, Lifetime)
             .SetEase(Tween.EaseType.Out)
             .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(this, "modulate:a", 0.0f, Lifetime * 0.6f)
             .SetDelay(Lifetime * 0.4f);
        tween.TweenProperty(this, "scale", Vector2.One, Lifetime * 0.3f)
             .SetEase(Tween.EaseType.Out);
        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(() => QueueFree()));
    }

    // ─── Factory ──────────────────────────────────────────────────

    /// <summary>Spawn a floating damage number at a world position.</summary>
    public static void Spawn(Node parent, Vector2 worldPos, int amount, bool isCritical = false)
    {
        var label = new DamageNumber
        {
            Text = amount.ToString(),
            Position = worldPos,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        // Style by damage type.
        if (isCritical || amount >= 100)
        {
            // Stealth kill / crit — big red text.
            label.AddThemeColorOverride("font_color", new Color(1f, 0.2f, 0.15f));
            label.AddThemeFontSizeOverride("font_size", 14);
            label.Text = amount >= 999 ? "KILL" : amount.ToString();
        }
        else if (amount <= 0)
        {
            // Blocked / immune.
            label.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            label.Text = "Block";
        }
        else
        {
            // Normal damage — white/yellow.
            label.AddThemeColorOverride("font_color", new Color(1f, 0.95f, 0.7f));
            label.AddThemeFontSizeOverride("font_size", 10);
        }

        parent.AddChild(label);
    }

    /// <summary>Spawn a healing number (green, rises).</summary>
    public static void SpawnHeal(Node parent, Vector2 worldPos, int amount)
    {
        var label = new DamageNumber
        {
            Text = $"+{amount}",
            Position = worldPos,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        label.AddThemeColorOverride("font_color", new Color(0.3f, 0.9f, 0.3f));
        label.AddThemeFontSizeOverride("font_size", 10);
        parent.AddChild(label);
    }
}
