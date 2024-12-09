using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Codecs.Audio {
    public class aac : AudioCodec {
        string Codec.Name => "aac";

        IEnumerable<string> Codec.GetArguments(string streamSpecifier) {
            yield break;
        }
    }
}
