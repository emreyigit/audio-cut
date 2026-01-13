# Acut Setup Guide

## Prerequisites Installation

### 1. Install .NET SDK

Download and install .NET 6.0 SDK or later from:
https://dotnet.microsoft.com/download

Verify installation:
```bash
dotnet --version
```

### 2. Install FFmpeg

FFmpeg is required for audio processing and format conversion.

#### macOS (using Homebrew)
```bash
# Install Homebrew if you don't have it
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install FFmpeg
brew install ffmpeg
```

#### macOS (using MacPorts)
```bash
sudo port install ffmpeg
```

#### Windows (using Chocolatey)
```bash
choco install ffmpeg
```

#### Windows (Manual Installation)
1. Download FFmpeg from https://ffmpeg.org/download.html
2. Extract to a folder (e.g., `C:\ffmpeg`)
3. Add `C:\ffmpeg\bin` to your system PATH

#### Linux (Debian/Ubuntu)
```bash
sudo apt update
sudo apt install ffmpeg
```

#### Linux (Fedora)
```bash
sudo dnf install ffmpeg
```

#### Verify FFmpeg Installation
```bash
ffmpeg -version
```

## Quick Start

### Option 1: Using Setup Script (macOS/Linux)

```bash
# Make setup script executable
chmod +x setup.sh

# Run setup
./setup.sh

# Run the application
./run.sh
```

### Option 2: Manual Setup

```bash
# Clone/navigate to the project directory
cd Acut

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project src/Acut.Desktop/Acut.Desktop.csproj
```

## Development Environment Setup

### Visual Studio Code
1. Install VS Code
2. Install C# extension (ms-dotnettools.csharp)
3. Install Avalonia for VSCode extension (optional, for XAML support)
4. Open the project folder
5. Press F5 to debug

### Visual Studio (Windows)
1. Open `Acut.sln`
2. Set `Acut.Desktop` as startup project
3. Press F5 to run

### JetBrains Rider
1. Open `Acut.sln`
2. Rider will automatically restore packages
3. Press F5 to run

## Troubleshooting

### "FFmpeg not found" Error

**Problem**: Application can't find FFmpeg executable.

**Solution**:
- Ensure FFmpeg is installed and in your system PATH
- Test with: `ffmpeg -version`
- On macOS, after installing with Homebrew, restart your terminal
- On Windows, you may need to restart your computer after adding to PATH

### "Could not load file or assembly" Error

**Problem**: Missing or incompatible NuGet packages.

**Solution**:
```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

### Audio File Won't Load

**Problem**: Unsupported audio format or corrupted file.

**Solution**:
- Ensure the file is a valid audio file
- Supported formats: MP3, WAV, FLAC, AAC, OGG, M4A
- Try converting the file with FFmpeg first:
  ```bash
  ffmpeg -i input.xxx output.mp3
  ```

### macOS Security Warning

**Problem**: "App is from an unidentified developer"

**Solution**:
1. Right-click the app
2. Select "Open"
3. Click "Open" in the dialog

Or disable Gatekeeper temporarily:
```bash
sudo spctl --master-disable
# Run the app
sudo spctl --master-enable
```

## Building for Production

### macOS App Bundle

```bash
# Build self-contained app
dotnet publish src/Acut.Desktop/Acut.Desktop.csproj \
  -c Release \
  -r osx-x64 \
  --self-contained true \
  -p:PublishSingleFile=true

# Output will be in:
# src/Acut.Desktop/bin/Release/net6.0/osx-x64/publish/
```

### Windows Executable

```bash
dotnet publish src/Acut.Desktop/Acut.Desktop.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true
```

### Linux Binary

```bash
dotnet publish src/Acut.Desktop/Acut.Desktop.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true
```

## FFmpeg Configuration

The application expects FFmpeg to be in your system PATH. If you have FFmpeg in a custom location, you can:

1. Add it to your PATH environment variable, or
2. Modify the `AudioService.cs` to specify the FFmpeg path:

```csharp
// In Acut.Core/Audio/AudioService.cs
GlobalFFOptions.Configure(options => {
    options.BinaryFolder = "/path/to/ffmpeg/folder";
});
```

## Next Steps

After setup is complete:
1. Run the application
2. Click "Open File" to load an audio file
3. Set start and end times for your cut
4. Preview with playback controls
5. Click "Export" to save your edited audio

For more information, see [README.md](README.md)
