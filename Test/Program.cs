// See https://aka.ms/new-console-template for more information

using FFMpeg_Wrapper;
using FFMpeg_Wrapper.Codecs;
using FFMpeg_Wrapper.ffprobe;
using Iso639;

MediaAnalysis? input = new FFProbe().Analyse(@"C:\Users\Zach\Downloads\VTS_02_1.VOB");
if (input == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Failed to read input file!");
    Console.ResetColor();
    return;
}

FFMpeg ffmpeg = new();
var arguments = ffmpeg.Transcode(@"C:\Users\Zach\Downloads\Test.mp4")
    .AddVideoStreams(
        input,
        new VideoStreamOptions() {
            Codec = Codecs.Libx264.SetCRF(16)
        })
    .AddAudioStream(
        input.AudioStreams[0],
        new AudioStreamOptions() {
            Codec = Codecs.AAC,
            Language = Language.FromPart2("eng")
        });

bool result = arguments
    .NotifyOnProgress((double percent) =>
    {
        Console.WriteLine($"Progress: {percent:0}%");
    })
    .SetLogPath(@"C:\Users\Zach\Downloads\TestLog.txt")
    .SetOverwrite(false)
    .Run();

if (!result)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("FFMpeg encountered an error!");
    Console.ResetColor();
    return;
}
Console.WriteLine("Success!");

ffmpeg.Snapshot(@"C:\Users\Zach\Downloads\Test.mp4", 0, @"C:\Users\Zach\Downloads\Snapshot_frame_0.png").Run();