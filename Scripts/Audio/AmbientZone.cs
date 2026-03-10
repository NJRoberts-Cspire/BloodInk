using Godot;

namespace BloodInk.Audio;

/// <summary>
/// An Area2D zone that plays ambient audio when the player is inside.
/// Supports crossfading in/out and optional random one-shot sounds
/// layered on top (crickets, dripping, wind gusts).
/// </summary>
public partial class AmbientZone : Area2D
{
    /// <summary>Looping ambient stream (wind, rain, cave echo, fire crackle).</summary>
    [Export] public AudioStream? AmbientLoop { get; set; }

    /// <summary>Volume in dB for the ambient loop.</summary>
    [Export] public float VolumeDb { get; set; } = -6f;

    /// <summary>Fade-in/out time when entering/leaving the zone.</summary>
    [Export] public float FadeDuration { get; set; } = 1.5f;

    /// <summary>One-shot sounds played randomly while in zone
    /// (e.g., bird calls, distant thunder).</summary>
    [Export] public AudioStream[]? RandomOneShots { get; set; }

    /// <summary>Min interval between random one-shots (seconds).</summary>
    [Export] public float OneShotMinInterval { get; set; } = 4f;

    /// <summary>Max interval between random one-shots (seconds).</summary>
    [Export] public float OneShotMaxInterval { get; set; } = 12f;

    /// <summary>Optional music track to play in this zone.</summary>
    [Export] public string MusicTrackPath { get; set; } = "";

    private AudioStreamPlayer? _loopPlayer;
    private AudioStreamPlayer? _oneShotPlayer;
    private bool _playerInside = false;
    private float _oneShotTimer;

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 1 << 1; // Player layer.
        Monitoring = true;
        Monitorable = false;

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        // Create audio players.
        _loopPlayer = new AudioStreamPlayer
        {
            Bus = "Ambient",
            VolumeDb = -80f, // Start silent, fade in.
            Stream = AmbientLoop
        };
        AddChild(_loopPlayer);

        _oneShotPlayer = new AudioStreamPlayer { Bus = "Ambient" };
        AddChild(_oneShotPlayer);

        ResetOneShotTimer();
    }

    public override void _Process(double delta)
    {
        if (!_playerInside) return;
        if (RandomOneShots == null || RandomOneShots.Length == 0) return;

        _oneShotTimer -= (float)delta;
        if (_oneShotTimer <= 0)
        {
            PlayRandomOneShot();
            ResetOneShotTimer();
        }
    }

    // ─── Zone Enter/Exit ──────────────────────────────────────────

    /// <summary>Current fade tween — killed before creating a new one.</summary>
    private Tween? _fadeTween;

    private void OnBodyEntered(Node2D body)
    {
        if (!body.IsInGroup("Player")) return;
        _playerInside = true;

        // Start loop and fade in.
        if (_loopPlayer != null && AmbientLoop != null)
        {
            if (!_loopPlayer.Playing)
                _loopPlayer.Play();

            _fadeTween?.Kill();
            _fadeTween = CreateTween();
            _fadeTween.TweenProperty(_loopPlayer, "volume_db", VolumeDb, FadeDuration);
        }

        // Switch music if specified.
        if (!string.IsNullOrEmpty(MusicTrackPath))
            AudioManager.Instance?.PlayMusic(MusicTrackPath);
    }

    private void OnBodyExited(Node2D body)
    {
        if (!body.IsInGroup("Player")) return;
        _playerInside = false;

        // Fade out loop.
        if (_loopPlayer != null && _loopPlayer.Playing)
        {
            _fadeTween?.Kill();
            _fadeTween = CreateTween();
            _fadeTween.TweenProperty(_loopPlayer, "volume_db", -80f, FadeDuration);
            _fadeTween.TweenCallback(Callable.From(() =>
            {
                if (!_playerInside) _loopPlayer.Stop();
            }));
        }
    }

    // ─── Random One-Shots ─────────────────────────────────────────

    private void PlayRandomOneShot()
    {
        if (RandomOneShots == null || RandomOneShots.Length == 0) return;
        if (_oneShotPlayer == null || _oneShotPlayer.Playing) return;

        var clip = RandomOneShots[GD.RandRange(0, RandomOneShots.Length - 1)];
        _oneShotPlayer.Stream = clip;
        _oneShotPlayer.VolumeDb = VolumeDb - 3f; // Slightly quieter than loop.
        _oneShotPlayer.Play();
    }

    private void ResetOneShotTimer()
    {
        _oneShotTimer = (float)GD.RandRange(OneShotMinInterval, OneShotMaxInterval);
    }
}
