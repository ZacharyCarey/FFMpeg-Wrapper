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

    }

    public interface Filter
    {
        internal string Name { get; }
        internal IEnumerable<string> GetArguments(string streamSpecifier);
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
