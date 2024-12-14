using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Codecs.Audio {
    public class ac3 : AudioCodec {

        string Codec.Name => "ac3";

        IEnumerable<string> Codec.GetArguments() {
            yield break;
        }

    }
}
