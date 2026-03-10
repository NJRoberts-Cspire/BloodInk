using Godot;

namespace BloodInk.VFX;

/// <summary>
/// Hit-stop / freeze-frame effect. Briefly pauses the game engine to add
/// impact to heavy hits and stealth kills.
/// Usage: <c>HitStop.Freeze(0.08f);</c>
/// </summary>
public partial class HitStop : Node
{
    public static HitStop? Instance { get; private set; }

    private bool _frozen;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Freeze the game for the given duration in seconds.</summary>
    public void Freeze(float duration)
    {
        if (_frozen) return;
        _frozen = true;
        Engine.TimeScale = 0.05; // Near-stop instead of full stop (feels better).
        GetTree().CreateTimer(duration, true, false, true).Timeout += Unfreeze;
    }

    private void Unfreeze()
    {
        Engine.TimeScale = 1.0;
        _frozen = false;
    }

    /// <summary>Light hit-stop for normal attacks.</summary>
    public void FreezeLight() => Freeze(0.04f);

    /// <summary>Medium hit-stop for charged/heavy attacks.</summary>
    public void FreezeMedium() => Freeze(0.08f);

    /// <summary>Heavy hit-stop for stealth kills and critical moments.</summary>
    public void FreezeHeavy() => Freeze(0.14f);
}
