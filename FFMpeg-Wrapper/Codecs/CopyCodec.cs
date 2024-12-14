using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Codecs {
    public class CopyCodec : VideoCodec, AudioCodec, SubtitleCodec {
        string Codec.Name => "copy";

        IEnumerable<string> Codec.GetArguments() {
            yield break;
        }
    }
}
