using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Filters.Video {
    public class bwdif : VideoFilter {

        string Filter.GetArguments() {
            return "bwdif";
        }
    }
}
