using FFMpeg_Wrapper.ffprobe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg {
    public class ConcatArguments : FFMpegArgs {

        TimeSpan Duration = new();
        string TempFolder;
        List<string> Arguments = new();
        OutputFileOptions OutputFile;
        List<string> FileData = new();
        int OutputStreamCount = 0;

        public ConcatArguments(FFMpeg core, OutputFileOptions outputFile) : base(core) {
            OutputFile = outputFile;

            string ext = Path.GetExtension(outputFile.FilePath).ToLower();
            switch (ext)
            {
                case ".m4v":
                case ".mov":
                case ".m4a":
                case ".mp4":
                    // I figured there wasn't much of an impact on processing time
                    // so it was worth just always using this flag
                    Arguments.Add("-movflags +faststart");
                    break;
                default:
                    break;
            }

            DirectoryInfo di = Directory.CreateTempSubdirectory();
            if (!di.Exists) throw new DirectoryNotFoundException();
            this.TempFolder = di.FullName;
        }

        protected override TimeSpan DetermineProcessTime() {
            return Duration;
        }

        protected override string FolderToClean() {
            return TempFolder;
        }

        protected override IEnumerable<string> GetArguments() {
            File.WriteAllLines(Path.Combine(this.TempFolder, "FileList.txt"), FileData);

            yield return "-f concat";
            yield return "-safe 0";
            yield return $"-i \"{Path.Combine(this.TempFolder, "FileList.txt")}\"";
            foreach(var arg in this.Arguments)
            {
                yield return arg;
            }
            foreach(var arg in this.OutputFile.GetArguments())
            {
                yield return arg;
            }
            yield return $"\"{this.OutputFile.FilePath}\"";
        }

        public ConcatArguments AddFile(MediaAnalysis file, TimeSpan? duration = null, TimeSpan? startTime = null, TimeSpan? stopTime = null) {
            this.Duration += file.Duration;
            
            string filePath = file.FilePath.Replace("'", "\\'");
            this.FileData.Add($"file '{filePath}'");

            if (duration != null) this.FileData.Add($"duration {duration.Value.GetFFMpegFormat()}");
            if (startTime != null) this.FileData.Add($"inpoint {startTime.Value.GetFFMpegFormat()}");
            if (stopTime != null) this.FileData.Add($"outpoint {stopTime.Value.GetFFMpegFormat()}");

            return this;
        }

        public ConcatArguments AddChapter(string ID, TimeSpan start, TimeSpan end) {
            this.FileData.Add($"chapter {ID} {start.GetFFMpegFormat()} {end.GetFFMpegFormat()}");
            return this;
        }

        /// <summary>
        /// Maps all video streams from an input file to the output. 
        /// The given options will be applies to each stream as well.
        /// For concat, streams with the same index will overwrite 
        /// previous settings so it is recommended to only add streams
        /// from the first appended file.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public ConcatArguments AddVideoStreams(MediaAnalysis inputFile, VideoStreamOptions? streamOptions, InputFileOptions? fileOptions) {
            return AddVideoStreams(inputFile.VideoStreams, streamOptions, fileOptions);
        }

        /// <summary>
        /// Maps all provided input video streams the output. 
        /// The given options will be applies to each stream as well.
        /// For concat, streams with the same index will overwrite 
        /// previous settings so it is recommended to only add streams
        /// from the first appended file.
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public ConcatArguments AddVideoStreams(IEnumerable<VideoStream> streams, VideoStreamOptions? streamOptions, InputFileOptions? fileOptions) {
            foreach (var stream in streams)
            {
                AddVideoStream(stream, streamOptions, fileOptions);
            }
            return this;
        }

        /// <summary>
        /// Maps the input video stream to the output file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public ConcatArguments AddVideoStream(VideoStream stream, VideoStreamOptions? streamOptions, InputFileOptions? fileOptions) {
            // Get the default options if none are provided. This is usually just specifying to use the "copy codec".
            if (streamOptions == null) streamOptions = new VideoStreamOptions();

            Arguments.Add($"-map 0:{stream.Index}");
            Arguments.AddRange(streamOptions.GetArguments($":{OutputStreamCount}"));
            OutputStreamCount++;

            return this;
        }

        /// <summary>
        /// Maps all audio streams from an input file to the output. 
        /// The given options will be applies to each stream as well.
        /// For concat, streams with the same index will overwrite 
        /// previous settings so it is recommended to only add streams
        /// from the first appended file.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public ConcatArguments AddAudioStreams(MediaAnalysis inputFile, AudioStreamOptions? streamOptions, InputFileOptions? fileOptions) {
            return AddAudioStreams(inputFile.AudioStreams, streamOptions, fileOptions);
        }

        /// <summary>
        /// Maps all provided input audio streams the output. 
        /// The given options will be applies to each stream as well.
        /// For concat, streams with the same index will overwrite 
        /// previous settings so it is recommended to only add streams
        /// from the first appended file.
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public ConcatArguments AddAudioStreams(IEnumerable<AudioStream> streams, AudioStreamOptions? streamOptions, InputFileOptions? fileOptions) {
            foreach (var stream in streams)
            {
                AddAudioStream(stream, streamOptions, fileOptions);
            }
            return this;
        }

        /// <summary>
        /// Maps the input audio stream to the output file.
        /// For concat, streams with the same index will overwrite 
        /// previous settings so it is recommended to only add streams
        /// from the first appended file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public ConcatArguments AddAudioStream(AudioStream stream, AudioStreamOptions? streamOptions, InputFileOptions? fileOptions) {
            // Get the default options if none are provided. This is usually just specifying to use the "copy codec".
            if (streamOptions == null) streamOptions = new AudioStreamOptions();

            Arguments.Add($"-map 0:{stream.Index}");
            Arguments.AddRange(streamOptions.GetArguments($":{OutputStreamCount}"));
            OutputStreamCount++;

            return this;
        }

        /// <summary>
        /// Maps all subtitle streams from an input file to the output. 
        /// The given options will be applies to each stream as well.
        /// For concat, streams with the same index will overwrite 
        /// previous settings so it is recommended to only add streams
        /// from the first appended file.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public ConcatArguments AddSubtitleStreams(MediaAnalysis inputFile, SubtitleStreamOptions? streamOptions, InputFileOptions? fileOptions) {
            return AddSubtitleStreams(inputFile.SubtitleStreams, streamOptions, fileOptions);
        }

        /// <summary>
        /// Maps all provided input subtitle streams the output. 
        /// The given options will be applies to each stream as well.
        /// For concat, streams with the same index will overwrite 
        /// previous settings so it is recommended to only add streams
        /// from the first appended file.
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public ConcatArguments AddSubtitleStreams(IEnumerable<SubtitleStream> streams, SubtitleStreamOptions? streamOptions, InputFileOptions? fileOptions) {
            foreach (var stream in streams)
            {
                AddSubtitleStream(stream, streamOptions, fileOptions);
            }
            return this;
        }

        /// <summary>
        /// Maps the input subtitle stream to the output file.
        /// For concat, streams with the same index will overwrite 
        /// previous settings so it is recommended to only add streams
        /// from the first appended file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public ConcatArguments AddSubtitleStream(SubtitleStream stream, SubtitleStreamOptions? streamOptions, InputFileOptions? fileOptions) {
            // Get the default options if none are provided. This is usually just specifying to use the "copy codec".
            if (streamOptions == null) streamOptions = new SubtitleStreamOptions();

            Arguments.Add($"-map 0:{stream.Index}");
            Arguments.AddRange(streamOptions.GetArguments($":{OutputStreamCount}"));
            OutputStreamCount++;

            return this;
        }
    }
}
