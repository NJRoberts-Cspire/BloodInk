using Godot;
using System.Collections.Generic;

namespace BloodInk.Audio;

/// <summary>
/// Centralized audio manager. Plays music, SFX, and ambient sounds
/// via named audio buses. Supports crossfading, layered music,
/// and positional audio helpers.
/// </summary>
public partial class AudioManager : Node
{
    [Signal] public delegate void MusicChangedEventHandler(string trackName);

    public static AudioManager? Instance { get; private set; }

    // ─── Bus indices (cached) ─────────────────────────────────────
    private int _masterBus;
    private int _musicBus;
    private int _sfxBus;
    private int _ambientBus;
    private int _uiBus;

    // ─── Music state ──────────────────────────────────────────────
    private AudioStreamPlayer? _musicPlayer;
    private AudioStreamPlayer? _musicFadeOut;
    private string _currentMusicTrack = "";

    // ─── SFX pool ─────────────────────────────────────────────────
    private readonly List<AudioStreamPlayer> _sfxPool = new();
    private const int SfxPoolSize = 8;

    // ─── Volume defaults (linear, 0-1) ────────────────────────────
    private float _masterVolume = 1f;
    private float _musicVolume = 0.7f;
    private float _sfxVolume = 0.8f;
    private float _ambientVolume = 0.6f;
    private float _uiVolume = 0.9f;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always; // Audio persists during pause.

        // Ensure audio buses exist — create them if needed.
        EnsureBus("Music");
        EnsureBus("SFX");
        EnsureBus("Ambient");
        EnsureBus("UI");

        _masterBus = AudioServer.GetBusIndex("Master");
        _musicBus = AudioServer.GetBusIndex("Music");
        _sfxBus = AudioServer.GetBusIndex("SFX");
        _ambientBus = AudioServer.GetBusIndex("Ambient");
        _uiBus = AudioServer.GetBusIndex("UI");

        // Create music players.
        _musicPlayer = new AudioStreamPlayer { Bus = "Music" };
        _musicFadeOut = new AudioStreamPlayer { Bus = "Music" };
        AddChild(_musicPlayer);
        AddChild(_musicFadeOut);

        // Pre-allocate SFX pool.
        for (int i = 0; i < SfxPoolSize; i++)
        {
            var player = new AudioStreamPlayer { Bus = "SFX" };
            AddChild(player);
            _sfxPool.Add(player);
        }

        ApplyVolumes();
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    // ─── Music ────────────────────────────────────────────────────

    /// <summary>Play a music track with optional crossfade.</summary>
    public void PlayMusic(string resourcePath, float crossfadeDuration = 1f)
    {
        if (_currentMusicTrack == resourcePath) return;

        var stream = GD.Load<AudioStream>(resourcePath);
        if (stream == null)
        {
            GD.PrintErr($"AudioManager: Music not found: {resourcePath}");
            return;
        }

        _currentMusicTrack = resourcePath;

        if (_musicPlayer!.Playing && crossfadeDuration > 0)
        {
            // Crossfade: move current to fade-out, start new on main.
            _musicFadeOut!.Stream = _musicPlayer.Stream;
            _musicFadeOut.VolumeDb = _musicPlayer.VolumeDb;
            _musicFadeOut.Play(_musicPlayer.GetPlaybackPosition());
            _musicPlayer.Stop();

            var tween = CreateTween();
            tween.TweenProperty(_musicFadeOut, "volume_db", -40f, crossfadeDuration);
            tween.TweenCallback(Callable.From(() => _musicFadeOut.Stop()));
        }

        _musicPlayer.Stream = stream;
        _musicPlayer.VolumeDb = 0f;
        _musicPlayer.Play();

        EmitSignal(SignalName.MusicChanged, resourcePath);
    }

    /// <summary>Stop music with optional fade-out.</summary>
    public void StopMusic(float fadeDuration = 1f)
    {
        if (!_musicPlayer!.Playing) return;

        if (fadeDuration > 0)
        {
            var tween = CreateTween();
            tween.TweenProperty(_musicPlayer, "volume_db", -40f, fadeDuration);
            tween.TweenCallback(Callable.From(() =>
            {
                _musicPlayer.Stop();
                _currentMusicTrack = "";
            }));
        }
        else
        {
            _musicPlayer.Stop();
            _currentMusicTrack = "";
        }
    }

