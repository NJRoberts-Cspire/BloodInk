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

    // Procedurally generated fallback streams, created once and reused.
    private static AudioStreamWav? _proceduralDefault;
    private static AudioStreamWav? _proceduralGrass;
    private static AudioStreamWav? _proceduralStone;
    private static AudioStreamWav? _proceduralWood;

    public override void _Ready()
    {
        _player = new AudioStreamPlayer2D { Bus = "SFX" };
        AddChild(_player);

        // Pre-generate fallback sounds if exports are not set.
        if (DefaultStep == null)
        {
            _proceduralDefault ??= GenerateThud(0.06f, 180f, 0.7f);
            _proceduralGrass    ??= GenerateThud(0.07f, 120f, 0.5f);  // Softer, lower
            _proceduralStone    ??= GenerateThud(0.04f, 260f, 0.9f);  // Sharper, higher
            _proceduralWood     ??= GenerateThud(0.05f, 200f, 0.8f);  // Mid-resonance
        }
    }

    /// <summary>
    /// Generates a short percussive thud as an AudioStreamWav.
    /// Produces a decaying burst of noise shaped by a simple exponential envelope,
    /// so footsteps are audible even when no audio assets are present.
    /// </summary>
    private static AudioStreamWav GenerateThud(float durationSeconds, float centerFreq, float amplitude)
    {
        const int sampleRate = 22050;
        int sampleCount = (int)(sampleRate * durationSeconds);
        var data = new byte[sampleCount * 2]; // 16-bit mono

        var rng = new System.Random();
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 40f);           // Fast decay
            float tone     = Mathf.Sin(2f * Mathf.Pi * centerFreq * t);
            float noise    = (float)(rng.NextDouble() * 2.0 - 1.0);
            float sample   = (tone * 0.4f + noise * 0.6f) * envelope * amplitude;

            short s = (short)Mathf.Clamp(sample * 32767f, -32768f, 32767f);
            data[i * 2]     = (byte)(s & 0xFF);
            data[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
        }

        var wav = new AudioStreamWav
        {
            Data = data,
            Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = sampleRate,
            Stereo = false,
            LoopMode = AudioStreamWav.LoopModeEnum.Disabled,
        };
        return wav;
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

        // Select sound by surface — fall back to procedural thud when no assets are loaded.
        _player.Stream = _currentSurface switch
        {
            "grass" => (AudioStream?)(GrassStep ?? DefaultStep ?? _proceduralGrass ?? _proceduralDefault),
            "stone" => StoneStep ?? DefaultStep ?? _proceduralStone ?? _proceduralDefault,
            "wood"  => WoodStep  ?? DefaultStep ?? _proceduralWood  ?? _proceduralDefault,
            _       => DefaultStep ?? _proceduralDefault,
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
