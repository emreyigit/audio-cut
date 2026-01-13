# Acut - Project Summary

## Overview
Acut is a cross-platform audio cutting desktop application built with C# and Avalonia UI. The project successfully implements core audio processing features including file loading, waveform visualization, playback controls, and multi-format export capabilities.

## Build Status
✅ **Build Successful** - All projects compile without errors (only minor .NET version warnings)

## Project Structure

```
Acut/
├── Acut.sln                    # Solution file
├── README.md                   # Main documentation
├── SETUP.md                    # Setup guide
├── .gitignore                  # Git ignore rules
├── setup.sh                    # Setup script for Mac/Linux
├── run.sh                      # Run script for Mac/Linux
│
├── src/
│   ├── Acut.Desktop/          # Main Avalonia UI application
│   │   ├── App.axaml           # Application entry point
│   │   ├── App.axaml.cs
│   │   ├── Program.cs
│   │   ├── ViewModels/
│   │   │   ├── MainWindowViewModel.cs  # Main VM with all application logic
│   │   │   └── ViewModelBase.cs
│   │   ├── Views/
│   │   │   ├── MainWindow.axaml        # Main UI layout
│   │   │   └── MainWindow.axaml.cs
│   │   ├── Converters/
│   │   │   └── ObjectConverters.cs     # Value converters for bindings
│   │   └── Properties/
│   │       └── launchSettings.json
│   │
│   ├── Acut.Core/             # Core business logic
│   │   ├── Audio/
│   │   │   ├── IAudioService.cs        # Audio service interface
│   │   │   ├── AudioService.cs         # FFmpeg-based audio processing
│   │   │   ├── IPlaybackService.cs     # Playback interface
│   │   │   └── PlaybackService.cs      # NAudio-based playback
│   │   └── Models/
│   │       ├── AudioFileInfo.cs         # Audio file metadata
│   │       ├── AudioSegment.cs          # Cut region definition
│   │       └── ExportOptions.cs         # Export settings
│   │
│   └── Acut.Common/           # Shared utilities (empty for now)
│
└── tests/                      # Test projects (to be added)
```

## Implemented Features

### ✅ Core Functionality
- Audio file loading (MP3, WAV, FLAC, AAC, OGG, M4A)
- File metadata extraction (duration, sample rate, channels, bitrate)
- Waveform data generation for visualization
- Audio segment extraction with precise timing
- Multi-format export with quality options
- Audio playback with NAudio
- Volume control
- Position tracking

### ✅ UI Components
- File open dialog with format filtering
- File save dialog with format selection
- Audio file information display
- Time selection inputs (start/end)
- Playback controls (play/pause/stop)
- Volume slider
- Status bar with loading indicator
- Empty state UI

### ✅ Architecture
- MVVM pattern with CommunityToolkit.Mvvm
- Separation of concerns (UI / Business Logic / Models)
- Async/await for all I/O operations
- Service-based architecture
- Observable properties with automatic UI updates

## Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| UI Framework | Avalonia UI | 11.3.10 |
| MVVM | CommunityToolkit.Mvvm | 8.2.1 |
| Audio Processing | NAudio | 2.2.1 |
| Format Conversion | FFMpegCore | 5.4.0 |
| Waveform Analysis | NWaves | 0.9.6 |
| Target Framework | .NET | 6.0 |

## Key Classes

### MainWindowViewModel
Main application logic handling:
- File operations (open, export)
- Audio playback control
- Waveform management
- State management

**Key Commands:**
- `OpenFileCommand` - Opens file picker and loads audio
- `ExportSegmentCommand` - Exports selected audio segment
- `PlayPauseCommand` - Toggles playback
- `StopCommand` - Stops playback and resets position

**Key Properties:**
- `CurrentAudioFile` - Current loaded audio metadata
- `WaveformData` - Array of waveform amplitudes
- `StartTime` / `EndTime` - Cut region boundaries
- `IsPlaying` - Playback state
- `Volume` - Playback volume (0-1)

### AudioService
Handles audio file operations:
- `LoadAudioFileAsync()` - Extracts metadata using FFProbe
- `GenerateWaveformDataAsync()` - Creates visualization data
- `ExtractSegmentAsync()` - Cuts audio using FFmpeg
- `ExportAudioAsync()` - Converts and exports audio

### PlaybackService
Manages audio playback:
- Uses NAudio's `WaveOutEvent` for cross-platform playback
- Position tracking with events
- Volume control
- Proper resource disposal

## How to Use

### 1. Setup
```bash
./setup.sh
```

### 2. Run
```bash
./run.sh
```

Or manually:
```bash
dotnet run --project src/Acut.Desktop/Acut.Desktop.csproj
```

### 3. Basic Workflow
1. Click "Open File" and select an audio file
2. Set start and end times for the cut
3. Use playback controls to preview
4. Click "Export" to save the trimmed audio

## Known Limitations & Future Enhancements

### Current Limitations
- Waveform visualization is basic (Canvas placeholder)
- Time input is text-based, no visual selection
- No undo/redo functionality
- No fade in/out effects (infrastructure exists but not wired up)
- Single cut region only

### Planned Enhancements
- Interactive waveform control with drag-to-select
- Zoom and pan for waveform
- Multiple cut regions
- Batch processing
- Fade effects
- Keyboard shortcuts
- Recent files list
- Audio effects (normalize, etc.)
- Better error handling and user feedback

## Dependencies

### Runtime Requirements
- .NET 6.0 Runtime
- FFmpeg (must be in system PATH)

### Development Requirements
- .NET 6.0 SDK
- Any IDE (VS Code, Visual Studio, Rider)

## macOS Specific Notes

The application is designed with macOS as the primary target:
- Uses Avalonia's cross-platform file dialogs
- Supports dark mode
- Native-looking UI with Fluent theme
- Proper app lifecycle management

To create a macOS app bundle:
```bash
dotnet publish src/Acut.Desktop/Acut.Desktop.csproj \
  -c Release \
  -r osx-x64 \
  --self-contained true \
  -p:PublishSingleFile=true
```

## Performance Considerations

- Waveform generation runs on background thread
- Audio processing is async to prevent UI blocking
- NAudio handles audio buffering efficiently
- FFmpeg provides hardware-accelerated encoding where available

## Security Notes

- No credential storage
- Files are processed locally, no network calls
- Temporary files are cleaned up after operations
- File access limited to user-selected files

## Testing

Currently, the project has no automated tests. Testing plan:
- Unit tests for AudioService
- Unit tests for PlaybackService
- Integration tests for file operations
- UI tests for main workflows

## Contributing Areas

Good places to start contributing:
1. **Waveform Control** - Interactive visualization component
2. **Time Input** - Better UX for selecting time ranges
3. **Effects** - Implement fade in/out, normalize, etc.
4. **Batch Processing** - Process multiple files at once
5. **Tests** - Add comprehensive test coverage

## License
MIT License

## Credits
Created as a demonstration of modern .NET desktop development with Avalonia UI.
