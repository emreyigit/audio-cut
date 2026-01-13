using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using Acut.Desktop.ViewModels;

namespace Acut.Desktop.Views;

public partial class MainWindow : Window
{
    private Line? _playbackCursor;

    public MainWindow()
    {
        InitializeComponent();

        // Subscribe to DataContext changes to hook up waveform rendering
        DataContextChanged += OnDataContextChanged;

        // Handle window closing to ensure cleanup
        Closing += OnWindowClosing;
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Clean up resources when window closes
        if (DataContext is MainWindowViewModel viewModel)
        {
            // Stop any playback
            viewModel.StopCommand?.Execute(null);
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.WaveformData))
        {
            // Ensure we're on the UI thread
            Avalonia.Threading.Dispatcher.UIThread.Post(() => DrawWaveform());
        }
        else if (e.PropertyName == nameof(MainWindowViewModel.CurrentPosition))
        {
            // Ensure we're on the UI thread
            Avalonia.Threading.Dispatcher.UIThread.Post(() => UpdatePlaybackCursor());
        }
    }

    private void DrawWaveform()
    {
        var canvas = this.FindControl<Canvas>("WaveformCanvas");
        if (canvas == null) return;

        var viewModel = DataContext as MainWindowViewModel;
        if (viewModel?.WaveformData == null || viewModel.WaveformData.Length == 0)
            return;

        canvas.Children.Clear();

        var width = canvas.Bounds.Width;
        var height = canvas.Bounds.Height;

        if (width <= 0 || height <= 0)
        {
            // Canvas not yet sized, try again on layout update
            canvas.LayoutUpdated += (s, e) =>
            {
                if (canvas.Bounds.Width > 0 && canvas.Bounds.Height > 0)
                {
                    DrawWaveform();
                }
            };
            return;
        }

        var waveformData = viewModel.WaveformData;
        var pointSpacing = width / waveformData.Length;
        var centerY = height / 2;
        var maxAmplitude = 0f;

        // Find max amplitude for scaling
        foreach (var sample in waveformData)
        {
            if (Math.Abs(sample) > maxAmplitude)
                maxAmplitude = Math.Abs(sample);
        }

        if (maxAmplitude == 0)
            maxAmplitude = 1;

        var scaleFactor = (height / 2 - 10) / maxAmplitude;

        // Draw waveform as vertical lines
        for (int i = 0; i < waveformData.Length; i++)
        {
            var amplitude = waveformData[i] * scaleFactor;
            var x = i * pointSpacing;

            var line = new Line
            {
                StartPoint = new Avalonia.Point(x, centerY - amplitude),
                EndPoint = new Avalonia.Point(x, centerY + amplitude),
                Stroke = new SolidColorBrush(Color.Parse("#4A9EFF")),
                StrokeThickness = Math.Max(1, pointSpacing * 0.8)
            };

            canvas.Children.Add(line);
        }

        // Draw center line
        var centerLine = new Line
        {
            StartPoint = new Avalonia.Point(0, centerY),
            EndPoint = new Avalonia.Point(width, centerY),
            Stroke = new SolidColorBrush(Color.Parse("#666666")),
            StrokeThickness = 1
        };

        canvas.Children.Add(centerLine);

        // Create playback cursor
        _playbackCursor = new Line
        {
            StartPoint = new Avalonia.Point(0, 0),
            EndPoint = new Avalonia.Point(0, height),
            Stroke = new SolidColorBrush(Color.Parse("#FF4444")),
            StrokeThickness = 2,
            IsVisible = false
        };

        canvas.Children.Add(_playbackCursor);
    }

    private void UpdatePlaybackCursor()
    {
        if (_playbackCursor == null) return;

        var canvas = this.FindControl<Canvas>("WaveformCanvas");
        if (canvas == null) return;

        var viewModel = DataContext as MainWindowViewModel;
        if (viewModel?.CurrentAudioFile == null) return;

        var width = canvas.Bounds.Width;
        var height = canvas.Bounds.Height;

        if (width <= 0 || height <= 0) return;

        // Calculate cursor position based on current time
        var totalDuration = viewModel.CurrentAudioFile.Duration.TotalSeconds;
        var currentPosition = viewModel.CurrentPosition.TotalSeconds;

        if (totalDuration <= 0) return;

        var progress = currentPosition / totalDuration;
        var cursorX = progress * width;

        // Update cursor position
        _playbackCursor.StartPoint = new Avalonia.Point(cursorX, 0);
        _playbackCursor.EndPoint = new Avalonia.Point(cursorX, height);

        // Show cursor when playing
        _playbackCursor.IsVisible = viewModel.IsPlaying || currentPosition > 0;
    }
}