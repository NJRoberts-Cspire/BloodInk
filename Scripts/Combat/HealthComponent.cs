using Godot;

namespace BloodInk.Combat;

/// <summary>
/// Health component. Attach to any entity that can take damage.
/// </summary>
public partial class HealthComponent : Node
{
    [Signal] public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
    [Signal] public delegate void DamageTakenEventHandler(int amount);
    [Signal] public delegate void DiedEventHandler();

    [Export] public int MaxHealth { get; set; } = 5;

    private int _currentHealth;
    public int CurrentHealth
    {
        get => _currentHealth;
        private set
        {
            _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
            EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);
        }
    }

    public bool IsDead => CurrentHealth <= 0;

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        if (amount < 0) amount = 0;
        CurrentHealth -= amount;
        EmitSignal(SignalName.DamageTaken, amount);
        if (IsDead)
        {
            EmitSignal(SignalName.Died);
        }
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        if (amount < 0) amount = 0;
        CurrentHealth += amount;
    }

    public void FullHeal()
    {
        // Intentionally no IsDead guard — FullHeal is used for revival/respawn.
        CurrentHealth = MaxHealth;
    }
}
