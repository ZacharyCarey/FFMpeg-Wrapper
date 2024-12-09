using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Codecs {

    public interface Codec {
        internal string Name { get; }
        internal IEnumerable<string> GetArguments(string streamSpecifier);
    }

    public interface VideoCodec : Codec {

    }

    public interface AudioCodec : Codec {

    }

    public interface SubtitleCodec : Codec {

    }
}
