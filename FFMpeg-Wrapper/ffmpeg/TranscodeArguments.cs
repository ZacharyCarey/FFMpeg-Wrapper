using CLI_Wrapper;
using FFMpeg_Wrapper.Codecs;
using FFMpeg_Wrapper.Filters;
using FFMpeg_Wrapper.ffprobe;
using Iso639;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg
{
    public class TranscodeArguments : FFMpegArgs
    {
        List<InputFileOptions> InputFiles = new();
        List<string> GlobalOptions = new();
        List<string> Arguments = new();
        OutputFileOptions OutputFile;
        List<MediaStream> OutputStreams = new();

        // TODO store applied codec options so if all video streams
        // have the same codec applied, the command cam be simplified to
        // "-c:v" instead of applying it seperately to each stream

        internal TranscodeArguments(FFMpeg core, OutputFileOptions outputFile) : base(core)
        {
            OutputFile = outputFile;

            string ext = Path.GetExtension(outputFile.FilePath).ToLower();
            switch(ext)
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
        }

        protected override TimeSpan DetermineProcessTime() {
            if (OutputFile.Duration != null)
            {
                // Duration takes priority over stop time
                return TimeSpan.Parse(OutputFile.Duration);
            } else
            {
                if (OutputFile.ToTime != null)
                {
                    TimeSpan startTime = new();
                    if (OutputFile.SeekTime != null)
                    {
                        startTime = TimeSpan.Parse(OutputFile.SeekTime);
                    }
                    return TimeSpan.Parse(OutputFile.ToTime) - startTime;
                }
            }

            // Duration will be determined by input files
            // OutputFile.ToTime is not defined, so the output length is uncapped.
            // There may still be output seeking
            TimeSpan LongestInput = new();
            foreach (var input in this.InputFiles)
            {
                if (input.Duration != null)
                {
                    TimeSpan duration = TimeSpan.Parse(input.Duration);
                    if (duration > LongestInput) LongestInput = duration;
                } else
                {
                    TimeSpan startTime = new();
                    if (input.SeekTime != null) startTime = TimeSpan.Parse(input.SeekTime);

                    TimeSpan duration;
                    if (input.ToTime != null)
                    {
                        duration = TimeSpan.Parse(input.ToTime) - startTime;
                    } else
                    {
                        duration = input.File.Duration - startTime;
                    }

                    if (duration > LongestInput) LongestInput = duration;
                }
            }

            if (OutputFile.SeekTime != null)
            {
                TimeSpan seekTime = TimeSpan.Parse(OutputFile.SeekTime);
                if (seekTime > LongestInput) return new TimeSpan();
                else return LongestInput - seekTime;
            } else
            {
                return LongestInput;
            }
        }

        protected override IEnumerable<string> GetArguments()
        {
            return GlobalOptions
                .Concat(InputFiles.SelectMany(file =>
                    file.GetArguments().Append($"-i \"{file.File.FilePath}\""))
                )
                .Concat(Arguments)
                .Concat(
                    OutputFile.GetArguments().Append($"\"{OutputFile.FilePath}\"")
                );
        }

        /// <summary>
        /// Maps all video streams from an input file to the output. 
        /// The given options will be applies to each stream as well.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public TranscodeArguments AddVideoStreams(MediaAnalysis inputFile, VideoStreamOptions? streamOptions, InputFileOptions? fileOptions)
        {
            return AddVideoStreams(inputFile.VideoStreams, streamOptions, fileOptions);
        }

        /// <summary>
        /// Maps all provided input video streams the output. 
        /// The given options will be applies to each stream as well.
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public TranscodeArguments AddVideoStreams(IEnumerable<VideoStream> streams, VideoStreamOptions? streamOptions, InputFileOptions? fileOptions)
        {
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
        public TranscodeArguments AddVideoStream(VideoStream stream, VideoStreamOptions? streamOptions, InputFileOptions? fileOptions)
        {
            // Get the default options if none are provided. This is usually just specifying to use the "copy codec".
            if (streamOptions == null) streamOptions = new VideoStreamOptions();

            int sourceIndex = GetInputIndex(stream.SourceFile, fileOptions);
            string outputStreamSpecifier = $":{OutputStreams.Count}";

            Arguments.Add($"-map {sourceIndex}:{stream.Index}");
            Arguments.AddRange(streamOptions.GetArguments(outputStreamSpecifier));
            OutputStreams.Add(stream);

            return this;
        }

        /// <summary>
        /// Maps all audio streams from an input file to the output. 
        /// The given options will be applies to each stream as well.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public TranscodeArguments AddAudioStreams(MediaAnalysis inputFile, AudioStreamOptions? streamOptions, InputFileOptions? fileOptions)
        {
            return AddAudioStreams(inputFile.AudioStreams, streamOptions, fileOptions);
        }

        /// <summary>
        /// Maps all provided input audio streams the output. 
        /// The given options will be applies to each stream as well.
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public TranscodeArguments AddAudioStreams(IEnumerable<AudioStream> streams, AudioStreamOptions? streamOptions, InputFileOptions? fileOptions)
        {
            foreach (var stream in streams)
            {
                AddAudioStream(stream, streamOptions, fileOptions);
            }
            return this;
        }

        /// <summary>
        /// Maps the input audio stream to the output file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public TranscodeArguments AddAudioStream(AudioStream stream, AudioStreamOptions? streamOptions, InputFileOptions? fileOptions)
        {
            // Get the default options if none are provided. This is usually just specifying to use the "copy codec".
            if (streamOptions == null) streamOptions = new AudioStreamOptions();

            int sourceIndex = GetInputIndex(stream.SourceFile, fileOptions);
            string outputStreamSpecifier = $":{OutputStreams.Count}";

            Arguments.Add($"-map {sourceIndex}:{stream.Index}");
            if (streamOptions != null) Arguments.AddRange(streamOptions.GetArguments(outputStreamSpecifier));
            OutputStreams.Add(stream);

            return this;
        }

        /// <summary>
        /// Maps all subtitle streams from an input file to the output. 
        /// The given options will be applies to each stream as well.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public TranscodeArguments AddSubtitleStreams(MediaAnalysis inputFile, SubtitleStreamOptions? streamOptions, InputFileOptions? fileOptions)
        {
            return AddSubtitleStreams(inputFile.SubtitleStreams, streamOptions, fileOptions);
        }

        /// <summary>
        /// Maps all provided input subtitle streams the output. 
        /// The given options will be applies to each stream as well.
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public TranscodeArguments AddSubtitleStreams(IEnumerable<SubtitleStream> streams, SubtitleStreamOptions? streamOptions, InputFileOptions? fileOptions)
        {
            foreach (var stream in streams)
            {
                AddSubtitleStream(stream, streamOptions, fileOptions);
            }
            return this;
        }

        /// <summary>
        /// Maps the input subtitle stream to the output file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOptions"></param>
        /// <param name="fileOptions"></param>
        /// <returns></returns>
        public TranscodeArguments AddSubtitleStream(SubtitleStream stream, SubtitleStreamOptions? streamOptions, InputFileOptions? fileOptions)
        {
            // Get the default options if none are provided. This is usually just specifying to use the "copy codec".
            if (streamOptions == null) streamOptions = new SubtitleStreamOptions();

            int sourceIndex = GetInputIndex(stream.SourceFile, fileOptions);
            string outputStreamSpecifier = $":{OutputStreams.Count}";

            Arguments.Add($"-map {sourceIndex}:{stream.Index}");
            if (streamOptions != null) Arguments.AddRange(streamOptions.GetArguments(outputStreamSpecifier));
            OutputStreams.Add(stream);

            return this;
        }

        private int GetInputIndex(MediaAnalysis inputFile, InputFileOptions? fileOptions)
        {
            if (fileOptions == null)
            {
                fileOptions = new();
            } else
            {
                fileOptions = fileOptions.DeepCopy();
            }
            fileOptions.File = inputFile;

            int index = InputFiles.IndexOf(fileOptions);
            if (index < 0)
            {
                index = InputFiles.Count;
                InputFiles.Add(fileOptions);
            }
            return index;
        }
    }

}
