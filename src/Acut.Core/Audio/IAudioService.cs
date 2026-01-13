using System.Threading.Tasks;
using Acut.Core.Models;

namespace Acut.Core.Audio;

public interface IAudioService
{
    /// <summary>
    /// Loads audio file metadata
    /// </summary>
    Task<AudioFileInfo> LoadAudioFileAsync(string filePath);

    /// <summary>
    /// Generates waveform data for visualization
    /// </summary>
    Task<float[]> GenerateWaveformDataAsync(string filePath, int sampleCount = 1000);

    /// <summary>
    /// Extracts a segment from the audio file
    /// </summary>
    Task ExtractSegmentAsync(string inputPath, string outputPath, AudioSegment segment, ExportOptions options);

    /// <summary>
    /// Exports audio with specified options
    /// </summary>
    Task ExportAudioAsync(string inputPath, string outputPath, ExportOptions options);
}
