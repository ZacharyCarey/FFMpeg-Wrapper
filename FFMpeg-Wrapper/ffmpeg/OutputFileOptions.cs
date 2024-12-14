using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg {
    public class OutputFileOptions {
        internal string? SeekTime = null;
        internal string? ToTime = null;
        internal string? Duration = null;
        long? LimitSize = null;
        internal string FilePath;

        /// <summary>
        /// These MUST be added BEFORE the output file
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<string> GetArguments() {
            if (SeekTime != null) yield return $"-ss {SeekTime}";
            if (ToTime != null) yield return $"-to {ToTime}";
            if (Duration != null) yield return $"-t {Duration}";
            if (LimitSize != null) yield return $"-fs {LimitSize}";
        }

        /// <summary>
        /// <para>
        /// Decodes but discards input until the timestamps reach <paramref name="position"/>.
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
        public OutputFileOptions SetStartTime(TimeSpan position) {
            this.SeekTime = position.GetFFMpegFormat();
            return this;
        }

        /// <summary>
        /// Stop writing the output at <paramref name="position"/>.
        /// This option and <see cref="SetDuration(TimeSpan)"/> are mutually exclusive,
        /// and <see cref="SetDuration(TimeSpan)"/> has priority.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public OutputFileOptions SetStopTime(TimeSpan position) {
            this.ToTime = position.GetFFMpegFormat();
            return this;
        }

        /// <summary>
        /// Stop writing the output after its duration reaches <paramref name="duration"/>.
        /// This option and <see cref="SetStopTime(TimeSpan)"/> are mutually exclusive,
        /// and <see cref="SetDuration(TimeSpan)"/> has priority.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public OutputFileOptions SetDuration(TimeSpan duration) {
            this.Duration = duration.GetFFMpegFormat();
            return this;
        }

        /// <summary>
        /// Set the file size limit, expressed in bytes. No further chunk of bytes is written
        /// after the limit is exceeded. The size of the output file is slightly more than the
        /// requested file size.
        /// </summary>
        /// <param name="sizeInBytes"></param>
        /// <returns></returns>
        public OutputFileOptions SetFileSizeLimit(long sizeInBytes) {
            LimitSize = sizeInBytes;
            return this;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object? obj) {
            if (obj == null) return false;
            if (obj is OutputFileOptions other)
            {
                return (FilePath == other.FilePath)
                    && (SeekTime == other.SeekTime)
                    && (ToTime == other.ToTime)
                    && (Duration == other.Duration)
                    && (LimitSize == other.LimitSize);
            } else
            {
                return false;
            }
        }

    }
}
