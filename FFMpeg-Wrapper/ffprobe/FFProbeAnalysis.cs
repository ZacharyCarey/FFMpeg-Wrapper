using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffprobe {
    public class FFProbeAnalysis {
        [JsonPropertyName("streams")]
        public List<FFProbeStream> Streams { get; set; } = null!;

        [JsonPropertyName("format")]
        public Format Format { get; set; } = null!;

        [JsonPropertyName("chapters")]
        public List<Chapter> Chapters { get; set; } = null!;

        [JsonIgnore]
        public IReadOnlyList<string> ErrorData { get; set; } = new List<string>();
    }

    public class FFProbeStream : DispositionContainer {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("avg_frame_rate")]
        public string AvgFrameRate { get; set; } = null!;

        [JsonPropertyName("bits_per_raw_sample")]
        public string BitsPerRawSample { get; set; } = null!;

        [JsonPropertyName("bits_per_sample")]
        public int BitsPerSample { get; set; } = 0;

        [JsonPropertyName("bit_rate")]
        public string BitRate { get; set; } = null!;

        [JsonPropertyName("channels")]
        public int? Channels { get; set; }

        [JsonPropertyName("channel_layout")]
        public string ChannelLayout { get; set; } = null!;

        [JsonPropertyName("codec_type")]
        public string CodecType { get; set; } = null!;

        [JsonPropertyName("codec_name")]
        public string CodecName { get; set; } = null!;

        [JsonPropertyName("codec_long_name")]
        public string CodecLongName { get; set; } = null!;

        [JsonPropertyName("codec_tag")]
        public string CodecTag { get; set; } = null!;

        [JsonPropertyName("codec_tag_string")]
        public string CodecTagString { get; set; } = null!;

        [JsonPropertyName("display_aspect_ratio")]
        public string DisplayAspectRatio { get; set; } = null!;

        [JsonPropertyName("sample_aspect_ratio")]
        public string SampleAspectRatio { get; set; } = null!;

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; } = null!;

        [JsonPropertyName("duration")]
        public string Duration { get; set; } = null!;

        [JsonPropertyName("profile")]
        public string Profile { get; set; } = null!;

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("r_frame_rate")]
        public string FrameRate { get; set; } = null!;

        [JsonPropertyName("pix_fmt")]
        public string PixelFormat { get; set; } = null!;

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("sample_rate")]
        public string SampleRate { get; set; } = null!;

        [JsonPropertyName("disposition")]
        public override Dictionary<string, int> Disposition { get; set; } = null!;

        [JsonPropertyName("tags")]
        public override Dictionary<string, string>? Tags { get; set; }

        [JsonPropertyName("side_data_list")]
        public List<Dictionary<string, JsonValue>> SideData { get; set; } = null!;

        [JsonPropertyName("color_range")]
        public string ColorRange { get; set; } = null!;

        [JsonPropertyName("color_space")]
        public string ColorSpace { get; set; } = null!;

        [JsonPropertyName("color_transfer")]
        public string ColorTransfer { get; set; } = null!;

        [JsonPropertyName("color_primaries")]
        public string ColorPrimaries { get; set; } = null!;

        [JsonPropertyName("nb_frames")]
        public string NumberOfFrames { get; set; } = null!;

        [JsonPropertyName("field_order")]
        public string? FieldOrder { get; set; } = null;
    }

    public class Format : TagsContainer {
        [JsonPropertyName("filename")]
        public string Filename { get; set; } = null!;

        [JsonPropertyName("nb_streams")]
        public int NbStreams { get; set; }

        [JsonPropertyName("nb_programs")]
        public int NbPrograms { get; set; }

        [JsonPropertyName("format_name")]
        public string FormatName { get; set; } = null!;

        [JsonPropertyName("format_long_name")]
        public string FormatLongName { get; set; } = null!;

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; } = null!;

        [JsonPropertyName("duration")]
        public string Duration { get; set; } = null!;

        [JsonPropertyName("size")]
        public string Size { get; set; } = null!;

        [JsonPropertyName("bit_rate")]
        public string? BitRate { get; set; } = null!;

        [JsonPropertyName("probe_score")]
        public int ProbeScore { get; set; }

        [JsonPropertyName("tags")]
        public override Dictionary<string, string>? Tags { get; set; }
    }

    public class Chapter : TagsContainer {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("time_base")]
        public string TimeBase { get; set; } = null!;

        [JsonPropertyName("start")]
        public long Start { get; set; }

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; } = null!;

        [JsonPropertyName("end")]
        public long End { get; set; }

        [JsonPropertyName("end_time")]
        public string EndTime { get; set; } = null!;

        [JsonPropertyName("tags")]
        public override Dictionary<string, string>? Tags { get; set; }
    }

    public abstract class TagsContainer {
        public abstract Dictionary<string, string>? Tags { get; set; }

        private string? TryGetTagValue(string key) {
            if (Tags != null && Tags.TryGetValue(key, out var tagValue))
            {
                return tagValue;
            }

            return null;
        }

        public string? Language { get => TryGetTagValue("language"); }
        public string? CreationTime { get => TryGetTagValue("creation_time"); }
        public string? Rotate { get => TryGetTagValue("rotate"); }
        public string? Duration { get => TryGetTagValue("duration"); }

    }

    public abstract class DispositionContainer : TagsContainer {
        public abstract Dictionary<string, int> Disposition { get; set; }

        private int? TryGetDispositionValue(string key) {
            if (Disposition.TryGetValue(key, out var dispositionValue))
            {
                return dispositionValue;
            }

            return null;
        }

        private bool? TryGetBool(string key) {
            int? val = TryGetDispositionValue(key);
            if (val == null) return null;

            return (val != 0);
        }

        public bool? DefaultFlag { get => TryGetBool("default"); }
        public bool? ForcedFlag { get => TryGetBool("forced"); }
        public bool? DubFlag { get => TryGetBool("dub"); }
        public bool? OriginalFlag { get => TryGetBool("original"); }
        public bool? CommentFlag { get => TryGetBool("comment"); }
        public bool? LyricsFlag { get => TryGetBool("lyrics"); }
        public bool? KaraokeFlag { get => TryGetBool("karaoke"); }
        public bool? HearingImpairedFlag { get => TryGetBool("hearing_impaired"); }
        public bool? VisualImpairedFlag { get => TryGetBool("visual_impaired"); }
        public bool? CleanEffects { get => TryGetBool("clean_effects"); }
        public bool? AttachedPicture { get => TryGetBool("attached_pic"); }
        public bool? CaptionsFlag { get => TryGetBool("captions"); }
        public bool? DescriptionsFlag { get => TryGetBool("descriptions"); }
        public bool? StillImageFlag { get => TryGetBool("still_image"); }
        public bool? MultilayerFlag { get => TryGetBool("multilayer"); }
    }
}
