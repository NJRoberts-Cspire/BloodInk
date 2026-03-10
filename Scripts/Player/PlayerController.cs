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
        AnimPlayer = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D")!;
        Hurtbox = GetNodeOrNull<Hurtbox>("Hurtbox")!;
        SwordHitbox = GetNodeOrNull<Hitbox>("SwordHitbox")!;
        Health = GetNodeOrNull<HealthComponent>("HealthComponent")!;
        VfxLibrary = GetNodeOrNull<VfxAnimationLibrary>("VfxLibrary");

        if (AnimPlayer == null || Hurtbox == null || SwordHitbox == null || Health == null)
        {
            GD.PrintErr($"PlayerController '{Name}': missing required child nodes (AnimatedSprite2D, Hurtbox, SwordHitbox, or HealthComponent).");
            SetPhysicsProcess(false);
            return;
        }

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
        if (AnimPlayer.SpriteFrames != null && AnimPlayer.SpriteFrames.HasAnimation(animName))
            AnimPlayer.Play(animName);
        else if (AnimPlayer.SpriteFrames != null && AnimPlayer.SpriteFrames.HasAnimation("idle"))
            AnimPlayer.Play("idle");
    }

    /// <summary>Post-hit invincibility duration in seconds.</summary>
    private const float InvincibilityDuration = 0.6f;

    private void OnHurt(int damage, Vector2 knockback)
    {
        Health.TakeDamage(damage);
        KnockbackVelocity = knockback;

        // Post-hit invincibility frames to prevent multi-hit shredding.
        Hurtbox.IsInvincible = true;
        var iTimer = GetTree().CreateTimer(InvincibilityDuration);
        iTimer.Timeout += () => { if (IsInsideTree()) Hurtbox.IsInvincible = false; };

        // VFX: red screen flash + camera shake on player hit.
        VFX.ScreenTransition.Instance?.FlashRed(0.2f);
        VFX.CameraShake.Instance?.ShakeMedium();
    }

    private void OnDied()
    {
        GD.Print("Player died!");
        UpdateAnimation("death");
        SetPhysicsProcess(false);
        SetProcessUnhandledInput(false);

        // Disable the state machine so the player can't attack/dodge while dead.
        var sm = GetNodeOrNull<Core.StateMachine>("StateMachine");
        if (sm != null) sm.ProcessMode = ProcessModeEnum.Disabled;

        // Prevent further hits and disable sword.
        Hurtbox.IsInvincible = true;
        SwordHitbox.Monitoring = false;

        // VFX: dramatic death effects.
        VFX.CameraShake.Instance?.ShakeExtreme();
        VFX.HitStop.Instance?.FreezeHeavy();
        VFX.ScreenTransition.Instance?.FadeToBlack(1.5f);

        // Ensure game is unpaused (e.g. if player dies during dialogue).
        Core.GameManager.Instance?.SetPaused(false);

        // Store the current scene for retry, then transition to Game Over.
        UI.GameOver.LastMissionScene = GetTree().CurrentScene?.SceneFilePath ?? "";
        // processAlways: true ensures the timer ticks even if something re-pauses.
        var timer = GetTree().CreateTimer(2.0f, true, false, true);
        timer.Timeout += () =>
        {
            GetTree().ChangeSceneToFile("res://Scenes/UI/GameOver.tscn");
        };
    }
}
