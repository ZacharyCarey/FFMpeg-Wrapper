using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper {
    internal static class Utils {

        /// <summary>
        /// Gets a FFMpeg-compatible string that represents the given timespan.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        internal static string GetFFMpegFormat(this TimeSpan time) {
            string negative = (time < TimeSpan.Zero) ? "-" : "";
            return $"{negative}{time:hh\\:mm\\:ss\\.ffff}";
        }

        internal static string GetEscapedString(string str) {
            str = str.Replace("\"", "\\\"");
            return str;
        }
    }
}
