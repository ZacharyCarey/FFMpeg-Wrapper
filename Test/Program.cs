// See https://aka.ms/new-console-template for more information

using FFMpeg_Wrapper;
using FFMpeg_Wrapper.Codecs;
using FFMpeg_Wrapper.ffmpeg;
using FFMpeg_Wrapper.ffprobe;
using FFMpeg_Wrapper.Filters;
using FFMpeg_Wrapper.Filters.Video;
using Iso639;
using System.Diagnostics;

MediaAnalysis? input = new FFProbe().Analyse(@"F:\Video\backup\TestVideo\BDMV\STREAM\00055.m2ts");
if (input == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Failed to read input file!");
    Console.ResetColor();
    return;
}

FFMpeg ffmpeg = new();

InputFileOptions inputOptions = new InputFileOptions()
    .SetStartTime(new TimeSpan(2, 17, 0));
OutputFileOptions outputOptions = new OutputFileOptions()
    .SetDuration(new TimeSpan(0, 5, 0))
    .SetTitle("This is a title.");

var arguments = ffmpeg.Transcode(@"C:\Users\Zach\Downloads\test3.mkv", outputOptions)
    .AddVideoStreams(
        input,
        new VideoStreamOptions() 
            .SetCodec(Codecs.LibSvtAV1
                .SetCRF(20)
                .SetPreset(5))
            .AddFilter(Filters.Scale(ScaleResolution.SD_720x480))
            .SetFlag(StreamFlag.Forced, true),
        inputOptions)
    .AddAudioStream(
        input.AudioStreams[0],
        new AudioStreamOptions() 
            .SetCodec(Codecs.AC3)
            .SetLanguage(Language.FromPart2("eng"))
            .SetFlag(StreamFlag.Commentary, true)
            .SetFlag(StreamFlag.HearingImpaired, true),
        inputOptions);

Stopwatch timer = new();
string? result = arguments
    .NotifyOnProgress((double percent) =>
    {
        if (!timer.IsRunning) timer.Start();
        if (percent >= 99.5) timer.Stop();
        Console.WriteLine($"Progress: {percent:0}%");
    })
    .SetLogPath(@"C:\Users\Zach\Downloads\TestLog.txt")
    .SetOverwrite(true)
    .Run();

Console.WriteLine($"Process took {timer.Elapsed} to finish.");

if (result != null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"FFMpeg encountered an error! ({result})");
    Console.ResetColor();
    return;
}
Console.WriteLine("Success!");

//ffmpeg.Snapshot(@"C:\Users\Zach\Downloads\Test.mp4", 0, @"C:\Users\Zach\Downloads\Snapshot_frame_0.png").Run();