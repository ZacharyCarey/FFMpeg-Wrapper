using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.Codecs.Video {
    public class libsvtav1 : VideoCodec {

        string Codec.Name => "libsvtav1";
        int? Crf = null;
        int? Preset = null;
        bool? Tune = null;
        int? FilmGrain = null;
        bool FilmGrainDenoise = true;
        int? FastDecode = null;

        IEnumerable<string> Codec.GetArguments() {
            if (Preset != null) yield return $"-preset {Preset}";
            if (Crf != null) yield return $"-crf {Crf}";
            
            // The following are special options that require the use of -svtav1-params
            List<string> encoderParams = new();
            if (Tune != null) encoderParams.Add($"tune={(Tune.Value ? 1 : 0)}");
            if (FilmGrain != null)
            {
                encoderParams.Add($"film-grain={FilmGrain}");
                encoderParams.Add($"film-grain-denoise={(FilmGrainDenoise ? 1 : 0)}");
            }
            if (FastDecode != null) encoderParams.Add($"fast-decode={FastDecode}");

            if (encoderParams.Count > 0)
            {
                yield return $"-svtav1-params {string.Join(':', encoderParams)}";
            }
        }

        /// <summary>
        /// A value between 0-63 that in general refers to the encoding quality.
        /// 
        /// Much like CRF in x264 and x265, this rate control method tries to ensure that every frame gets the 
        /// number of bits it deserves to achieve a certain (perceptual) quality level.
        /// 0 = lossless, 35 = default, 63 = worst quality.
        /// A lower value is higher quality, but larger file size.
        /// A higher value is lower quality, but smaller file size.
        /// </summary>
        /// <param name="crf"></param>
        /// <returns></returns>
        public libsvtav1 SetCRF(int crf) {
            if (crf < 0 || crf > 63)
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

        /// <summary>
        /// A value between 0-8, a trade-off between encoding speed and compression efficiency.
        /// 0 = Slowest with best quality
        /// 8 = Fastest with lowest quality
        /// </summary>
        /// <param name="preset"></param>
        /// <returns></returns>
        public libsvtav1 SetPreset(int preset) {
            if (preset < 0 || preset > 8)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Invalid preset value '{preset}'. Using default instead.");
                Console.ResetColor();
                this.Preset = null;
            } else
            {
                this.Preset = preset;
            }

            return this;
        }

        /// <summary>
        /// The encoder also supports tuning for visual quality (sharpness). The default value is true, which tunes the encoder for PSNR.
        /// False is optimized for subjective quality, which can result in a sharper image and higher psycho-visual fidelity.
        /// </summary>
        /// <param name="tune"></param>
        /// <returns></returns>
        public libsvtav1 SetTuning(bool tune) {
            this.Tune = tune;
            return this;
        }

        /// <summary>
        /// <para>
        /// A value from 1 to 50.
        /// An AV1 feature for preserving the look of grainy video while spending very little bitrate to do so.
        /// The grain is removed from the image with denoising, its look is approximated and synthesized, and then added on top of the video at decode-time as a filter.
        /// Higher numbers correspond to higher levels of denoising for the grain synthesis process and thus a higher amount of grain.
        /// </para>
        /// 
        /// <para>
        /// The grain denoising process can remove detail as well, especially at the high values that are required to preserve the look of very grainy films. 
        /// This can be mitigated with the filmGrainDenoise=false option. 
        /// While by default the denoised frames are passed on to be encoded as the final pictures (filmGrainDenoise=true), 
        /// turning this off will lead to the original frames to be used instead.
        /// </para>
        /// </summary>
        /// <returns></returns>
        public libsvtav1 SetFilmGrain(int filmGrain, bool filmGrainDenoise = true) {
            if (filmGrain < 1 || filmGrain > 50)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Invalid film-grain value '{filmGrain}'. Using default instead.");
                Console.ResetColor();
                this.FilmGrain = null;
            } else
            {
                this.FilmGrain = filmGrain;
            }

            return this;
        }

        /// <summary>
        /// tuning the encoder to produce bitstreams that are faster (less CPU intensive) to decode, similar to the fastdecode tune in x264 and x265.
        /// the option accepts an integer from 1 to 3, with higher numbers resulting in easier-to-decode video.
        /// decoder tuning is only supported for presets from 5 to 10, and the level of decoder tuning varies between presets.
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public libsvtav1 SetFastDecode(int speed) {
            if (speed < 1 || speed > 3)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Invalid fast-decode value '{speed}'. Using default instead.");
                Console.ResetColor();
                this.FastDecode = null;
            } else
            {
                this.FastDecode = speed;
            }

            return this;
        }
    }
}
