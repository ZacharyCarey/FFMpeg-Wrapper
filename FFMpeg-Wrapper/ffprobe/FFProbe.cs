using CLI_Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffprobe
{
    public class FFProbe
    {

        bool IsCommandLine;
        string NameOrPath;

        /// <summary>
        /// Will attempt to find the .exe from the PATH environment variable
        /// </summary>
        public FFProbe()
        {
            IsCommandLine = true;
            NameOrPath = "ffprobe.exe";
        }

        /// <summary>
        /// Will use the given path to ffmpeg.exe to execute commands
        /// </summary>
        /// <param name="exePath"></param>
        public FFProbe(string exePath)
        {
            IsCommandLine = false;
            NameOrPath = exePath;
        }

        private CLI GetCLI()
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

        /// <summary>
        /// It's recommended to only count frames on files <10MB. Larger than that will 
        /// start taking a significant amount of time to process.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="countFrames"></param>
        /// <returns></returns>
        public MediaAnalysis? Analyse(string filePath, bool countFrames = false) {
            CLI cli = GetCLI();
            cli.AddArguments("-loglevel error", "-print_format json", "-show_format -sexagesimal", "-show_streams", "-show_chapters");
            if (countFrames) cli.AddArgument("-count_frames");
            cli.AddArgument($"\"{filePath}\"");
            
            CliResult result = cli.Run();
            if (result.ExitCode != 0) return null;

            string json = string.Join("", result.OutputData);
            var analysis = JsonSerializer.Deserialize<FFProbeAnalysis>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (analysis?.Format == null) return null;

            analysis.ErrorData = result.ErrorData;
            return new MediaAnalysis(analysis, filePath);
        }

        /// <summary>
        /// A shortcut method for just getting the number of frames.
        /// Calls Analyse() on the path and returns the number of
        /// frames from the primary video stream, if any.
        /// 
        /// CountFrames=true is only recommended to only run on files < 10MB. Larger files
        /// will start to take a significant amount of time for FFProbe to read the entire file.
        /// 
        /// CountFrames=false will only look for the nb_frames flag in the file itself. This will likely 
        /// be present if the file was processed by ffmpeg, but is not required to be there so
        /// sometimes CountFrames=true is the only way to accurately get the number of frames.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public uint? GetNumberOfFrames(string filePath, bool countFrames) {
            MediaAnalysis? analysis = Analyse(filePath, countFrames);
            return analysis?.PrimaryVideoStream?.NumberOfFrames;
        }
    }
}
