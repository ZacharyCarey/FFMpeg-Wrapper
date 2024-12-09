using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper {
    internal class CliParser {
        private static readonly Regex ProgressRegex = new(@"time=(\d\d:\d\d:\d\d.\d\d?)", RegexOptions.Compiled);
        private static readonly Regex DurationRegex = new(@"Duration=(\d\d:\d\d:\d\d.\d\d?)", RegexOptions.Compiled);

        bool readingInputFile = false;
        TimeSpan? TotalDuration = null;

        public event EventHandler<double>? OnPercentProgress;
        public event EventHandler<TimeSpan>? OnTimeProgress;

        internal void ForceUpdateComplete() {
            this.OnPercentProgress?.Invoke(this, 100.0);
            if (TotalDuration != null) this.OnTimeProgress?.Invoke(this, TotalDuration.Value);
        }

        internal void ParseStdOutput(object? sender, string line) {

        }

        internal void ParseStdError(object? sender, string line) {
            if (line.StartsWith("frame"))
            {
                readingInputFile = false;

                var match = ProgressRegex.Match(line);
                if (match.Success)
                {
                    TimeSpan time = TimeSpan.Parse(match.Groups[1].Value);
                    OnTimeProgress?.Invoke(this, time);

                    if (TotalDuration != null)
                    {
                        var percentage = Math.Round(time.TotalSeconds / TotalDuration.Value.TotalSeconds * 100, 2);
                        OnPercentProgress?.Invoke(this, percentage);
                    }
                }
            } else if (line.StartsWith("Input #"))
            {
                readingInputFile = true;
            } else if (readingInputFile)
            {
                if (line.StartsWith("Stream") || line.StartsWith("Output"))
                {
                    readingInputFile = false;
                } else
                {
                    var match = DurationRegex.Match(line);
                    if (match.Success)
                    {
                        TimeSpan duration = TimeSpan.Parse(match.Groups[1].Value);
                        if (TotalDuration == null || duration > TotalDuration)
                        {
                            this.TotalDuration = duration;
                        }
                    }
                }
            }
        }
    }
}
