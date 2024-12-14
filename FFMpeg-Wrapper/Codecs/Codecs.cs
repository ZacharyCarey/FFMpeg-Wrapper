using FFMpeg_Wrapper.Codecs.Audio;
using FFMpeg_Wrapper.Codecs.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Codecs {
    public static class Codecs {
        // Video codecs
        public static libx264 Libx264 => new();
        public static libsvtav1 LibSvtAV1 => new();

        // Audio codecs
        public static aac AAC => new();
        public static ac3 AC3 => new();

        // General codecs
        public static CopyCodec Copy => new();
    }
}
