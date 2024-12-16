using FFMpeg_Wrapper.ffprobe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg {
    public class FileInput : FFMpegInput {
        /// <summary>
        /// <para>
        /// Seeks the input file to <see cref="StartTime"/>.
        /// In most formats it is not possible to seek exactly, so ffmpeg will
        /// seek to the closest seek point BEFORE <see cref="StartTime"/>.
        /// </para>
        /// <para>
        /// When transcoding, this extra segment between the seek point and <see cref="StartTime"/>
        /// will be decoded but discarded. When doing a stream copy it will be preserved.
        /// </para>
        /// </summary>
        public TimeSpan? StartTime = null;

        /// <summary>
        /// Stop reading the input at this timestamp.
        /// This option and <see cref="Duration"/> are mutually exclusive,
        /// and <see cref="Duration"/> has priority.
        /// </summary>
        public TimeSpan? StopTime = null;

        /// <summary>
        /// Limits the <see cref="Duration"/> of data read from the input file.
        /// This option and <see cref="StopTime"/> are mutually exclusive,
        /// and <see cref="Duration"/> has priority.
        /// </summary>
        public TimeSpan? Duration = null;

        internal string FilePath;
        internal TimeSpan FileLength;

        TimeSpan FFMpegInput.Duration => this.FileLength;

        public FileInput(string filePath, TimeSpan fileLength) {
            this.FilePath = filePath;
            this.FileLength = fileLength;
        }

        public FileInput(MediaAnalysis file) {
            this.FilePath = file.FilePath;
            this.FileLength = file.Duration;
        }

        void IFFMpegArgs.Begin() {

        }

        IEnumerable<string> IFFMpegArgs.GetArguments() {
            if (StartTime != null) yield return $"-ss {StartTime.Value.GetFFMpegFormat()}";
            if (StopTime != null) yield return $"-to {StopTime.Value.GetFFMpegFormat()}";
            if (Duration != null) yield return $"-t {Duration.Value.GetFFMpegFormat()}";
            yield return $"-i \"{this.FilePath}\"";
        }

        void IFFMpegArgs.Cleanup() {

        }
    }
}
