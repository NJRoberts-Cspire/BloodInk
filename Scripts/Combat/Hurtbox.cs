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

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (IsInvincible) return;

        if (area is Hitbox hitbox)
        {
            var knockbackDir = Vector2.Zero;
            if (hitbox.Source != null && Owner is Node2D self)
            {
                knockbackDir = (self.GlobalPosition - hitbox.Source.GlobalPosition).Normalized();
            }

            var knockback = knockbackDir * hitbox.KnockbackForce.Length();
            EmitSignal(SignalName.Hurt, hitbox.Damage, knockback);

            // ─── VFX Juice ────────────────────────────────────────
            if (Owner is Node2D target)
            {
                bool isCrit = hitbox.Damage >= 100;

                // Hit flash (white blink on the damaged sprite).
                HitFlash.FlashNode(
                    target.GetNodeOrNull<CanvasItem>("AnimatedSprite2D") ?? target,
                    Colors.White, 0.12f);

                // Damage number.
                DamageNumber.Spawn(
                    target.GetTree().CurrentScene,
                    target.GlobalPosition + new Vector2(0, -12),
                    hitbox.Damage, isCrit);

                // Blood splatter in knockback direction.
                BloodSplatter.Spawn(
                    target.GetTree().CurrentScene,
                    target.GlobalPosition,
                    knockbackDir,
                    isCrit ? 14 : 6);

                // Screen shake scaled to damage.
                if (isCrit)
                    CameraShake.Instance?.ShakeHeavy();
                else
                    CameraShake.Instance?.ShakeLight();

                // Hit-stop on heavy hits.
                if (isCrit)
                    HitStop.Instance?.FreezeHeavy();
                else if (hitbox.Damage >= 2)
                    HitStop.Instance?.FreezeLight();
            }
        }
    }
}
