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

        public MediaAnalysis? Analyse(string filePath) {
            CLI cli = GetCLI();
            cli.AddArguments("-loglevel error", "-print_format json", "-show_format -sexagesimal", "-show_streams", "-show_chapters", $"\"{filePath}\"");
            
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
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public uint? GetNumberOfFrames(string filePath) {
            MediaAnalysis? analysis = Analyse(filePath);
            return analysis?.PrimaryVideoStream?.NumberOfFrames;
        }
    }
}
