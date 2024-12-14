using FFMpeg_Wrapper.Filters.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Filters
{
    public static class Filters
    {

        public static yadif Yadif => new yadif();
        public static bwdif Bwdif => new bwdif();
        //public static scale Scale => new scale();

        /// <summary>
        /// Sets the vertical resolution while maintaining the aspect ratio of the file.
        /// </summary>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static scale Scale(ScaleResolution resolution) {
            return new scale(resolution);
        }
    }

    public interface Filter
    {
        internal string GetArguments();
    }

    public interface VideoFilter : Filter
    {

    }

    public interface AudioFilter : Filter
    {

    }

    public interface SubtitleFilter : Filter
    {

    }
}
