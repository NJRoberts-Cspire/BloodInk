using Godot;

namespace BloodInk.Audio;

/// <summary>
/// Plays footstep sounds based on player movement and surface type.
/// Attach as a child of the player. Listens to velocity and crouching state.
/// </summary>
public partial class FootstepPlayer : Node
{
    /// <summary>Default footstep sound.</summary>
    [Export] public AudioStream? DefaultStep { get; set; }

    /// <summary>Footstep on grass/dirt.</summary>
    [Export] public AudioStream? GrassStep { get; set; }

    /// <summary>Footstep on stone.</summary>
    [Export] public AudioStream? StoneStep { get; set; }

    /// <summary>Footstep on wood.</summary>
    [Export] public AudioStream? WoodStep { get; set; }

    /// <summary>Base interval between steps at normal walk speed.</summary>
    [Export] public float BaseInterval { get; set; } = 0.35f;

    /// <summary>Volume at normal walk speed (dB).</summary>
    [Export] public float BaseVolumeDb { get; set; } = -10f;

    /// <summary>Volume reduction when crouching (dB).</summary>
    [Export] public float CrouchVolumeReduction { get; set; } = -12f;

    private AudioStreamPlayer2D? _player;
    private float _stepTimer;
    private string _currentSurface = "default";

    public override void _Ready()
    {
        _player = new AudioStreamPlayer2D { Bus = "SFX" };
        AddChild(_player);
    }

    public override void _PhysicsProcess(double delta)
    {
        var parent = GetParent<CharacterBody2D>();
        if (parent == null) return;

        float speed = parent.Velocity.Length();
        if (speed < 10f)
        {
            _stepTimer = 0;
            return;
        }

        // Adjust interval by speed.
        float interval = BaseInterval * (120f / Mathf.Max(speed, 30f));

        _stepTimer += (float)delta;
        if (_stepTimer >= interval)
        {
            _stepTimer = 0;
            PlayStep(speed, parent);
        }
    }

    private void PlayStep(float speed, CharacterBody2D parent)
    {
        if (_player == null) return;

        // Select sound by surface.
        _player.Stream = _currentSurface switch
        {
            "grass" => GrassStep ?? DefaultStep,
            "stone" => StoneStep ?? DefaultStep,
            "wood"  => WoodStep ?? DefaultStep,
            _       => DefaultStep
        };

        if (_player.Stream == null) return;

        // Volume varies by speed and crouch state.
        float vol = BaseVolumeDb;
        var stealth = parent.GetNodeOrNull<Stealth.StealthProfile>("StealthProfile");
        if (stealth?.IsCrouching == true)
            vol += CrouchVolumeReduction;

        // Running = louder.
        if (speed > 100f)
            vol += 4f;

        _player.VolumeDb = vol;
        _player.PitchScale = (float)GD.RandRange(0.9, 1.1); // Slight variation.
        _player.Play();
    }

    /// <summary>Set the current surface type (called by floor tiles/zones).</summary>
    public void SetSurface(string surfaceType)
    {
        _currentSurface = surfaceType;
    }
}
