using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace Acut.Desktop.Controls;

public partial class WaveformControl : UserControl
{
    // Styled properties
    public static readonly StyledProperty<float[]?> WaveformDataProperty =
        AvaloniaProperty.Register<WaveformControl, float[]?>(nameof(WaveformData));

    public static readonly StyledProperty<TimeSpan> StartTimeProperty =
        AvaloniaProperty.Register<WaveformControl, TimeSpan>(nameof(StartTime));

    public static readonly StyledProperty<TimeSpan> EndTimeProperty =
        AvaloniaProperty.Register<WaveformControl, TimeSpan>(nameof(EndTime));

    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<WaveformControl, TimeSpan>(nameof(Duration));

    public static readonly StyledProperty<TimeSpan> CurrentPositionProperty =
        AvaloniaProperty.Register<WaveformControl, TimeSpan>(nameof(CurrentPosition));

    // Events
    public event EventHandler<TimeSpan>? StartTimeChanged;
    public event EventHandler<TimeSpan>? EndTimeChanged;
    public event EventHandler<TimeSpan>? SeekRequested;

    // Properties
    public float[]? WaveformData
    {
        get => GetValue(WaveformDataProperty);
        set => SetValue(WaveformDataProperty, value);
    }

    public TimeSpan StartTime
    {
        get => GetValue(StartTimeProperty);
        set => SetValue(StartTimeProperty, value);
    }

    public TimeSpan EndTime
    {
        get => GetValue(EndTimeProperty);
        set => SetValue(EndTimeProperty, value);
    }

    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public TimeSpan CurrentPosition
    {
        get => GetValue(CurrentPositionProperty);
        set => SetValue(CurrentPositionProperty, value);
    }

    // Private fields
    private bool _isSelecting = false;
    private bool _isDraggingStart = false;
    private bool _isDraggingEnd = false;
    private double _selectionStartX = 0;
    private const double HandleWidth = 8;
    private const double HandleHitTestMargin = 12;

    public WaveformControl()
    {
        InitializeComponent();

        // Subscribe to property changes
        WaveformDataProperty.Changed.AddClassHandler<WaveformControl>((control, e) => control.OnWaveformDataChanged());
        StartTimeProperty.Changed.AddClassHandler<WaveformControl>((control, e) => control.OnSelectionChanged());
        EndTimeProperty.Changed.AddClassHandler<WaveformControl>((control, e) => control.OnSelectionChanged());
        CurrentPositionProperty.Changed.AddClassHandler<WaveformControl>((control, e) => control.OnCurrentPositionChanged());
    }

    private void OnWaveformDataChanged()
    {
        Dispatcher.UIThread.Post(() => RenderWaveform(), DispatcherPriority.Render);
    }

    private void OnSelectionChanged()
    {
        Dispatcher.UIThread.Post(() => RenderWaveform(), DispatcherPriority.Render);
    }

    private void OnCurrentPositionChanged()
    {
        Dispatcher.UIThread.Post(() => RenderWaveform(), DispatcherPriority.Render);
    }

    private void RenderWaveform()
    {
        if (WaveformCanvas == null || WaveformData == null || WaveformData.Length == 0)
            return;

        var width = WaveformCanvas.Bounds.Width;
        var height = WaveformCanvas.Bounds.Height;

        if (width <= 0 || height <= 0)
            return;

        WaveformCanvas.Children.Clear();

        // Draw waveform
        DrawWaveform(width, height);

        // Draw selection region
        DrawSelectionRegion(width, height);

        // Draw current position indicator
        DrawPositionIndicator(width, height);
    }

    private void DrawWaveform(double width, double height)
    {
        if (WaveformData == null || WaveformData.Length == 0)
            return;

        var centerY = height / 2;
        var maxAmplitude = WaveformData.Max();
        if (maxAmplitude == 0) maxAmplitude = 1;

        var points = new PolylineGeometry();
        var topPoints = new System.Collections.Generic.List<Point>();
        var bottomPoints = new System.Collections.Generic.List<Point>();

        for (int i = 0; i < WaveformData.Length; i++)
        {
            var x = (i / (double)WaveformData.Length) * width;
            var normalizedValue = WaveformData[i] / maxAmplitude;
            var amplitude = normalizedValue * (height * 0.4);

            topPoints.Add(new Point(x, centerY - amplitude));
            bottomPoints.Add(new Point(x, centerY + amplitude));
        }

        // Create path for waveform
        var streamGeometry = new StreamGeometry();
        using (var context = streamGeometry.Open())
        {
            if (topPoints.Count > 0)
            {
                context.BeginFigure(topPoints[0], true);
                foreach (var point in topPoints.Skip(1))
                {
                    context.LineTo(point);
                }
                foreach (var point in bottomPoints.AsEnumerable().Reverse())
                {
                    context.LineTo(point);
                }
                context.EndFigure(true);
            }
        }

        var waveformPath = new Avalonia.Controls.Shapes.Path
        {
            Data = streamGeometry,
            Fill = new SolidColorBrush(Color.FromRgb(70, 130, 180)),
            Opacity = 0.6
        };

        WaveformCanvas.Children.Add(waveformPath);
    }

