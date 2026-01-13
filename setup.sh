#!/bin/bash

echo "🎵 Acut Audio Cutter - Setup Script"
echo "===================================="
echo ""

# Check for .NET SDK
echo "Checking for .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 6.0 or later from https://dotnet.microsoft.com/download"
    exit 1
else
    DOTNET_VERSION=$(dotnet --version)
    echo "✅ .NET SDK found: $DOTNET_VERSION"
fi

echo ""

# Check for FFmpeg
echo "Checking for FFmpeg..."
if ! command -v ffmpeg &> /dev/null; then
    echo "⚠️  FFmpeg not found. Installing FFmpeg is required for audio processing."
    echo ""

    if [[ "$OSTYPE" == "darwin"* ]]; then
        echo "On macOS, you can install it with Homebrew:"
        echo "  brew install ffmpeg"
    elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
        echo "On Linux, you can install it with:"
        echo "  sudo apt install ffmpeg  (Debian/Ubuntu)"
        echo "  sudo dnf install ffmpeg  (Fedora)"
    fi
    echo ""
    read -p "Would you like to continue anyway? (y/n) " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
else
    FFMPEG_VERSION=$(ffmpeg -version | head -n1)
    echo "✅ FFmpeg found: $FFMPEG_VERSION"
fi

echo ""
echo "Restoring NuGet packages..."
dotnet restore

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Setup complete!"
    echo ""
    echo "To run the application:"
    echo "  ./run.sh"
    echo ""
    echo "Or manually:"
    echo "  dotnet run --project src/Acut.Desktop/Acut.Desktop.csproj"
else
    echo "❌ Setup failed. Please check the errors above."
    exit 1
fi
