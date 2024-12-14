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
        Language? Language;

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

            if (Language != null)
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
            Codec = codec;
            return GetThis();
        }

        public TSelf AddFilter(TFilter filter) {
            this.Filters.Add(filter);
            return GetThis();
        }

        public TSelf SetLanguage(Language? language) {
            Language = language;
            return GetThis();
        }
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
