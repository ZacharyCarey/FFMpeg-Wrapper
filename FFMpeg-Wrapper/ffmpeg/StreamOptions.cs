using FFMpeg_Wrapper.Codecs;
using FFMpeg_Wrapper.Filters;
using Iso639;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg {

    public class StreamOptions {
        Codec Codec = new CopyCodec();
        Dictionary<string, bool> Dispositions = new();
        Dictionary<string, string> Metadata = new();
        string InputMap;

        public StreamOptions(string namedStream) {
            this.InputMap = $"\"[{Utils.GetEscapedString(namedStream)}]\"";
        }

        public StreamOptions(int inputFileIndex, int streamIndex) {
            this.InputMap = $"{inputFileIndex}:{streamIndex}";
        }

        /// <summary>
        /// a:0 would select the first audio stream in the output
        /// </summary>
        /// <param name="outputStreamSpecifier"></param>
        /// <returns></returns>
        internal IEnumerable<string> GetArguments(string OutputStreamSpecifier) {
            yield return $"-map {InputMap}";
            yield return $"-c{OutputStreamSpecifier} {Codec.Name}";
            foreach (var arg in Codec.GetArguments())
            {
                yield return arg;
            }

            if (Dispositions.Count > 0)
            {
                IEnumerable<string> flags = this.Dispositions.Select(pair =>
                {
                    return $"{(pair.Value ? "+" : "-")}{pair.Key}";
                });
                yield return $"-disposition{OutputStreamSpecifier} {string.Join("", flags)}";
            }

            if (this.Metadata.Count > 0)
            {
                string cmd;
                if (string.IsNullOrEmpty(OutputStreamSpecifier))
                {
                    cmd = $"-metadata";
                } else
                {
                    cmd = $"-metadata:s{OutputStreamSpecifier}";
                }
                foreach (var metadata in this.Metadata)
                {
                    yield return $"{cmd} {metadata.Key}={metadata.Value}";
                }
            }
        }

        public StreamOptions SetCodec(Codec codec) {
            Codec = codec;
            return this;
        }

        public StreamOptions SetFlag(StreamFlag flag, bool enabled) {
            string? key = GetStreamFlagKey(flag);
            if (key != null)
            {
                this.Dispositions[key] = enabled;
            }
            return this;
        }

        private string? GetStreamFlagKey(StreamFlag flag) {
            switch (flag)
            {
                case StreamFlag.Default: return "default";
                case StreamFlag.Commentary: return "comment";
                case StreamFlag.Forced: return "forced";
                case StreamFlag.Original: return "original";
                case StreamFlag.HearingImpaired: return "hearing_impaired";
                case StreamFlag.VisualImpaired: return "visual_impaired";
                case StreamFlag.Captions: return "captions";
                case StreamFlag.Descriptions: return "descriptions";
                default: return null;
            }
        }

        /// <summary>
        /// Passing null will list the language as "und"
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public StreamOptions SetLanguage(Language? language) {
            this.Metadata["language"] = language.Part3 ?? language.Part2 ?? language.Part1 ?? "und";
            return this;
        }

        public StreamOptions SetName(string name) {
            this.Metadata["title"] = $"\"{Utils.GetEscapedString(name)}\"";
            return this;
        }
    }

    public abstract class StreamOptions<TCodec, TFilter, TSelf> : StreamOptions
        where TCodec : class, Codec
        where TFilter : class, Filter
        where TSelf : class 
    {
        protected StreamOptions(string namedStream) : base(namedStream) {
        }

        protected StreamOptions(int inputFileIndex, int streamIndex) : base(inputFileIndex, streamIndex) {
        }

        protected abstract TSelf GetThis();

        // Hide old function
        private new StreamOptions SetCodec(Codec codec) {
            return base.SetCodec(codec);
        } 

        /// <inheritdoc cref="StreamOptions.SetCodec(Codec)"/>
        public TSelf SetCodec(TCodec codec) {
            base.SetCodec(codec);
            return GetThis();
        }

        /// <inheritdoc cref="StreamOptions.SetFlag(StreamFlag, bool)"/>
        public new TSelf SetFlag(StreamFlag flag, bool enabled) {
            base.SetFlag(flag, enabled);
            return GetThis();
        }

        private string? GetStreamFlagKey(StreamFlag flag) {
            switch(flag)
            {
                case StreamFlag.Default: return "default";
                case StreamFlag.Commentary: return "comment";
                case StreamFlag.Forced: return "forced";
                case StreamFlag.Original: return "original";
                case StreamFlag.HearingImpaired: return "hearing_impaired";
                case StreamFlag.VisualImpaired: return "visual_impaired";
                case StreamFlag.Captions: return "captions";
                case StreamFlag.Descriptions: return "descriptions";
                default: return null;
            }
        }

        /// <inheritdoc cref="StreamOptions.SetLanguage(Language?)"/>
        public new TSelf SetLanguage(Language? language) {
            base.SetLanguage(language);
            return GetThis();
        }

        /// <inheritdoc cref="StreamOptions.SetName(string)"/>
        public TSelf SetName(string name) {
            base.SetName(name);
            return GetThis();
        }
    }

    public enum StreamFlag {
        Default,
        Commentary,
        Forced,
        Original,
        HearingImpaired,
        VisualImpaired,
        Captions,
        Descriptions
    }

    public class VideoStreamOptions : StreamOptions<VideoCodec, VideoFilter, VideoStreamOptions> {
        public VideoStreamOptions(string namedStream) : base(namedStream) {
        }

        public VideoStreamOptions(int inputFileIndex, int streamIndex) : base(inputFileIndex, streamIndex) {
        }

        protected override VideoStreamOptions GetThis() {
            return this;
        }
    }
    public class AudioStreamOptions : StreamOptions<AudioCodec, AudioFilter, AudioStreamOptions> {
        public AudioStreamOptions(string namedStream) : base(namedStream) {
        }

        public AudioStreamOptions(int inputFileIndex, int streamIndex) : base(inputFileIndex, streamIndex) {
        }

        protected override AudioStreamOptions GetThis() {
            return this;
        }
    }
    public class SubtitleStreamOptions : StreamOptions<SubtitleCodec, SubtitleFilter, SubtitleStreamOptions> {
        public SubtitleStreamOptions(string namedStream) : base(namedStream) {
        }

        public SubtitleStreamOptions(int inputFileIndex, int streamIndex) : base(inputFileIndex, streamIndex) {
        }

        protected override SubtitleStreamOptions GetThis() {
            return this;
        }
    }
}
