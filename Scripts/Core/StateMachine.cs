using Godot;

namespace BloodInk.Core;

/// <summary>
/// Base state machine. Attach as a child node of any CharacterBody2D.
/// Add State nodes as children of this node.
/// </summary>
public partial class StateMachine : Node
{
    [Export] public NodePath InitialStatePath { get; set; } = "";

    private State? _currentState;
    public State? CurrentState => _currentState;

    public override void _Ready()
    {
        // If an initial state path is set, use it; otherwise use the first child State.
        if (!string.IsNullOrEmpty(InitialStatePath))
        {
            _currentState = GetNodeOrNull<State>(InitialStatePath);
            if (_currentState == null)
                GD.PrintErr($"StateMachine: InitialStatePath '{InitialStatePath}' did not resolve to a State node.");
        }
        else
        {
            foreach (var child in GetChildren())
            {
                if (child is State state)
                {
                    _currentState = state;
                    break;
                }
            }
        }

        // Initialize all states with a reference to this machine.
        foreach (var child in GetChildren())
        {
            if (child is State state)
            {
                state.Machine = this;
                state.Init();
            }
        }

        _currentState?.Enter();
    }

    public override void _Process(double delta)
    {
        _currentState?.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsUpdate(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        _currentState?.HandleInput(@event);
    }

    public void TransitionTo(string stateName)
    {
        var nextState = GetNodeOrNull<State>(stateName);
        if (nextState == null)
        {
            GD.PrintErr($"StateMachine: State '{stateName}' not found.");
            return;
        }

        if (nextState == _currentState) return;

        _currentState?.Exit();
        _currentState = nextState;
        _currentState.Enter();
    }
}
