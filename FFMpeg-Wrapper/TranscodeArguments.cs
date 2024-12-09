using CLI_Wrapper;
using FFMpeg_Wrapper.Codecs;
using FFMpeg_Wrapper.ffprobe;
using Iso639;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper {
    public class TranscodeArguments : FFMpegArgs {
        List<MediaAnalysis> InputFiles = new(); 
        List<string> GlobalOptions = new();
        List<string> Arguments = new();
        string OutputFile;
        List<MediaStream> OutputStreams = new();

        // TODO store applied codec options so if all video streams
        // have the same codec applied, the command cam be simplified to
        // "-c:v" instead of applying it seperately to each stream

        internal TranscodeArguments(FFMpeg core, string outputFile) : base(core) {
            this.OutputFile = outputFile;
        }

        protected override IEnumerable<string> GetArguments() {
            return GlobalOptions
                .Concat(this.InputFiles.Select(file => $"-i \"{file.FilePath}\""))
                .Concat(Arguments)
                .Append($"\"{OutputFile}\"");
        }

        /// <summary>
        /// Maps all video streams from an input file to the output. 
        /// The given options will be applies to each stream as well.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public TranscodeArguments AddVideoStreams(MediaAnalysis inputFile, VideoStreamOptions? options) {
            return AddVideoStreams(inputFile.VideoStreams, options);
        }

        /// <summary>
        /// Maps all provided input video streams the output. 
        /// The given options will be applies to each stream as well.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public TranscodeArguments AddVideoStreams(IEnumerable<VideoStream> streams, VideoStreamOptions? options) {
            foreach (var stream in streams)
            {
                AddVideoStream(stream, options);
            }
            return this;
        }

        /// <summary>
        /// Maps the input video stream to the output file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public TranscodeArguments AddVideoStream(VideoStream stream, VideoStreamOptions? options) {
            int sourceIndex = GetInputIndex(stream.SourceFile);
            string outputStreamSpecifier = $":{this.OutputStreams.Count}";

            this.Arguments.Add($"-map {sourceIndex}:{stream.Index}");
            if(options != null) this.Arguments.AddRange(options.GetArguments(outputStreamSpecifier));
            this.OutputStreams.Add(stream);

            return this;
        }

        /// <summary>
        /// Maps all audio streams from an input file to the output. 
        /// The given options will be applies to each stream as well.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public TranscodeArguments AddAudioStreams(MediaAnalysis inputFile, AudioStreamOptions? options) {
            return AddAudioStreams(inputFile.AudioStreams, options);
        }

        /// <summary>
        /// Maps all provided input audio streams the output. 
        /// The given options will be applies to each stream as well.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public TranscodeArguments AddAudioStreams(IEnumerable<AudioStream> streams, AudioStreamOptions? options) {
            foreach (var stream in streams)
            {
                AddAudioStream(stream, options);
            }
            return this;
        }

        /// <summary>
        /// Maps the input audio stream to the output file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public TranscodeArguments AddAudioStream(AudioStream stream, AudioStreamOptions? options) {
            int sourceIndex = GetInputIndex(stream.SourceFile);
            string outputStreamSpecifier = $":{this.OutputStreams.Count}";

            this.Arguments.Add($"-map {sourceIndex}:{stream.Index}");
            if (options != null) this.Arguments.AddRange(options.GetArguments(outputStreamSpecifier));
            this.OutputStreams.Add(stream);

            return this;
        }

        /// <summary>
        /// Maps all subtitle streams from an input file to the output. 
        /// The given options will be applies to each stream as well.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public TranscodeArguments AddSubtitleStreams(MediaAnalysis inputFile, SubtitleStreamOptions? options) {
            return AddSubtitleStreams(inputFile.SubtitleStreams, options);
        }

        /// <summary>
        /// Maps all provided input subtitle streams the output. 
        /// The given options will be applies to each stream as well.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public TranscodeArguments AddSubtitleStreams(IEnumerable<SubtitleStream> streams, SubtitleStreamOptions? options) {
            foreach (var stream in streams)
            {
                AddSubtitleStream(stream, options);
            }
            return this;
        }

        /// <summary>
        /// Maps the input subtitle stream to the output file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public TranscodeArguments AddSubtitleStream(SubtitleStream stream, SubtitleStreamOptions? options) {
            int sourceIndex = GetInputIndex(stream.SourceFile);
            string outputStreamSpecifier = $":{this.OutputStreams.Count}";

            this.Arguments.Add($"-map {sourceIndex}:{stream.Index}");
            if (options != null) this.Arguments.AddRange(options.GetArguments(outputStreamSpecifier));
            this.OutputStreams.Add(stream);

            return this;
        }

        private int GetInputIndex(MediaAnalysis inputFile) {
            int index = this.InputFiles.IndexOf(inputFile);
            if (index < 0)
            {
                index = this.InputFiles.Count;
                this.InputFiles.Add(inputFile);
            }
            return index;
        }
    }

    public abstract class StreamOptions<TCodec, TSelf> where TCodec : class, Codec where TSelf : class {
        public TCodec Codec;
        public Language? Language;

        protected StreamOptions(TCodec defaultCodec) {
            this.Codec = defaultCodec;
        }

        protected abstract TSelf GetThis();

        /// <summary>
        /// a:0 would select the first audio stream in the output
        /// </summary>
        /// <param name="outputStreamSpecifier"></param>
        /// <returns></returns>
        internal IEnumerable<string> GetArguments(string outputStreamSpecifier) {
            yield return $"-c{outputStreamSpecifier} {Codec.Name}";
            foreach (var arg in Codec.GetArguments(outputStreamSpecifier))
            {
                yield return arg;
            }

            if (this.Language != null)
            {
                string arg = $"language={Language.Part3 ?? Language.Part2 ?? Language.Part1 ?? "und"}";
                if (string.IsNullOrEmpty(outputStreamSpecifier))
                {
                    yield return $"-metadata {arg}";
                } else
                {
                    yield return $"-metadata:s{outputStreamSpecifier} {arg}";
                } 
            }
        }

        public TSelf SetCodec(TCodec codec) {
            this.Codec = codec;
            return GetThis();
        }

        public TSelf SetLanguage(Language? language) {
            this.Language = language;
            return GetThis();
        }
    }

    public class VideoStreamOptions : StreamOptions<VideoCodec, VideoStreamOptions> {
        public VideoStreamOptions() : base(Codecs.Codecs.Copy) { }

        protected override VideoStreamOptions GetThis() {
            return this;
        }
    }
    public class AudioStreamOptions : StreamOptions<AudioCodec, AudioStreamOptions> {
        public AudioStreamOptions() : base(Codecs.Codecs.Copy) { }

        protected override AudioStreamOptions GetThis() {
            return this;
        }
    }
    public class SubtitleStreamOptions : StreamOptions<SubtitleCodec, SubtitleStreamOptions> {
        public SubtitleStreamOptions() : base(Codecs.Codecs.Copy) { }

        protected override SubtitleStreamOptions GetThis() {
            return this;
        }
    }
}