    private void DrawSelectionRegion(double width, double height)
    {
        if (Duration.TotalSeconds == 0)
            return;

        var startX = (StartTime.TotalSeconds / Duration.TotalSeconds) * width;
        var endX = (EndTime.TotalSeconds / Duration.TotalSeconds) * width;
        var selectionWidth = endX - startX;

        // Selection overlay
        var selectionRect = new Avalonia.Controls.Shapes.Rectangle
        {
            Fill = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
            Width = selectionWidth,
            Height = height
        };
        Canvas.SetLeft(selectionRect, startX);
        Canvas.SetTop(selectionRect, 0);
        WaveformCanvas.Children.Add(selectionRect);

        // Start handle
        var startHandle = new Avalonia.Controls.Shapes.Rectangle
        {
            Fill = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
            Width = HandleWidth,
            Height = height,
            Cursor = new Cursor(StandardCursorType.SizeWestEast)
        };
        Canvas.SetLeft(startHandle, startX - HandleWidth / 2);
        Canvas.SetTop(startHandle, 0);
        WaveformCanvas.Children.Add(startHandle);

        // End handle
        var endHandle = new Avalonia.Controls.Shapes.Rectangle
        {
            Fill = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
            Width = HandleWidth,
            Height = height,
            Cursor = new Cursor(StandardCursorType.SizeWestEast)
        };
        Canvas.SetLeft(endHandle, endX - HandleWidth / 2);
        Canvas.SetTop(endHandle, 0);
        WaveformCanvas.Children.Add(endHandle);
    }

    private void DrawPositionIndicator(double width, double height)
    {
        if (Duration.TotalSeconds == 0)
            return;

        var positionX = (CurrentPosition.TotalSeconds / Duration.TotalSeconds) * width;

        var positionLine = new Avalonia.Controls.Shapes.Line
        {
            StartPoint = new Point(positionX, 0),
            EndPoint = new Point(positionX, height),
            Stroke = new SolidColorBrush(Colors.White),
            StrokeThickness = 2
        };
        WaveformCanvas.Children.Add(positionLine);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WaveformCanvas == null || Duration.TotalSeconds == 0)
            return;

        var point = e.GetPosition(WaveformCanvas);
        var width = WaveformCanvas.Bounds.Width;

        var startX = (StartTime.TotalSeconds / Duration.TotalSeconds) * width;
        var endX = (EndTime.TotalSeconds / Duration.TotalSeconds) * width;

        // Check if clicking near start handle
        if (Math.Abs(point.X - startX) < HandleHitTestMargin)
        {
            _isDraggingStart = true;
            e.Pointer.Capture(WaveformCanvas);
        }
        // Check if clicking near end handle
        else if (Math.Abs(point.X - endX) < HandleHitTestMargin)
        {
            _isDraggingEnd = true;
            e.Pointer.Capture(WaveformCanvas);
        }
        // Otherwise start new selection
        else
        {
            _isSelecting = true;
            _selectionStartX = point.X;

            var clickTime = TimeSpan.FromSeconds((point.X / width) * Duration.TotalSeconds);

            // If shift key is held, seek to position
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                SeekRequested?.Invoke(this, clickTime);
            }
            else
            {
                StartTime = clickTime;
                EndTime = clickTime;
                StartTimeChanged?.Invoke(this, StartTime);
                EndTimeChanged?.Invoke(this, EndTime);
            }

            e.Pointer.Capture(WaveformCanvas);
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (WaveformCanvas == null || Duration.TotalSeconds == 0)
            return;

        var point = e.GetPosition(WaveformCanvas);
        var width = WaveformCanvas.Bounds.Width;

        // Clamp point.X to valid range
        point = point.WithX(Math.Clamp(point.X, 0, width));

        var time = TimeSpan.FromSeconds((point.X / width) * Duration.TotalSeconds);

        if (_isDraggingStart)
        {
            StartTime = TimeSpan.FromSeconds(Math.Min(time.TotalSeconds, EndTime.TotalSeconds));
            StartTimeChanged?.Invoke(this, StartTime);
        }
        else if (_isDraggingEnd)
        {
            EndTime = TimeSpan.FromSeconds(Math.Max(time.TotalSeconds, StartTime.TotalSeconds));
            EndTimeChanged?.Invoke(this, EndTime);
        }
        else if (_isSelecting)
        {
            if (point.X > _selectionStartX)
            {
                StartTime = TimeSpan.FromSeconds((_selectionStartX / width) * Duration.TotalSeconds);
                EndTime = time;
            }
            else
            {
                StartTime = time;
                EndTime = TimeSpan.FromSeconds((_selectionStartX / width) * Duration.TotalSeconds);
            }
            StartTimeChanged?.Invoke(this, StartTime);
            EndTimeChanged?.Invoke(this, EndTime);
        }
        else
        {
            // Update cursor based on position
            var startX = (StartTime.TotalSeconds / Duration.TotalSeconds) * width;
            var endX = (EndTime.TotalSeconds / Duration.TotalSeconds) * width;

            if (Math.Abs(point.X - startX) < HandleHitTestMargin ||
                Math.Abs(point.X - endX) < HandleHitTestMargin)
            {
                Cursor = new Cursor(StandardCursorType.SizeWestEast);
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.Arrow);
            }
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isSelecting = false;
        _isDraggingStart = false;
        _isDraggingEnd = false;
        e.Pointer.Capture(null);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        RenderWaveform();
    }
}
