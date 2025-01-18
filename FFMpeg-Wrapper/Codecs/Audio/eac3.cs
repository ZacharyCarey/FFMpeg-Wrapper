using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Codecs.Audio {
    public class eac3 : AudioCodec {
        string Codec.Name => "eac3";

        IEnumerable<string> Codec.GetArguments() {
            yield break;
        }
    }
}
