using Godot;

namespace BloodInk.Core;

/// <summary>
/// Base class for all states. Override the virtual methods as needed.
/// </summary>
public partial class State : Node
{
    public StateMachine? Machine { get; set; }

    /// <summary>Called once after _Ready, before any state is entered.</summary>
    public virtual void Init() { }

    /// <summary>Called when this state becomes active.</summary>
    public virtual void Enter() { }

    /// <summary>Called when leaving this state.</summary>
    public virtual void Exit() { }

    /// <summary>Called every _Process frame while active.</summary>
    public virtual void Update(double delta) { }

    /// <summary>Called every _PhysicsProcess frame while active.</summary>
    public virtual void PhysicsUpdate(double delta) { }

    /// <summary>Called for unhandled input while active.</summary>
    public virtual void HandleInput(InputEvent @event) { }
}
