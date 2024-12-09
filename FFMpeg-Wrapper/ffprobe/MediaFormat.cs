using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffprobe
{
    public class MediaFormat
    {
        public TimeSpan Duration { get; set; }
        public TimeSpan StartTime { get; set; }
        public string FormatName { get; set; } = null!;
        public string FormatLongName { get; set; } = null!;
        public int StreamCount { get; set; }
        public double ProbeScore { get; set; }
        public double BitRate { get; set; }
        public Dictionary<string, string>? Tags { get; set; }
    }
}
