using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Filters.Video {
    public class scale : VideoFilter {
        ScaleResolution Resolution;

        /// <summary>
        /// Sets the vertical resolution while maintaining the aspect ratio of the file.
        /// </summary>
        /// <param name="resolution"></param>
        public scale(ScaleResolution resolution) {
            this.Resolution = resolution;
        }

        string Filter.GetArguments() {
            // -2 forces the horizontal resolution to be divisible by two,
            // which is required by some encoders
            return $"scale=-2:{(int)Resolution}";
        }
    }

    /// <summary>
    /// If original content is in a wide format it will be automatically
    /// scaled to keep the same aspect ratio. These are simply the most
    /// common resolutions for the given quality.
    /// </summary>
    public enum ScaleResolution {
        UHD_7680x4320 = 4320,
        UHD_3840x2160 = 2160,
        HD_1920x1080 = 1080,
        HD_1280x720 = 720,
        SD_720x480 = 480
    }
}
