using FFMpeg_Wrapper.ffprobe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg
{
    internal class InternalArgs : FFMpegArgs
    {
        string[] Arguments;
        internal TimeSpan ProcessTime = new();

        protected override TimeSpan DetermineProcessTime() {
            return ProcessTime;
        }

        internal InternalArgs(FFMpeg core, params string[] args) : base(core)
        {
            Arguments = args;
        }

        protected override IEnumerable<string> GetArguments()
        {
            return Arguments;
        }
    }
}
