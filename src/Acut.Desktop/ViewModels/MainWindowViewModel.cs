using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Platform.Storage;
using Acut.Core.Audio;
using Acut.Core.Models;

namespace Acut.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IAudioService _audioService;
    private readonly IPlaybackService _playbackService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetStartTimeCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetEndTimeCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportSegmentCommand))]
    private AudioFileInfo? currentAudioFile;

    [ObservableProperty]
    private TimeSpan startTime;

    [ObservableProperty]
    private TimeSpan endTime;

    [ObservableProperty]
    private TimeSpan currentPosition;

    [ObservableProperty]
    private bool isPlaying;

    [ObservableProperty]
    private double volume = 0.5;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private float[]? waveformData;

    public MainWindowViewModel(IAudioService audioService, IPlaybackService playbackService)
    {
        _audioService = audioService;
        _playbackService = playbackService;

        // Subscribe to playback events
        _playbackService.PositionChanged += (s, position) =>
        {
            CurrentPosition = position;
        };

        _playbackService.PlaybackStopped += (s, e) =>
        {
            IsPlaying = false;
            StatusMessage = "Playback stopped";
        };
    }

    partial void OnVolumeChanged(double value)
    {
        _playbackService.Volume = (float)value;
    }

    [RelayCommand]
    private async Task OpenFile()
    {
        try
        {
            // Get the main window to show file picker
            var window = App.MainWindow;
            if (window == null) return;

            // Show file picker
            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Audio File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Audio Files")
                    {
                        Patterns = new[] { "*.mp3", "*.wav", "*.flac", "*.aac", "*.ogg", "*.m4a" }
                    },
                    new FilePickerFileType("All Files")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (files.Count == 0) return;

            var filePath = files[0].Path.LocalPath;

            StatusMessage = "Loading audio file...";
            IsLoading = true;

            // Load audio file metadata
            CurrentAudioFile = await _audioService.LoadAudioFileAsync(filePath);

            // Initialize time range to full duration
            StartTime = TimeSpan.Zero;
            EndTime = CurrentAudioFile.Duration;

            // Load audio for playback
            await _playbackService.LoadAsync(filePath);
            _playbackService.Volume = (float)Volume;

            // Generate waveform data
            StatusMessage = "Generating waveform...";
            WaveformData = await _audioService.GenerateWaveformDataAsync(filePath);

            StatusMessage = $"Loaded: {CurrentAudioFile.FileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading file: {ex.Message}";
            CurrentAudioFile = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void PlayPause()
    {
        if (_playbackService.IsPlaying)
        {
            _playbackService.Pause();
            IsPlaying = false;
            StatusMessage = "Paused";
        }
        else
        {
            _playbackService.Play();
            IsPlaying = true;
            StatusMessage = "Playing...";
        }
    }

    [RelayCommand]
    private void Stop()
    {
        _playbackService.Stop();
        IsPlaying = false;
        CurrentPosition = TimeSpan.Zero;
        StatusMessage = "Stopped";
    }

    [RelayCommand(CanExecute = nameof(CanSetTrimPoint))]
    private void SetStartTime()
    {
        StartTime = CurrentPosition;

        // Ensure start time doesn't exceed end time
        if (StartTime > EndTime)
        {
            EndTime = CurrentAudioFile?.Duration ?? StartTime;
        }

        StatusMessage = $"Start time set to: {StartTime}";
    }

    [RelayCommand(CanExecute = nameof(CanSetTrimPoint))]
    private void SetEndTime()
    {
        EndTime = CurrentPosition;

        // Ensure end time is not before start time
        if (EndTime < StartTime)
        {
            StartTime = TimeSpan.Zero;
        }

        StatusMessage = $"End time set to: {EndTime}";
    }

    private bool CanSetTrimPoint()
    {
        return CurrentAudioFile != null;
    }

    [RelayCommand(CanExecute = nameof(CanExportSegment))]
    private async Task ExportSegment()
    {
        try
        {
            if (CurrentAudioFile == null) return;

            // Get the main window to show save dialog
            var window = App.MainWindow;
            if (window == null) return;

            // Show file save picker
            var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export Audio Segment",
                SuggestedFileName = Path.GetFileNameWithoutExtension(CurrentAudioFile.FileName) + "_cut",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("MP3 Audio") { Patterns = new[] { "*.mp3" } },
                    new FilePickerFileType("WAV Audio") { Patterns = new[] { "*.wav" } },
                    new FilePickerFileType("FLAC Audio") { Patterns = new[] { "*.flac" } },
                    new FilePickerFileType("AAC Audio") { Patterns = new[] { "*.aac" } },
                    new FilePickerFileType("OGG Audio") { Patterns = new[] { "*.ogg" } },
                    new FilePickerFileType("M4A Audio") { Patterns = new[] { "*.m4a" } }
                }
            });

            if (file == null) return;

            var outputPath = file.Path.LocalPath;
            var extension = Path.GetExtension(outputPath).TrimStart('.').ToLower();

            StatusMessage = "Exporting segment...";
            IsLoading = true;

            // Determine export format
            var format = extension switch
            {
                "mp3" => AudioFormat.MP3,
                "wav" => AudioFormat.WAV,
                "flac" => AudioFormat.FLAC,
                "aac" => AudioFormat.AAC,
                "ogg" => AudioFormat.OGG,
                "m4a" => AudioFormat.M4A,
                _ => AudioFormat.MP3
            };

            var segment = new AudioSegment
            {
                StartTime = StartTime,
                EndTime = EndTime
            };

            var options = new ExportOptions
            {
                OutputPath = outputPath,
                Format = format,
                BitRate = 192,
                SampleRate = 44100
            };

            await _audioService.ExtractSegmentAsync(CurrentAudioFile.FilePath, outputPath, segment, options);

            StatusMessage = $"Exported successfully to: {Path.GetFileName(outputPath)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanExportSegment()
    {
        return CurrentAudioFile != null;
    }

    // Handler for waveform control seek requests
    public void OnWaveformSeekRequested(TimeSpan time)
    {
        if (_playbackService != null)
        {
            _playbackService.Seek(time);
            CurrentPosition = time;
            StatusMessage = $"Seeked to: {time:hh\\:mm\\:ss\\.fff}";
        }
    }

    // Handler for waveform control start time changes
    public void OnWaveformStartTimeChanged(TimeSpan time)
    {
        StartTime = time;
        StatusMessage = $"Start time: {StartTime:hh\\:mm\\:ss\\.fff}";
    }

    // Handler for waveform control end time changes
    public void OnWaveformEndTimeChanged(TimeSpan time)
    {
        EndTime = time;
        StatusMessage = $"End time: {EndTime:hh\\:mm\\:ss\\.fff}";
    }
}
