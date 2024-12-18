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
        public static overlay Overlay => new overlay();

        /// <inheritdoc cref="scale(ScaleResolution)"/>
        public static scale Scale(ScaleResolution resolution) {
            return new scale(resolution);
        }

        /// <inheritdoc cref="Scale(int, int)"/>
        public static scale Scale(int width, int height) {
            return new scale(width, height);
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
