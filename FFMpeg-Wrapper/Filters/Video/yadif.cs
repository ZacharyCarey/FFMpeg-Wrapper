using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Filters.Video {
    public class yadif : VideoFilter {
        string Filter.Name => "yadif";

        IEnumerable<string> Filter.GetArguments(string streamSpecifier) {
            yield break;
        }
    }
}
