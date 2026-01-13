# Interactive Waveform Selection API

This document describes the implementation and API for the interactive waveform selection feature in Acut.

## Overview

The interactive waveform selection feature allows users to visually select audio regions for cutting and editing by clicking and dragging directly on the waveform display.

## Components

### WaveformControl

**Location**: `src/Acut.Desktop/Controls/WaveformControl.axaml.cs`

A custom Avalonia UserControl that provides interactive waveform visualization and selection capabilities.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `WaveformData` | `float[]?` | Array of amplitude values representing the audio waveform |
| `StartTime` | `TimeSpan` | Start time of the selected region |
| `EndTime` | `TimeSpan` | End time of the selected region |
| `Duration` | `TimeSpan` | Total duration of the audio file |
| `CurrentPosition` | `TimeSpan` | Current playback position |

#### Events

| Event | Type | Description |
|-------|------|-------------|
| `StartTimeChanged` | `EventHandler<TimeSpan>` | Fired when the selection start time changes via user interaction |
| `EndTimeChanged` | `EventHandler<TimeSpan>` | Fired when the selection end time changes via user interaction |
| `SeekRequested` | `EventHandler<TimeSpan>` | Fired when user requests to seek to a specific position (Shift+Click) |

#### User Interactions

1. **Click and Drag Selection**
   - Click on the waveform and drag to create a new selection region
   - The selection is highlighted with a semi-transparent overlay

2. **Adjust Selection Handles**
   - Click and drag the blue handles at the start or end of the selection
   - Handles are 8px wide with a 12px hit-test margin for easier interaction

3. **Seek to Position**
   - Hold Shift and click on the waveform to seek playback to that position
   - The `SeekRequested` event is fired with the target time

4. **Visual Feedback**
   - Selection region: Semi-transparent white overlay
   - Selection handles: Blue rectangles at start/end positions
   - Playback position: White vertical line
   - Cursor changes to resize cursor when hovering over handles

## Integration

### XAML Usage

```xml
<controls:WaveformControl
    WaveformData="{Binding WaveformData}"
    StartTime="{Binding StartTime}"
    EndTime="{Binding EndTime}"
    Duration="{Binding CurrentAudioFile.Duration}"
    CurrentPosition="{Binding CurrentPosition}"/>
```

### Code-Behind Event Wiring

```csharp
var waveformControl = this.FindControl<WaveformControl>("WaveformControl");
if (waveformControl != null && DataContext is MainWindowViewModel viewModel)
{
    waveformControl.StartTimeChanged += (s, time) => viewModel.OnWaveformStartTimeChanged(time);
    waveformControl.EndTimeChanged += (s, time) => viewModel.OnWaveformEndTimeChanged(time);
    waveformControl.SeekRequested += (s, time) => viewModel.OnWaveformSeekRequested(time);
}
```

### ViewModel Methods

The MainWindowViewModel exposes three public methods to handle waveform events:

```csharp
public void OnWaveformStartTimeChanged(TimeSpan time)
{
    StartTime = time;
    StatusMessage = $"Start time: {StartTime:hh\\:mm\\:ss\\.fff}";
}

public void OnWaveformEndTimeChanged(TimeSpan time)
{
    EndTime = time;
    StatusMessage = $"End time: {EndTime:hh\\:mm\\:ss\\.fff}";
}

public void OnWaveformSeekRequested(TimeSpan time)
{
    _playbackService.Seek(time);
    CurrentPosition = time;
    StatusMessage = $"Seeked to: {time:hh\\:mm\\:ss\\.fff}";
}
```

## Rendering Details

### Waveform Visualization

The waveform is rendered using a filled polygon that represents the audio amplitude:

1. Audio samples are normalized to the maximum amplitude
2. Top and bottom points are calculated for each sample
3. A `StreamGeometry` is created to draw the filled waveform shape
4. Color: `#4682B4` (Steel Blue) with 60% opacity

### Selection Region

The selection region consists of:

1. **Overlay Rectangle**: Semi-transparent white rectangle showing the selected region
2. **Start Handle**: Blue vertical bar at the start position
3. **End Handle**: Blue vertical bar at the end position

### Playback Position Indicator

A white vertical line that moves across the waveform during playback:
- Color: White (`#FFFFFF`)
- Thickness: 2px
- Updates in real-time based on `CurrentPosition` property

## Performance Considerations

1. **Waveform Data Sampling**: Default sample count is 1000 points
   - Configurable via `AudioService.GenerateWaveformDataAsync(filePath, sampleCount)`

2. **UI Thread Marshalling**: All rendering operations are posted to the UI thread using `Dispatcher.UIThread.Post()`

3. **Event Throttling**: Property changes trigger UI updates via `DispatcherPriority.Render` to batch rendering operations

## Extending the API

### Adding Custom Interactions

To add new interaction modes, extend the pointer event handlers in `WaveformControl.axaml.cs`:

1. Add state tracking fields (e.g., `private bool _isCustomMode`)
2. Update `OnPointerPressed` to detect the new interaction
3. Handle movement in `OnPointerMoved`
4. Clean up state in `OnPointerReleased`
5. Fire appropriate events to notify the ViewModel

### Custom Rendering

To add custom visual elements:

1. Override or extend the `RenderWaveform()` method
2. Create Avalonia shapes and add them to `WaveformCanvas.Children`
3. Use `Canvas.SetLeft()` and `Canvas.SetTop()` to position elements

Example:
```csharp
private void DrawCustomMarker(double x)
{
    var marker = new Avalonia.Controls.Shapes.Ellipse
    {
        Fill = new SolidColorBrush(Colors.Red),
        Width = 10,
        Height = 10
    };
    Canvas.SetLeft(marker, x - 5);
    Canvas.SetTop(marker, 0);
    WaveformCanvas.Children.Add(marker);
}
```

## Future Enhancements

Potential improvements to the interactive waveform API:

1. **Zoom and Pan**
   - Add `ZoomLevel` property
   - Support pinch-to-zoom gesture
   - Horizontal scrolling for zoomed waveforms

2. **Multiple Regions**
   - Support selecting multiple regions for batch operations
   - `Regions` collection property
   - Region management events

3. **Snap to Beat/Grid**
   - Optional grid overlay
   - Snap selection handles to grid lines
   - Beat detection integration

4. **Waveform Colors**
   - Customizable color schemes
   - Gradient-based amplitude visualization
   - Theme support (light/dark mode)

5. **Keyboard Shortcuts**
   - Arrow keys for fine-tuning selection
   - Delete key to clear selection
   - Number keys for quick position jumps

## Testing

To test the interactive waveform feature:

1. Build and run the application:
   ```bash
   dotnet run --project src/Acut.Desktop/Acut.Desktop.csproj
   ```

2. Load an audio file using the "Open File" button

3. Test interactions:
   - Click and drag to create selection
   - Drag handles to adjust selection
   - Shift+Click to seek
   - Verify playback position indicator moves during playback

4. Check status bar for feedback messages during interactions

## Troubleshooting

### Waveform not displaying
- Check that `WaveformData` is not null or empty
- Verify the control has valid bounds (width and height > 0)
- Check for exceptions during waveform generation

### Selection not responding
- Ensure events are properly wired in the code-behind
- Verify ViewModel methods are being called
- Check that `Duration` property is set correctly

### Performance issues
- Reduce waveform sample count if file is very large
- Consider implementing virtualization for very long audio files
- Profile rendering operations using Avalonia DevTools
