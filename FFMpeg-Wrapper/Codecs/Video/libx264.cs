using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Codecs.Video {
    public class libx264 : VideoCodec {

        string Codec.Name => "libx264";
        int? Crf = null;

        IEnumerable<string> Codec.GetArguments(string streamSpecifier) {
            if (Crf != null) yield return $"-crf{streamSpecifier} {Crf}";
        }

        /// <summary>
        /// A value between 0-51 that in general refers to the encoding quality.
        /// 0 = lossless, 23 = default, 51 = worst quality.
        /// A lower value is higher quality, but larger file size.
        /// A higher value is lower quality, but smaller file size.
        /// A "sane" range is considered to be 17-28.
        /// 17/18 is considered nearly visually lossless, although lower quality videos (like an old DVD) may see
        /// some artifacting when the video is expanded to the large size of a TV
        /// </summary>
        /// <returns></returns>
        public libx264 SetCRF(int crf) {
            if (crf < 0 || crf > 51)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Invalid CRF value '{crf}'. Using default instead.");
                Console.ResetColor();
                this.Crf = null;
            } else
            {
                this.Crf = crf;
            }

            return this;
        }
    }
}
