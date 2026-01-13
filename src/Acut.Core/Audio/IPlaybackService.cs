using System;
using System.Threading.Tasks;

namespace Acut.Core.Audio;

public interface IPlaybackService : IDisposable
{
    /// <summary>
    /// Event fired when playback position changes
    /// </summary>
    event EventHandler<TimeSpan>? PositionChanged;

    /// <summary>
    /// Event fired when playback completes
    /// </summary>
    event EventHandler? PlaybackStopped;

    /// <summary>
    /// Gets the current playback position
    /// </summary>
    TimeSpan CurrentPosition { get; }

    /// <summary>
    /// Gets the total duration
    /// </summary>
    TimeSpan TotalDuration { get; }

    /// <summary>
    /// Gets or sets the volume (0.0 to 1.0)
    /// </summary>
    float Volume { get; set; }

    /// <summary>
    /// Gets whether audio is currently playing
    /// </summary>
    bool IsPlaying { get; }

    /// <summary>
    /// Loads an audio file for playback
    /// </summary>
    Task LoadAsync(string filePath);

    /// <summary>
    /// Starts or resumes playback
    /// </summary>
    void Play();

    /// <summary>
    /// Pauses playback
    /// </summary>
    void Pause();

    /// <summary>
    /// Stops playback and resets position
    /// </summary>
    void Stop();

    /// <summary>
    /// Seeks to a specific position
    /// </summary>
    void Seek(TimeSpan position);
}
