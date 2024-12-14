using CLI_Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg
{
    public class FFMpeg
    {

        bool IsCommandLine;
        string NameOrPath;

        /// <summary>
        /// Will attempt to find the .exe from the PATH environment variable
        /// </summary>
        public FFMpeg()
        {
            IsCommandLine = true;
            NameOrPath = "ffmpeg.exe";
        }

        /// <summary>
        /// Will use the given path to ffmpeg.exe to execute commands
        /// </summary>
        /// <param name="exePath"></param>
        public FFMpeg(string exePath)
        {
            IsCommandLine = false;
            NameOrPath = exePath;
        }

        internal CLI GetCLI()
        {
            if (IsCommandLine)
            {
                return CLI.RunPathVariable(NameOrPath);
            }
            else
            {
                return CLI.RunExeFile(NameOrPath);
            }
        }

        public TranscodeArguments Transcode(string outputFile, OutputFileOptions? fileArgs = null)
        {
            if (fileArgs == null) fileArgs = new();
            fileArgs.FilePath = outputFile;
            return new TranscodeArguments(this, fileArgs);
        }

        /// <summary>
        /// Returns a set of arguments that will create a snapshot at the desired frame
        /// </summary>
        /// <returns></returns>
        public FFMpegArgs Snapshot(string inputFile, uint frameIndex, string outputFile)
        {
            return new InternalArgs(this,
                $"-i \"{inputFile}\"",
                $"-vf \"select=eq(n\\,{frameIndex})\"",
                "-frames:v 1",
                $"\"{outputFile}\""
            );
        }

        public FFMpegArgs Snapshot(string inputFile, TimeSpan time, string outputFile)
        {
            return new InternalArgs(this,
                $"-ss {time}",
                $"-i \"{inputFile}\"",
                "-frames:v 1",
                $"\"{outputFile}\""
            );
        }

    }
}
