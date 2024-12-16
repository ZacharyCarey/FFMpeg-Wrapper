using FFMpeg_Wrapper.ffprobe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg
{
    internal class InternalArgs : FFMpegCliArgs
    {
        string[] Arguments;
        internal TimeSpan ProcessTime = new();
        string? TempFolder = null;

        protected override TimeSpan DetermineProcessTime() {
            return ProcessTime;
        }

        internal InternalArgs(FFMpeg core, params string[] args) : base(core)
        {
            Arguments = args;
        }

        protected override void Begin() {
            
        }

        protected override IEnumerable<string> GetArguments()
        {
            return Arguments;
        }

        protected override void Cleanup() {
            if (TempFolder != null)
            {
                try
                {
                    Directory.Delete(TempFolder, true);
                } catch (Exception) { }
            }
        }
    }
}
