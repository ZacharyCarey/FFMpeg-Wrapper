using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Filters.Video {
    public class overlay : VideoFilter {

        int CornerPadding = 10;

        string Filter.GetArguments() {
            return $"overlay=main_w-overlay_w-{CornerPadding}:main_h-overlay_h-{CornerPadding}";
        }

        /// <summary>
        /// The number of pixels off the edge of the screen the overlay should be placed
        /// </summary>
        /// <param name="padding"></param>
        /// <returns></returns>
        public overlay SetCornerPadding (int padding) {
            this.CornerPadding = padding;
            return this;
        }
    }
}
