using Godot;

namespace BloodInk.Abilities;

/// <summary>
/// Blood Rage — when wounded, briefly surge attack speed and damage.
/// Auto-activates when health drops below 50%. Manual re-activation on cooldown.
/// </summary>
public partial class BloodRageAbility : AbilityBase
{
    [Export] public float DamageMult { get; set; } = 2.0f;
    [Export] public float Duration { get; set; } = 4f;

    private bool _isRaging;
    private float _rageTimer;

    public override void _Ready()
    {
        AbilityId = "blood_rage";
        Cooldown = 12f;

        // Wire to player health to auto-trigger
        CallDeferred(MethodName.WireHealthTrigger);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_isRaging)
        {
            _rageTimer -= (float)delta;
            if (_rageTimer <= 0f)
                EndRage();
        }
    }

    private void WireHealthTrigger()
    {
        var health = Owner2D?.GetNodeOrNull<Combat.HealthComponent>("HealthComponent");
        if (health != null)
            health.HealthChanged += OnHealthChanged;
    }

    public override void _ExitTree()
    {
        // Disconnect to prevent duplicate connections if the ability node is re-parented or re-added.
        var health = Owner2D?.GetNodeOrNull<Combat.HealthComponent>("HealthComponent");
        if (health != null && health.IsConnected(Combat.HealthComponent.SignalName.HealthChanged, Callable.From<int, int>(OnHealthChanged)))
            health.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int current, int max)
    {
        if (!_isRaging && !IsOnCooldown && current <= max / 2)
            TryActivate();
    }

    protected override void Activate()
    {
        _isRaging = true;
        _rageTimer = Duration;

        var hitbox = Owner2D?.GetNodeOrNull<Combat.Hitbox>("Hitbox");
        if (hitbox != null)
            hitbox.DamageMultiplier = DamageMult;

        GD.Print($"[BloodRage] RAGE ACTIVE for {Duration}s — damage ×{DamageMult}");
    }

    private void EndRage()
    {
        _isRaging = false;

        var hitbox = Owner2D?.GetNodeOrNull<Combat.Hitbox>("Hitbox");
        if (hitbox != null)
            hitbox.DamageMultiplier = 1.0f;

        GD.Print("[BloodRage] Rage expired.");
        ExpireAbility();
    }

    /// <summary>Whether Blood Rage is currently active (for damage multiplier queries).</summary>
    public bool IsRaging => _isRaging;
}
