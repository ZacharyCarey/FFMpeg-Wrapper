using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Codecs.Video {
    public class h264_nvenc : VideoCodec {
        string Codec.Name => "h264_nvenc";

        IEnumerable<string> Codec.GetArguments() {
            yield break;
        }
    }
}
