# [FFMpeg-Wrapper](https://www.nuget.org/packages/FFMpeg-Wrapper/) 
A wrapper for FFMpeg CLI for ease-of-use when running commands.

[![NuGet Version](https://img.shields.io/nuget/v/FFMpeg-Wrapper)](https://www.nuget.org/packages/FFMpeg-Wrapper/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/FFMpeg-Wrapper)](https://www.nuget.org/packages/FFMpeg-Wrapper/)
[![GitHub issues](https://img.shields.io/github/issues/ZacharyCarey/FFMpeg-Wrapper)](https://github.com/ZacharyCarey/FFMpeg-Wrapper/issues)
[![GitHub stars](https://img.shields.io/github/stars/ZacharyCarey/FFMpeg-Wrapper)](https://github.com/ZacharyCarey/FFMpeg-Wrapper/stargazers)
[![GitHub](https://img.shields.io/github/license/ZacharyCarey/FFMpeg-Wrapper)](https://github.com/ZacharyCarey/FFMpeg-Wrapper/blob/master/LICENSE.txt)
[![GitHub code contributors](https://img.shields.io/github/contributors/ZacharyCarey/FFMpeg-Wrapper)](https://github.com/ZacharyCarey/FFMpeg-Wrapper/graphs/contributors)

# Features
- one
- two

# Example
This example uses most of the major features of the wrapper. It runs FFMpeg command using the PATH environment variable to find the .exe file.

```c#
using FFMpeg_Wrapper;
using FFMpeg_Wrapper.Codecs;
using FFMpeg_Wrapper.ffprobe;
using Iso639;

// Run FFProbe on the input to determine what streams are available for processing.
MediaAnalysis? input = new FFProbe().Analyse(@"C:\Users\Zach\Downloads\VTS_02_1.VOB");
if (input == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Failed to read input file!");
    Console.ResetColor();
    return;
}

// This creates a new instance by finding the .exe in the PATH environment variable
// Optionally you can pass it the ffmpeg.exe path directly
FFMpeg ffmpeg = new();

// Creates the necessary command line arguments for FFMpeg. The arguments themselves are
// invisible to the user, and only a higher-level understanding of how the streams should be
// remuxed is required.
var arguments = ffmpeg.Transcode(@"C:\Users\Zach\Downloads\Test.mp4") // Start a transcode command and set our desired output file
    .AddVideoStreams( // Add all video streams
        input, // getting the streams from our only input file
        new VideoStreamOptions() {
            Codec = Codecs.Libx264.SetCRF(16) // use the libx264 transcoder with -crf 16
        })
    .AddAudioStream( // Add a single audio stream
        input.AudioStreams[0], // Select the first stream from our only input file
        new AudioStreamOptions() {
            Codec = Codecs.AAC, // Use the AAC codec
            Language = Language.FromPart2("eng") // Tag the track language as "English"
        });

// Now run the FFMpeg arguments on the command line
bool result = arguments
    .NotifyOnProgress((double percent) =>
    {
        // This gets ran when the percentage completion is updated.
        // Useful for using IProgress<> in UI applications or async operation
        Console.WriteLine($"Progress: {percent:0}%");
    })
    .SetLogPath(@"C:\Users\Zach\Downloads\TestLog.txt") // Set optional log path. Will print the full command ran, the FFMpeg output, and the exit code or exception of the process.
    .SetOverwrite(false) // Defaults to true, when false the program will exit with an error if the output file already exists.
    .Run(); // Blocking call to run the command. Use RunAsync() for an async call.

// Check the result of the command
if (!result)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("FFMpeg encountered an error!");
    Console.ResetColor();
    return;
}
Console.WriteLine("Success!");

// Example of getting a screenshot at a specific frame or time
// Same general layout, except the .Snapshot() function already creates all the arguments,
// so the only thing left to configure is log path, overwrite, or NotifyOnProgress (same as previous example)
int frameIndex = 0;
var snapshotArgs = ffmpeg.Snapshot(@"C:\Users\Zach\Downloads\Test.mp4", frameIndex, @"C:\Users\Zach\Downloads\Snapshot_frame_0.png");
snapshotArgs.Run();
```