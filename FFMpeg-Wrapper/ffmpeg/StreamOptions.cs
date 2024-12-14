using FFMpeg_Wrapper.Codecs;
using FFMpeg_Wrapper.Filters;
using Iso639;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg {
    public abstract class StreamOptions<TCodec, TFilter, TSelf>
        where TCodec : class, Codec
        where TFilter : class, Filter
        where TSelf : class 
    {
        TCodec Codec;
        List<TFilter> Filters = new();
        Dictionary<string, bool> Dispositions = new();
        Dictionary<string, string> Metadata = new();

        protected StreamOptions(TCodec defaultCodec) {
            Codec = defaultCodec;
        }

        protected abstract TSelf GetThis();

        /// <summary>
        /// a:0 would select the first audio stream in the output
        /// </summary>
        /// <param name="outputStreamSpecifier"></param>
        /// <returns></returns>
        internal IEnumerable<string> GetArguments(string outputStreamSpecifier) {
            yield return $"-c{outputStreamSpecifier} {Codec.Name}";
            foreach (var arg in Codec.GetArguments())
            {
                yield return arg;
            }

            if (Filters.Count > 0)
            {
                yield return $"-filter{outputStreamSpecifier} {string.Join(',', Filters.Select(filter => filter.GetArguments()))}";
            }

            if (Dispositions.Count > 0)
            {
                IEnumerable<string> flags = this.Dispositions.Select(pair =>
                {
                    return $"{(pair.Value ? "+" : "-")}{pair.Key}";
                });
                yield return $"-disposition{outputStreamSpecifier} {string.Join("", flags)}";
            }

            if (this.Metadata.Count > 0)
            {
                string cmd;
                if (string.IsNullOrEmpty(outputStreamSpecifier))
                {
                    cmd = $"-metadata";
                } else
                {
                    cmd = $"-metadata:s{outputStreamSpecifier}";
                }
                foreach(var metadata in this.Metadata)
                {
                    yield return $"{cmd} {metadata.Key}={metadata.Value}";
                }
            }
        }

        public TSelf SetCodec(TCodec codec) {
            Codec = codec;
            return GetThis();
        }

        public TSelf AddFilter(TFilter filter) {
            this.Filters.Add(filter);
            return GetThis();
        }

        public TSelf SetFlag(StreamFlag flag, bool enabled) {
            string? key = GetStreamFlagKey(flag);
            if (key != null)
            {
                this.Dispositions[key] = enabled;
            }
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

        /// <summary>
        /// Passing null will list the language as "und"
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public TSelf SetLanguage(Language? language) {
            this.Metadata["language"] = language.Part3 ?? language.Part2 ?? language.Part1 ?? "und";
            return GetThis();
        }

        public TSelf SetName(string name) {
            this.Metadata["title"] = Utils.GetEscapedString(name);
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
        public VideoStreamOptions() : base(Codecs.Codecs.Copy) { }

        protected override VideoStreamOptions GetThis() {
            return this;
        }
    }
    public class AudioStreamOptions : StreamOptions<AudioCodec, AudioFilter, AudioStreamOptions> {
        public AudioStreamOptions() : base(Codecs.Codecs.Copy) { }

        protected override AudioStreamOptions GetThis() {
            return this;
        }
    }
    public class SubtitleStreamOptions : StreamOptions<SubtitleCodec, SubtitleFilter, SubtitleStreamOptions> {
        public SubtitleStreamOptions() : base(Codecs.Codecs.Copy) { }

        protected override SubtitleStreamOptions GetThis() {
            return this;
        }
    }
}
