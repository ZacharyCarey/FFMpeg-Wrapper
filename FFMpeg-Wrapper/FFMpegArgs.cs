using CLI_Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper {
    public abstract class FFMpegArgs {

        private Action<TimeSpan>? OnTimeProgress = null;
        private Action<double>? OnPercentProgress = null;

        protected FFMpeg Core { get; }
        string? LogPath = null;
        bool Overwrite = true;

        protected FFMpegArgs(FFMpeg core) {
            this.Core = core;
        }

        protected abstract IEnumerable<string> GetArguments();

        /// <summary>
        /// Register an action that will be invoked during the ffmpeg processing when a percentage is calculated.
        /// The double parameter is a value from 0.0-100.0 indicating the estimated percentage complete.
        /// </summary>
        /// <param name="onPercentageProgress"></param>
        /// <returns></returns>
        public FFMpegArgs NotifyOnProgress(Action<double> callback) {
            this.OnPercentProgress = callback;
            return this;
        }

        /// <summary>
        /// Register an action that will be invoked during the ffmpeg processing.
        /// The TimeSpan parameter is the current time of the output file that ffmpeg
        /// is currently processing.
        /// </summary>
        /// <param name="onTimeProgress"></param>
        /// <returns></returns>
        public FFMpegArgs NotifyOnProgress(Action<TimeSpan> callback) {
            this.OnTimeProgress = callback;
            return this;
        }

        /// <inheritdoc cref="CLI.SetLogPath(string)"/>
        public FFMpegArgs SetLogPath(string path) {
            LogPath = path;
            return this;
        }

        public FFMpegArgs SetOverwrite(bool allow) {
            Overwrite = allow;
            return this;
        }

        public bool Run() {
            CLI cli = CreateCLI();
            CliParser parser = CreateParser(cli);
            CliResult result = cli.Run();
            parser.ForceUpdateComplete();
            return ParseResult(result);
        }

        public async Task<bool> RunAsync() {
            CLI cli = CreateCLI();
            CliParser parser = CreateParser(cli);
            CliResult result = await cli.RunAsync();
            parser.ForceUpdateComplete();
            return ParseResult(result);
        }

        private CLI CreateCLI() {
            CLI cli = Core.GetCLI();
            if (LogPath != null) cli.SetLogPath(LogPath);

            cli.AddArgument("-nostdin");
            cli.AddArgument(Overwrite ? "-y" : "-n");
            cli.AddArgument($"-abort_on empty_output");
            cli.AddArguments(this.GetArguments());

            return cli;
        }

        private CliParser CreateParser(CLI cli) {
            CliParser parser = new();
            if (this.OnPercentProgress != null) parser.OnPercentProgress += (sender, value) => this.OnPercentProgress(value);
            if (this.OnTimeProgress != null) parser.OnTimeProgress += (sender, value) => this.OnTimeProgress(value);
            cli.OutputDataReceived += parser.ParseStdOutput;
            cli.ErrorDataReceived += parser.ParseStdError;

            return parser;
        }

        private bool ParseResult(CliResult result) {
            if (result.Exception != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"FFMpeg encountered an exception: {result.Exception.Message}");
                Console.ResetColor();
                return false;
            } else if (result.ExitCode != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"FFMpeg returned an error code: {result.ExitCode}");
                Console.ResetColor();
                return false;
            } else
            {
                // Force error in certain situations
                foreach (var line in result.ErrorData)
                {
                    if (line.StartsWith("Error opening output file"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"FFMpeg failed to open output file.");
                        Console.ResetColor();
                        return false;
                    }
                }

                // No errors were found
                return true;
            }
        }
    }
}
