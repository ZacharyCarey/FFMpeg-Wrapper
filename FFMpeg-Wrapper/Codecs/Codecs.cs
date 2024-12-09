using FFMpeg_Wrapper.Codecs.Audio;
using FFMpeg_Wrapper.Codecs.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Codecs {
    public static class Codecs {
        public static libx264 Libx264 => new();
        public static aac AAC => new();
        public static CopyCodec Copy => new();
    }
}
