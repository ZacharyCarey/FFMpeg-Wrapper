using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffprobe {
    public class SubtitleStream : MediaStream {

        internal SubtitleStream(MediaAnalysis source) : base(source) {
            
        }

    }
}
