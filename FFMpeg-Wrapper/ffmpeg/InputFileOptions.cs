using FFMpeg_Wrapper.ffprobe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg {
    public class InputFileOptions {

        internal string? SeekTime = null;
        internal string? ToTime = null;
        internal string? Duration = null;
        internal MediaAnalysis File;

        internal InputFileOptions DeepCopy() {
            InputFileOptions result = new();
            result.SeekTime = this.SeekTime;
            result.ToTime = this.ToTime;
            result.Duration = this.Duration;
            result.File = this.File;
            return result;
        }

        /// <summary>
        /// These MUST be added BEFORE -i
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<string> GetArguments() {
            if (SeekTime != null) yield return $"-ss {SeekTime}";
            if (ToTime != null) yield return $"-to {ToTime}";
            if (Duration != null) yield return $"-t {Duration}";
        }

        /// <summary>
        /// <para>
        /// Seeks the input file to <paramref name="position"/>.
        /// In most formats it is not possible to seek exactly, so ffmpeg will
        /// seek to the closest seek point BEFORE <paramref name="position"/>.
        /// </para>
        /// <para>
        /// When transcoding, this extra segment between the seek point and <paramref name="position"/>
        /// will be decoded but discarded. When doing a stream copy it will be preserved.
        /// </para>
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public InputFileOptions SetStartTime(TimeSpan position) {
            this.SeekTime = position.GetFFMpegFormat();
            return this;
        }

        /// <summary>
        /// Stop reading the input at <paramref name="position"/>.
        /// This option and <see cref="SetDuration(TimeSpan)"/> are mutually exclusive,
        /// and <see cref="SetDuration(TimeSpan)"/> has priority.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public InputFileOptions SetStopTime(TimeSpan position) {
            this.ToTime = position.GetFFMpegFormat();
            return this;
        }

        /// <summary>
        /// Limits the <paramref name="duration"/> of data read from the input file.
        /// This option and <see cref="SetStopTime(TimeSpan)"/> are mutually exclusive,
        /// and <see cref="SetDuration(TimeSpan)"/> has priority.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public InputFileOptions SetDuration(TimeSpan duration) {
            this.Duration = duration.GetFFMpegFormat();
            return this;
        }

        public override int GetHashCode() {
            return HashCode.Combine(File.FilePath, this.ToTime, this.SeekTime, this.Duration);
        }

        public override bool Equals(object? obj) {
            if (obj == null) return false;
            if (obj is InputFileOptions other)
            {
                return (File.FilePath == other.File.FilePath)
                    && (SeekTime == other.SeekTime)
                    && (ToTime == other.ToTime)
                    && (Duration == other.Duration);
            } else
            {
                return false;
            }
        }
    }
}
