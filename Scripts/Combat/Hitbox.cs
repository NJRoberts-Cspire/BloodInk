using Godot;

namespace BloodInk.Combat;

/// <summary>
/// Attach to an Area2D marked as a hitbox (deals damage).
/// Set the collision layer to PlayerHitbox (layer 4) or EnemyHitbox (layer 5).
/// </summary>
public partial class Hitbox : Area2D
{
    [Export] public int Damage { get; set; } = 1;
    [Export] public Vector2 KnockbackForce { get; set; } = new(100, 0);

    /// <summary>Owner of this hitbox, used to compute knockback direction.</summary>
    public Node2D? Source { get; set; }
}
