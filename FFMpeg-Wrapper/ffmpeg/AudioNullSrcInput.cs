using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg {

    /// <summary>
    /// Creates an "anullsrc" audio input. This acts as an empty/null audio source input.
    /// </summary>
    public class AudioNullSrcInput : FFMpegInput {
        private readonly TimeSpan Duration;
        TimeSpan FFMpegInput.Duration => this.Duration;

        void IFFMpegArgs.Begin() {
            
        }

        void IFFMpegArgs.Cleanup() {
            
        }

        IEnumerable<string> IFFMpegArgs.GetArguments() {
            yield return "-f lavfi";
            yield return $"-t {Duration.GetFFMpegFormat()}";
            yield return "-i anullsrc";
        }

        public AudioNullSrcInput(TimeSpan duration) {
            this.Duration = duration;
        }
    }
}
