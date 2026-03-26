using Godot;
using BloodInk.Combat;

namespace BloodInk.Abilities;

/// <summary>
/// Stone Heart — brief invincibility and heavy damage resistance.
/// Activates on demand. Player takes 0 damage for 3 seconds.
/// Wires directly to the player's Hurtbox.IsInvincible so damage is
/// actually blocked in the combat pipeline — not just a flag query.
/// </summary>
public partial class StoneHeartAbility : AbilityBase
{
    [Export] public float Duration { get; set; } = 3f;

    private bool _isStoneForm;
    private float _stoneTimer;
    private Hurtbox? _hurtbox;

    public override void _Ready()
    {
        AbilityId = "stone_heart";
        Cooldown = 20f;
        CallDeferred(MethodName.WireHurtbox);
    }

    private void WireHurtbox()
    {
        _hurtbox = Owner2D?.GetNodeOrNull<Hurtbox>("Hurtbox");
        if (_hurtbox == null)
            GD.PrintErr("[StoneHeart] No Hurtbox child found on owner — invincibility will not apply.");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_isStoneForm)
        {
            _stoneTimer -= (float)delta;
            if (_stoneTimer <= 0f)
                EndStoneForm();
        }
    }

    protected override void Activate()
    {
        _isStoneForm = true;
        _stoneTimer = Duration;

        // Set hurtbox invincible for the stone form duration.
        if (_hurtbox != null)
        {
            _hurtbox.IFrameDuration = Duration;
            _hurtbox.StartIFrames();
        }

        // Tint player grey-blue to indicate stone form
        var sprite = Owner2D?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite != null)
            sprite.Modulate = new Color(0.6f, 0.7f, 0.9f, 1f);

        GD.Print($"[StoneHeart] Stone form active for {Duration}s — invincible.");
    }

    private void EndStoneForm()
    {
        _isStoneForm = false;

        // Ensure i-frames are cleared even if hurtbox timer drifts.
        if (_hurtbox != null)
            _hurtbox.IsInvincible = false;

        var sprite = Owner2D?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite != null)
            sprite.Modulate = Colors.White;

        GD.Print("[StoneHeart] Stone form expired.");
        ExpireAbility();
    }

    /// <summary>Whether damage should be negated (queried externally as a status check).</summary>
    public bool IsInStoneForm => _isStoneForm;
}
