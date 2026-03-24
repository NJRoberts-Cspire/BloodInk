using Godot;

namespace BloodInk.Abilities;

/// <summary>
/// Wall Cling — grip vertical surfaces briefly to hang and observe.
/// Player presses Jump while moving into a wall. Holds for up to 3 seconds.
/// </summary>
public partial class WallClingAbility : AbilityBase
{
    [Export] public float ClingSustain { get; set; } = 3f;

    private bool _isClung;
    private float _clingTimer;

    public override void _Ready()
    {
        AbilityId = "wall_cling";
        Cooldown = 8f;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_isClung)
        {
            _clingTimer -= (float)delta;
            if (_clingTimer <= 0f)
                ReleaseWall();

            // Suppress gravity while clinging — player controller reads IsClung
        }
    }

    protected override void Activate()
    {
        _isClung = true;
        _clingTimer = ClingSustain;
        GD.Print($"[WallCling] Clinging to wall for up to {ClingSustain}s.");
    }

    public void ReleaseWall()
    {
        if (!_isClung) return;
        _isClung = false;
        GD.Print("[WallCling] Released.");
        ExpireAbility();
    }

    /// <summary>Whether the player is currently wall-clinging (gravity suppressed).</summary>
    public bool IsClung => _isClung;
}
