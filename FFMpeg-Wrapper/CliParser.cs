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
        private static readonly Regex DurationRegex = new(@"Duration: (\d\d:\d\d:\d\d.\d\d?)", RegexOptions.Compiled);

        readonly TimeSpan TotalDuration;
        int lastProgressInt = -1;

        public event EventHandler<double>? OnPercentProgress;
        public event EventHandler<TimeSpan>? OnTimeProgress;

        internal CliParser(TimeSpan completionTime) {
            this.TotalDuration = completionTime;
        }

        internal void ForceUpdateComplete() {
            this.OnPercentProgress?.Invoke(this, 100.0);
            this.OnTimeProgress?.Invoke(this, TotalDuration);
            lastProgressInt = 100;
        }

        internal void ParseStdOutput(object? sender, string line) {

        }

        internal void ParseStdError(object? sender, string line) {
            if (line.StartsWith("frame"))
            {
                var match = ProgressRegex.Match(line);
                if (match.Success)
                {
                    TimeSpan time = TimeSpan.Parse(match.Groups[1].Value);
                    OnTimeProgress?.Invoke(this, time);

                    var percentage = Math.Round(time.TotalSeconds / TotalDuration.TotalSeconds * 100, 2);
                    if ((int)percentage > lastProgressInt)
                    {
                        lastProgressInt = (int)percentage;
                        OnPercentProgress?.Invoke(this, percentage);
                    }
                }
            }
        }
    }
}
