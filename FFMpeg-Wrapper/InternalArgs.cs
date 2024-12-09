using FFMpeg_Wrapper.ffprobe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper {
    internal class InternalArgs : FFMpegArgs {
        string[] Arguments;

        internal InternalArgs(FFMpeg core, params string[] args) : base(core) {
            this.Arguments = args;
        }

        protected override IEnumerable<string> GetArguments() {
            return Arguments;
        }
    }
}
