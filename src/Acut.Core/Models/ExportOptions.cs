namespace Acut.Core.Models;

public class ExportOptions
{
    public string OutputPath { get; set; } = string.Empty;
    public AudioFormat Format { get; set; } = AudioFormat.MP3;
    public int BitRate { get; set; } = 192; // kbps
    public int SampleRate { get; set; } = 44100; // Hz
}

public enum AudioFormat
{
    MP3,
    WAV,
    FLAC,
    AAC,
    OGG,
    M4A
}
