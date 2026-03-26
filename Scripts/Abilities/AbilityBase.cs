using Godot;

namespace BloodInk.Abilities;

/// <summary>
/// Base class for all active tattoo abilities.
/// Attach to a Node2D owned by the player (spawned from TattooData.AbilityScenePath).
/// Each ability implements Activate() and handles its own cooldown/duration.
/// </summary>
public abstract partial class AbilityBase : Node2D
{
    [Signal] public delegate void AbilityActivatedEventHandler(string abilityId);
    [Signal] public delegate void AbilityExpiredEventHandler(string abilityId);

    [Export] public string AbilityId { get; set; } = "";
    [Export] public float Cooldown { get; set; } = 5f;

    public bool IsOnCooldown { get; private set; }
    public float CooldownRemaining { get; private set; }

    protected CharacterBody2D? Owner2D => GetParent() as CharacterBody2D;

    public override void _Process(double delta)
    {
        if (IsOnCooldown)
        {
            CooldownRemaining -= (float)delta;
            if (CooldownRemaining <= 0f)
            {
                IsOnCooldown = false;
                CooldownRemaining = 0f;
            }
        }
    }

    /// <summary>Try to activate the ability. Returns false if on cooldown.</summary>
    public bool TryActivate()
    {
        if (IsOnCooldown) return false;
        Activate();
        StartCooldown();
        EmitSignal(SignalName.AbilityActivated, AbilityId);
        return true;
    }

    /// <summary>Override in derived class to implement the ability effect.</summary>
    protected abstract void Activate();

    protected void StartCooldown()
    {
        IsOnCooldown = true;
        CooldownRemaining = Cooldown;
    }

    /// <summary>
    /// Cancel a cooldown that was started but whose effect did not actually land
    /// (e.g. ShadowStep found no destination). Resets IsOnCooldown immediately.
    /// </summary>
    protected void CancelCooldown()
    {
        IsOnCooldown = false;
        CooldownRemaining = 0f;
    }

    protected void ExpireAbility()
    {
        EmitSignal(SignalName.AbilityExpired, AbilityId);
    }
}
