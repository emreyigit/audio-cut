#!/bin/bash

echo "🎵 Starting Acut Audio Cutter..."
echo ""

# Check if FFmpeg is available
if ! command -v ffmpeg &> /dev/null; then
    echo "⚠️  Warning: FFmpeg not found. Audio processing may not work correctly."
    echo "   Install FFmpeg with: brew install ffmpeg (macOS)"
    echo ""
fi

# Run the application
dotnet run --project src/Acut.Desktop/Acut.Desktop.csproj
