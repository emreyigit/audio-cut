using System;

namespace Acut.Core.Models;

public class AudioSegment
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
}
