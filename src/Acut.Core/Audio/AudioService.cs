using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Acut.Core.Models;
using FFMpegCore;
using NWaves.Signals;
using NWaves.Audio;

namespace Acut.Core.Audio;

public class AudioService : IAudioService
{
    public async Task<AudioFileInfo> LoadAudioFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Audio file not found", filePath);

        var mediaInfo = await FFProbe.AnalyseAsync(filePath);

        return new AudioFileInfo
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath),
            Duration = mediaInfo.Duration,
            Format = Path.GetExtension(filePath).TrimStart('.').ToUpper(),
            SampleRate = mediaInfo.PrimaryAudioStream?.SampleRateHz ?? 0,
            Channels = mediaInfo.PrimaryAudioStream?.Channels ?? 0,
            BitRate = (int)(mediaInfo.PrimaryAudioStream?.BitRate ?? 0) / 1000 // Convert to kbps
        };
    }

    public async Task<float[]> GenerateWaveformDataAsync(string filePath, int sampleCount = 1000)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var fileStream = File.OpenRead(filePath);
                var waveFile = new WaveFile(fileStream);
                var signal = waveFile[Channels.Average];

                var samplesPerPoint = Math.Max(1, signal.Length / sampleCount);
                var waveformData = new float[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    var start = i * samplesPerPoint;
                    var end = Math.Min(start + samplesPerPoint, signal.Length);

                    // Calculate RMS (Root Mean Square) for this chunk
                    var sum = 0.0;
                    for (int j = start; j < end; j++)
                    {
                        sum += signal[j] * signal[j];
                    }
                    waveformData[i] = (float)Math.Sqrt(sum / (end - start));
                }

                return waveformData;
            }
            catch
            {
                // Return flat waveform on error
                return Enumerable.Repeat(0f, sampleCount).ToArray();
            }
        });
    }

    public async Task ExtractSegmentAsync(string inputPath, string outputPath, AudioSegment segment, ExportOptions exportOptions)
    {
        await Task.Run(() =>
        {
            FFMpegArguments
                .FromFileInput(inputPath, false, args => args
                    .Seek(segment.StartTime))
                .OutputToFile(outputPath, true, args => args
                    .WithDuration(segment.Duration)
                    .WithAudioBitrate(exportOptions.BitRate)
                    .WithAudioSamplingRate(exportOptions.SampleRate))
                .ProcessSynchronously();
        });
    }

    public async Task ExportAudioAsync(string inputPath, string outputPath, ExportOptions exportOptions)
    {
        await Task.Run(() =>
        {
            FFMpegArguments
                .FromFileInput(inputPath, false)
                .OutputToFile(outputPath, true, args =>
                {
                    args.WithAudioBitrate(exportOptions.BitRate)
                        .WithAudioSamplingRate(exportOptions.SampleRate);

                    // Set codec based on format
                    switch (exportOptions.Format)
                    {
                        case AudioFormat.MP3:
                            args.WithAudioCodec("libmp3lame");
                            break;
                        case AudioFormat.AAC:
                        case AudioFormat.M4A:
                            args.WithAudioCodec("aac");
                            break;
                        case AudioFormat.OGG:
                            args.WithAudioCodec("libvorbis");
                            break;
                        case AudioFormat.WAV:
                            args.WithAudioCodec("pcm_s16le");
                            break;
                        case AudioFormat.FLAC:
                            args.WithAudioCodec("flac");
                            break;
                    }
                })
                .ProcessSynchronously();
        });
    }
}
