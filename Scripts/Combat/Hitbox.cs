using Godot;
using BloodInk.Abilities;

namespace BloodInk.Combat;

/// <summary>
/// Attach to an Area2D marked as a hitbox (deals damage).
/// Set the collision layer to PlayerHitbox (layer 4) or EnemyHitbox (layer 5).
/// </summary>
public partial class Hitbox : Area2D
{
    [Export] public int Damage { get; set; } = 1;
    [Export] public Vector2 KnockbackForce { get; set; } = new(100, 0);

    /// <summary>Multiplier applied to Damage on output. Set by abilities such as BloodRage.</summary>
    public float DamageMultiplier { get; set; } = 1.0f;

    /// <summary>Damage after applying DamageMultiplier. Use this value when dealing damage.</summary>
    public int ScaledDamage => Mathf.RoundToInt(Damage * DamageMultiplier);

    /// <summary>Owner of this hitbox, used to compute knockback direction.</summary>
    public Node2D? Source { get; set; }

    /// <summary>If true, this hit is a stealth kill — suppress excessive VFX.</summary>
    public bool IsStealthKill { get; set; } = false;

    /// <summary>
    /// Emitted by <see cref="Hurtbox"/> immediately after this hitbox's damage signal fires.
    /// Consumed here to break any active MaskOfAsh disguise on the attacker.
    /// </summary>
    [Signal] public delegate void HitConnectedEventHandler();

    public override void _Ready()
    {
        HitConnected += OnHitConnected;
    }

    /// <summary>
    /// Called when a hit from this hitbox has landed. Breaks the MaskOfAsh disguise
    /// on <see cref="Source"/> if one is currently active, because attacking reveals
    /// the player's identity.
    /// </summary>
    private void OnHitConnected()
    {
        var mask = Source?.GetNodeOrNull<MaskOfAshAbility>("MaskOfAshAbility");
        if (mask != null && mask.IsMasked)
            mask.BreakMask();
    }
}
