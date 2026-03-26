using Godot;
using BloodInk.VFX;

namespace BloodInk.Combat;

/// <summary>
/// Attach to an Area2D marked as a hurtbox (receives damage).
/// Set collision mask to detect the opposing hitbox layer.
/// Emits signals when hit.
/// </summary>
public partial class Hurtbox : Area2D
{
    [Signal] public delegate void HurtEventHandler(int damage, Vector2 knockback);

    /// <summary>If true, this hurtbox is temporarily invincible (e.g. during dodge).</summary>
    [Export] public bool IsInvincible { get; set; }

    /// <summary>
    /// When set > 0, calling <see cref="StartIFrames"/> makes the hurtbox
    /// invincible for this many seconds, then automatically resets.
    /// </summary>
    [Export] public float IFrameDuration { get; set; } = 0.2f;

    private SceneTreeTimer? _iFrameTimer;

    /// <summary>
    /// Start a timed invincibility window using <see cref="IFrameDuration"/>.
    /// Calling this again while i-frames are active restarts the timer.
    /// </summary>
    public void StartIFrames()
    {
        IsInvincible = true;
        _iFrameTimer?.EmitSignal(SceneTreeTimer.SignalName.Timeout); // cancel previous if any
        _iFrameTimer = GetTree().CreateTimer(IFrameDuration);
        _iFrameTimer.Timeout += () => IsInvincible = false;
    }

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (IsInvincible) return;

        if (area is Hitbox hitbox)
        {
            // Capture owner reference once before EmitSignal, which may free the owner.
            var target = Owner as Node2D;

            var knockbackDir = Vector2.Zero;
            if (hitbox.Source != null && target != null)
            {
                knockbackDir = (target.GlobalPosition - hitbox.Source.GlobalPosition).Normalized();
            }

            var knockback = knockbackDir * hitbox.KnockbackForce.Length();
            EmitSignal(SignalName.Hurt, hitbox.ScaledDamage, knockback);
            hitbox.EmitSignal(Hitbox.SignalName.HitConnected);

            // ─── VFX Juice ────────────────────────────────────────
            // Guard against owner being freed on the same frame as EmitSignal
            // (e.g. HealthComponent.Died triggers QueueFree immediately).
            if (target == null || !IsInstanceValid(target))
                return;

            // Stealth kills are lethal but quiet — minimal VFX.
            bool isStealthKill = hitbox.IsStealthKill;
            bool isCrit = !isStealthKill && hitbox.ScaledDamage >= 3;

            // Hit flash (white blink on the damaged sprite).
            HitFlash.FlashNode(
                target.GetNodeOrNull<CanvasItem>("AnimatedSprite2D") ?? target,
                Colors.White, isStealthKill ? 0.06f : 0.12f);

            // Damage number (suppress for stealth kills — they're silent).
            if (!isStealthKill)
            {
                DamageNumber.Spawn(
                    target.GetTree().CurrentScene,
                    target.GlobalPosition + new Vector2(0, -12),
                    hitbox.ScaledDamage, isCrit);
            }

            // Blood splatter in knockback direction.
            BloodSplatter.Spawn(
                target.GetTree().CurrentScene,
                target.GlobalPosition,
                knockbackDir,
                isStealthKill ? 3 : (isCrit ? 14 : 6));

            // ─── Audio Feedback ───────────────────────────────────
            if (!isStealthKill)
            {
                Audio.AudioManager.Instance?.PlaySFX(
                    isCrit ? "res://Assets/Audio/SFX/hit_crit.wav"
                           : "res://Assets/Audio/SFX/hit_normal.wav");
            }

            // Screen shake scaled to damage (none for stealth).
            if (!isStealthKill)
            {
                if (isCrit)
                    CameraShake.Instance?.ShakeHeavy();
                else
                    CameraShake.Instance?.ShakeLight();
            }

            // Hit-stop on heavy hits (none for stealth).
            if (!isStealthKill)
            {
                if (isCrit)
                    HitStop.Instance?.FreezeHeavy();
                else if (hitbox.ScaledDamage >= 2)
                    HitStop.Instance?.FreezeLight();
            }
        }
    }
}
