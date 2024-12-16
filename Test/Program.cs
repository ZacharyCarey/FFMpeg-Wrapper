// See https://aka.ms/new-console-template for more information

using FFMpeg_Wrapper;
using FFMpeg_Wrapper.Codecs;
using FFMpeg_Wrapper.ffmpeg;
using FFMpeg_Wrapper.ffprobe;
using FFMpeg_Wrapper.Filters;
using FFMpeg_Wrapper.Filters.Video;
using Iso639;
using System.Diagnostics;

/*
ffmpeg -i INPUT                                        \
  -map 0:v:0 -c:v libx264 -crf 45 -f null -            \
  -threads 3 -dec 0:0                                  \
  -filter_complex '[0:v][dec:0]hstack[stack]'          \
  -map '[stack]' -c:v ffv1 OUTPUT

┌──────────┬───────────────┐
│ demuxer  │               │   ┌─────────┐            ┌─────────┐    ┌────────────────────┐
╞══════════╡ video stream  │   │  video  │            │  video  │    │ null muxer         │
│   INPUT  │               ├──⮞│ decoder ├──┬────────⮞│ encoder ├─┬─⮞│(discards its input)│
│          │               │   └─────────┘  │         │(libx264)│ │  └────────────────────┘
└──────────┴───────────────┘                │         └─────────┘ │
                                 ╭───────⮜──╯   ┌─────────┐       │
                                 │              │loopback │       │
                                 │ ╭─────⮜──────┤ decoder ├────⮜──╯
                                 │ │            └─────────┘
                                 │ │
                                 │ │
                                 │ │  ┌───────────────────┐
                                 │ │  │complex filtergraph│
                                 │ │  ╞═══════════════════╡
                                 │ │  │  ┌─────────────┐  │
                                 ╰─╫─⮞├─⮞│   hstack    ├─⮞├╮
                                   ╰─⮞├─⮞│             │  ││
                                      │  └─────────────┘  ││
                                      └───────────────────┘│
                                                           │
┌──────────┬───────────────┐  ┌─────────┐                  │
│ muxer    │               │  │  video  │                  │
╞══════════╡ video stream  │⮜─┤ encoder ├───────⮜──────────╯
│  OUTPUT  │               │  │ (ffv1)  │
│          │               │  └─────────┘
└──────────┴───────────────┘
*/
MediaAnalysis? input = new FFProbe().Analyse(@"C:\Users\Zach\Downloads\input_4k.mkv");
if (input == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Failed to read input file!");
    Console.ResetColor();
    return;
}

FFMpeg ffmpeg = new();

OutputFile outputFile = new(@"C:\Users\Zach\Downloads\test.mkv");
outputFile.Title = "This is the movie title";

FileInput file = new FileInput(input);
var arguments = ffmpeg.Transcode(outputFile);
arguments.AddInput(new ConcatArguments()
    .AddFiles(file, file, file));
var filter = new FilterArguments()
    .AddFilter(Filters.Scale(ScaleResolution.HD_1920x1080))
    .AddInput(0, input.PrimaryVideoStream.Index)
    .AddOutput("v0");

arguments.SetInputFilter(filter);
arguments.AddStream(new VideoStreamOptions("v0")
    .SetCodec(Codecs.LibSvtAV1
        .SetCRF(20)
        .SetPreset(5))
    .SetName("Video 1"));
arguments.CopyStreams(0, input.AudioStreams);
arguments.CopyStreams(0, input.SubtitleStreams);
////////

Stopwatch timer = new();
timer.Start();
string? result = arguments
    .NotifyOnProgress((double percent) =>
    {
        Console.WriteLine($"Progress: {percent:0}%");
    })
    .SetLogPath(@"C:\Users\Zach\Downloads\TestLog.txt")
    .SetOverwrite(true)
    .Run();
timer.Stop();
Console.WriteLine($"Process took {timer.Elapsed} to finish.");

if (result != null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"FFMpeg encountered an error! ({result})");
    Console.ResetColor();
    return;
}

long originalSize = new FileInfo(input.FilePath).Length;
long newSize = new FileInfo(@"C:\Users\Zach\Downloads\test.mkv").Length;

Console.WriteLine($"Time percentage: {(timer.Elapsed / input.Duration):0.00000000}");
Console.WriteLine($"File size percentage: {((double)newSize / originalSize):0.00000000}");

//Console.WriteLine("Success!");

//ffmpeg.Snapshot(@"C:\Users\Zach\Downloads\Test.mp4", 0, @"C:\Users\Zach\Downloads\Snapshot_frame_0.png").Run();