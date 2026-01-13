using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Acut.Core.Audio;

public class PlaybackService : IPlaybackService
{
    private IWavePlayer? _waveOut;
    private AudioFileReader? _audioFileReader;
    private Timer? _positionTimer;
    private bool _isPlaying;
    private Process? _ffplayProcess;
    private string? _currentFilePath;
    private TimeSpan _totalDuration;
    private TimeSpan _currentPosition;
    private DateTime _playbackStartTime;
    private TimeSpan _playbackStartPosition;
    private readonly bool _isMacOS;

    public event EventHandler<TimeSpan>? PositionChanged;
    public event EventHandler? PlaybackStopped;

    public PlaybackService()
    {
        _isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }

    public TimeSpan CurrentPosition
    {
        get
        {
            if (_isMacOS)
            {
                if (_isPlaying)
                {
                    // Calculate position based on elapsed time since play started
                    var elapsed = DateTime.Now - _playbackStartTime;
                    _currentPosition = _playbackStartPosition + elapsed;

                    // Don't exceed total duration
                    if (_currentPosition > _totalDuration)
                        _currentPosition = _totalDuration;
                }
                return _currentPosition;
            }
            else
            {
                return _audioFileReader?.CurrentTime ?? TimeSpan.Zero;
            }
        }
    }

    public TimeSpan TotalDuration => _isMacOS ? _totalDuration : (_audioFileReader?.TotalTime ?? TimeSpan.Zero);

    public float Volume { get; set; } = 0.5f;

    public bool IsPlaying => _isPlaying;

    public async Task LoadAsync(string filePath)
    {
        await Task.Run(() =>
        {
            Stop();

            _currentFilePath = filePath;

            if (_isMacOS)
            {
                // For macOS, we'll store the file path but not start playback yet
                // ffplay will be used when Play() is called
                // We still need to get duration from FFProbe
                try
                {
                    var mediaInfo = FFMpegCore.FFProbe.Analyse(filePath);
                    _totalDuration = mediaInfo.Duration;
                }
                catch
                {
                    _totalDuration = TimeSpan.Zero;
                }
            }
            else
            {
                // Windows/Linux: Use NAudio
                _audioFileReader = new AudioFileReader(filePath);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _waveOut = new WaveOutEvent();
                }
                else
                {
                    _waveOut = new WaveOutEvent(); // ALSA on Linux
                }

                _waveOut.Init(_audioFileReader);

                _waveOut.PlaybackStopped += (s, e) =>
                {
                    _isPlaying = false;
                    _positionTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    PlaybackStopped?.Invoke(this, EventArgs.Empty);
                };
            }

            // Set up position update timer
            _positionTimer?.Dispose();
            _positionTimer = new Timer(_ =>
            {
                PositionChanged?.Invoke(this, CurrentPosition);
            }, null, Timeout.Infinite, Timeout.Infinite);
        });
    }

    public void Play()
    {
        if (_isMacOS)
        {
            // macOS: Use ffplay for playback
            if (string.IsNullOrEmpty(_currentFilePath))
                return;

            // Track playback start time for position calculation
            _playbackStartTime = DateTime.Now;
            _playbackStartPosition = _currentPosition;
            _isPlaying = true;

            // Start ffplay in background (hidden window, auto-exit)
            _ffplayProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffplay",
                    Arguments = $"-nodisp -autoexit -volume {(int)(Volume * 100)} \"{_currentFilePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            _ffplayProcess.EnableRaisingEvents = true;
            _ffplayProcess.Exited += (s, e) =>
            {
                _isPlaying = false;
                _positionTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                PlaybackStopped?.Invoke(this, EventArgs.Empty);
            };

            try
            {
                _ffplayProcess.Start();
                _positionTimer?.Change(0, 100);
            }
            catch
            {
                _isPlaying = false;
            }
        }
        else
        {
            // Windows/Linux: Use NAudio
            if (_waveOut == null || _audioFileReader == null)
                return;

            _waveOut.Play();
            _isPlaying = true;
            _positionTimer?.Change(0, 100);
        }
    }

    public void Pause()
    {
        if (_isMacOS)
        {
            // macOS: ffplay doesn't support pause well, so we stop instead
            Stop();
        }
        else
        {
            if (_waveOut == null)
                return;

            _waveOut.Pause();
            _isPlaying = false;
            _positionTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }

    public void Stop()
    {
        _isPlaying = false;
        _positionTimer?.Change(Timeout.Infinite, Timeout.Infinite);

        if (_isMacOS)
        {
            // Reset position
            _currentPosition = TimeSpan.Zero;

            // macOS: Kill ffplay process
            if (_ffplayProcess != null && !_ffplayProcess.HasExited)
            {
                try
                {
                    _ffplayProcess.Kill();
                    _ffplayProcess.WaitForExit(1000);
                }
                catch { }
                finally
                {
                    _ffplayProcess?.Dispose();
                    _ffplayProcess = null;
                }
            }
        }
        else
        {
            // Windows/Linux: Use NAudio
            if (_waveOut != null)
            {
                _waveOut.Stop();
            }

            if (_audioFileReader != null)
            {
                _audioFileReader.Position = 0;
            }
        }
    }

    public void Seek(TimeSpan position)
    {
        if (_isMacOS)
        {
            // macOS: ffplay doesn't support seeking in our current implementation
            // Would need to restart with -ss parameter
            // For now, do nothing
            return;
        }
        else
        {
            if (_audioFileReader == null)
                return;

            _audioFileReader.CurrentTime = position;
            PositionChanged?.Invoke(this, CurrentPosition);
        }
    }

    public void Dispose()
    {
        Stop();
        _positionTimer?.Dispose();
        _waveOut?.Dispose();
        _audioFileReader?.Dispose();
        _ffplayProcess?.Dispose();
    }
}
