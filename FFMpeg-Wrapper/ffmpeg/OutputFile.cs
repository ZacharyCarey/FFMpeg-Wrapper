using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg {
    public class OutputFile {
        /// <summary>
        /// <para>
        /// Decodes but discards input until the timestamps reach <see cref="StartTime"/>.
        /// In most formats it is not possible to seek exactly, so ffmpeg will
        /// seek to the closest seek point BEFORE <see cref="StartTime"/>/>.
        /// </para>
        /// <para>
        /// When transcoding, this extra segment between the seek point and <see cref="StartTime"/>
        /// will be decoded but discarded. When doing a stream copy it will be preserved.
        /// </para>
        /// </summary>
        public TimeSpan? StartTime = null;

        /// <summary>
        /// Stop writing the output at <see cref="StopTime"/>.
        /// This option and <see cref="Duration"/> are mutually exclusive,
        /// and <see cref="Duration"/> has priority.
        /// </summary>
        public TimeSpan? StopTime = null;

        /// <summary>
        /// Stop writing the output after its duration reaches <see cref="Duration"/>.
        /// This option and <see cref="StopTime"/> are mutually exclusive,
        /// and <see cref="Duration"/> has priority.
        /// </summary>
        public TimeSpan? Duration = null;

        /// <summary>
        /// Set the file size limit, expressed in bytes. No further chunk of bytes is written
        /// after the limit is exceeded. The size of the output file is slightly more than the
        /// requested file size.
        /// </summary>
        public long? FileSizeLimit = null;

        /// <summary>
        /// The only thing I know of that uses this is when playing the file in
        /// VLC this is the text that will initially popup on the screen
        /// </summary>
        public string? Title = null;

        string FilePath;

        
        public OutputFile(string filePath) {
            this.FilePath = filePath;
        }

        internal IEnumerable<string> GetArguments() {
            if (StartTime != null) yield return $"-ss {StartTime.Value.GetFFMpegFormat()}";
            if (StopTime != null) yield return $"-to {StopTime.Value.GetFFMpegFormat()}";
            if (Duration != null) yield return $"-t {Duration.Value.GetFFMpegFormat()}";
            if (FileSizeLimit != null) yield return $"-fs {FileSizeLimit}";
            if (Title != null) yield return $"-metadata title=\"{Utils.GetEscapedString(Title)}\"";

            string ext = Path.GetExtension(FilePath).ToLower();
            switch (ext)
            {
                case ".m4v":
                case ".mov":
                case ".m4a":
                case ".mp4":
                    // I figured there wasn't much of an impact on processing time
                    // so it was worth just always using this flag
                    yield return "-movflags +faststart";
                    break;
                default:
                    break;
            }

            yield return $"\"{this.FilePath}\"";
        }
    }
}