    // ─── SFX ──────────────────────────────────────────────────────

    /// <summary>Play a one-shot SFX from the pool.</summary>
    public void PlaySFX(string resourcePath, float volumeDb = 0f)
    {
        var stream = GD.Load<AudioStream>(resourcePath);
        if (stream == null) return;

        var player = GetFreeSFXPlayer();
        if (player == null) return;

        player.Stream = stream;
        player.VolumeDb = volumeDb;
        player.Play();
    }

    /// <summary>Play a one-shot SFX from an already-loaded stream.</summary>
    public void PlaySFX(AudioStream stream, float volumeDb = 0f)
    {
        if (stream == null) return;
        var player = GetFreeSFXPlayer();
        if (player == null) return;

        player.Stream = stream;
        player.VolumeDb = volumeDb;
        player.Play();
    }

    private AudioStreamPlayer? GetFreeSFXPlayer()
    {
        foreach (var p in _sfxPool)
        {
            if (!p.Playing) return p;
        }
        // All busy — steal the oldest.
        _sfxPool[0].Stop();
        return _sfxPool[0];
    }

    // ─── UI Sounds ────────────────────────────────────────────────

    /// <summary>Play a UI sound effect (button clicks, menu transitions).</summary>
    public void PlayUI(string resourcePath, float volumeDb = 0f)
    {
        var stream = GD.Load<AudioStream>(resourcePath);
        if (stream == null) return;

        // Limit concurrent UI sounds to prevent unbounded node creation.
        int uiCount = 0;
        foreach (var child in GetChildren())
        {
            if (child is AudioStreamPlayer asp && asp.Bus == "UI" && asp.Playing)
                uiCount++;
        }
        if (uiCount >= 4) return;

        // Use a temporary player for UI sounds.
        var player = new AudioStreamPlayer { Bus = "UI", Stream = stream, VolumeDb = volumeDb };
        AddChild(player);
        player.Play();
        player.Finished += () => player.QueueFree();
    }

    // ─── Volume Control ───────────────────────────────────────────

    public void SetMasterVolume(float linear)
    {
        _masterVolume = Mathf.Clamp(linear, 0f, 1f);
        ApplyVolumes();
    }

    public void SetMusicVolume(float linear)
    {
        _musicVolume = Mathf.Clamp(linear, 0f, 1f);
        ApplyVolumes();
    }

    public void SetSFXVolume(float linear)
    {
        _sfxVolume = Mathf.Clamp(linear, 0f, 1f);
        ApplyVolumes();
    }

    public void SetAmbientVolume(float linear)
    {
        _ambientVolume = Mathf.Clamp(linear, 0f, 1f);
        ApplyVolumes();
    }

    public void SetUIVolume(float linear)
    {
        _uiVolume = Mathf.Clamp(linear, 0f, 1f);
        ApplyVolumes();
    }

    public float GetMasterVolume() => _masterVolume;
    public float GetMusicVolume() => _musicVolume;
    public float GetSFXVolume() => _sfxVolume;
    public float GetAmbientVolume() => _ambientVolume;

    private void ApplyVolumes()
    {
        SetBusVolume(_masterBus, _masterVolume);
        SetBusVolume(_musicBus, _musicVolume);
        SetBusVolume(_sfxBus, _sfxVolume);
        SetBusVolume(_ambientBus, _ambientVolume);
        SetBusVolume(_uiBus, _uiVolume);
    }

    private static void SetBusVolume(int busIdx, float linear)
    {
        if (busIdx < 0) return;
        // Convert linear (0-1) to decibels. Mute below threshold.
        float db = linear > 0.001f ? Mathf.LinearToDb(linear) : -80f;
        AudioServer.SetBusVolumeDb(busIdx, db);
        AudioServer.SetBusMute(busIdx, linear <= 0.001f);
    }

    // ─── Bus Management ───────────────────────────────────────────

    private static void EnsureBus(string busName)
    {
        if (AudioServer.GetBusIndex(busName) >= 0) return;
        int idx = AudioServer.BusCount;
        AudioServer.AddBus(idx);
        AudioServer.SetBusName(idx, busName);
        AudioServer.SetBusSend(idx, "Master");
    }
}
