using Godot;

namespace BloodInk.Abilities;

/// <summary>
/// Mask of Ash — assume a disguise that fools guards for a limited time.
/// Guards will not recognize Vetch as a threat until the mask breaks
/// (attacked, sprinted near guard, or duration expires).
/// </summary>
public partial class MaskOfAshAbility : AbilityBase
{
    [Export] public float Duration { get; set; } = 30f;

    private bool _isMasked;
    private float _maskTimer;

    public override void _Ready()
    {
        AbilityId = "mask_of_ash";
        Cooldown = 45f;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_isMasked)
        {
            _maskTimer -= (float)delta;
            if (_maskTimer <= 0f)
                RemoveMask();
        }
    }

    protected override void Activate()
    {
        _isMasked = true;
        _maskTimer = Duration;

        // Tint player to look like a guard / civilian
        var sprite = Owner2D?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite != null)
            sprite.Modulate = new Color(0.8f, 0.75f, 0.6f, 0.9f);

        GD.Print($"[MaskOfAsh] Disguise active for {Duration}s.");
    }

    /// <summary>Break the disguise early (attack, alarm, etc.).</summary>
    public void BreakMask()
    {
        if (!_isMasked) return;
        GD.Print("[MaskOfAsh] Disguise broken!");
        RemoveMask();
    }

    private void RemoveMask()
    {
        _isMasked = false;

        var sprite = Owner2D?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite != null)
            sprite.Modulate = Colors.White;

        GD.Print("[MaskOfAsh] Mask fades.");
        ExpireAbility();
    }

    /// <summary>Whether the disguise is currently active (queried by DetectionSensor).</summary>
    public bool IsMasked => _isMasked;
}
