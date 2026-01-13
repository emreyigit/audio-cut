using System;

namespace Acut.Core.Models;

public class AudioFileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string Format { get; set; } = string.Empty;
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public int BitRate { get; set; }
}
