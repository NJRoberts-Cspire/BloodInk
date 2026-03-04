using Godot;
using BloodInk.Combat;
using BloodInk.VFX;

namespace BloodInk.Player;

/// <summary>
/// Main player controller. Handles movement physics and exposes references
/// used by player states.
/// </summary>
public partial class PlayerController : CharacterBody2D
{
    [ExportGroup("Movement")]
    [Export] public float MoveSpeed { get; set; } = 120f;
    [Export] public float DodgeSpeed { get; set; } = 250f;
    [Export] public float Friction { get; set; } = 600f;
    [Export] public float Acceleration { get; set; } = 800f;

    // Child node references – wired in _Ready.
    public AnimatedSprite2D AnimPlayer { get; private set; } = null!;
    public Hurtbox Hurtbox { get; private set; } = null!;
    public Hitbox SwordHitbox { get; private set; } = null!;
    public HealthComponent Health { get; private set; } = null!;
    public VfxAnimationLibrary? VfxLibrary { get; private set; }

    /// <summary>Last non-zero input direction, used for attack/dodge direction.</summary>
    public Vector2 FacingDirection { get; set; } = Vector2.Down;

    /// <summary>Knockback velocity applied externally (e.g. on hurt).</summary>
    public Vector2 KnockbackVelocity { get; set; }

    public override void _Ready()
    {
        AnimPlayer = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        Hurtbox = GetNode<Hurtbox>("Hurtbox");
        SwordHitbox = GetNode<Hitbox>("SwordHitbox");
        Health = GetNode<HealthComponent>("HealthComponent");
        VfxLibrary = GetNodeOrNull<VfxAnimationLibrary>("VfxLibrary");

        SwordHitbox.Source = this;
        SwordHitbox.Monitoring = false; // Turned on only during attack state.

        Hurtbox.Hurt += OnHurt;
        Health.Died += OnDied;
    }

    /// <summary>Returns normalized WASD / arrow-key input vector.</summary>
    public Vector2 GetInputVector()
    {
        var input = new Vector2(
            Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
            Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up")
        );
        return input.Normalized();
    }

    /// <summary>Apply acceleration toward the target velocity.</summary>
    public void ApplyMovement(Vector2 direction, float speed, double delta)
    {
        if (direction != Vector2.Zero)
        {
            Velocity = Velocity.MoveToward(direction * speed, Acceleration * (float)delta);
            FacingDirection = direction;
        }
        else
        {
            Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);
        }
    }

    /// <summary>Apply knockback then decay it.</summary>
    public void ApplyKnockback(double delta)
    {
        KnockbackVelocity = KnockbackVelocity.MoveToward(Vector2.Zero, Friction * (float)delta);
        Velocity += KnockbackVelocity;
    }

    /// <summary>Update sprite flip/animation based on facing direction.</summary>
    public void UpdateAnimation(string animName)
    {
        AnimPlayer.FlipH = FacingDirection.X < 0;
        AnimPlayer.Play(animName);
    }

    private void OnHurt(int damage, Vector2 knockback)
    {
        Health.TakeDamage(damage);
        KnockbackVelocity = knockback;

        // VFX: red screen flash + camera shake on player hit.
        VFX.ScreenTransition.Instance?.FlashRed(0.2f);
        VFX.CameraShake.Instance?.ShakeMedium();
    }

    private void OnDied()
    {
        GD.Print("Player died!");
        UpdateAnimation("death");
        SetPhysicsProcess(false);

        // VFX: dramatic death effects.
        VFX.CameraShake.Instance?.ShakeExtreme();
        VFX.HitStop.Instance?.FreezeHeavy();
        VFX.ScreenTransition.Instance?.FadeToBlack(1.5f);

        // Store the current scene for retry, then transition to Game Over.
        UI.GameOver.LastMissionScene = GetTree().CurrentScene.SceneFilePath;
        var timer = GetTree().CreateTimer(2.0f);
        timer.Timeout += () =>
        {
            GetTree().ChangeSceneToFile("res://Scenes/UI/GameOver.tscn");
        };
    }
}
