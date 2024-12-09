﻿using Iso639;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffprobe
{
    public class MediaAnalysis {
        public string FilePath;

        internal MediaAnalysis(FFProbeAnalysis analysis, string filePath) {
            this.FilePath = filePath;
            Format = ParseFormat(analysis.Format);
            Chapters = analysis.Chapters.Select(c => ParseChapter(c)).ToList();
            VideoStreams = analysis.Streams.Where(stream => stream.CodecType == "video").Select(ParseVideoStream).ToList();
            AudioStreams = analysis.Streams.Where(stream => stream.CodecType == "audio").Select(ParseAudioStream).ToList();
            SubtitleStreams = analysis.Streams.Where(stream => stream.CodecType == "subtitle").Select(ParseSubtitleStream).ToList();
            ErrorData = analysis.ErrorData;
        }

        private MediaFormat ParseFormat(Format analysisFormat) {
            return new MediaFormat {
                Duration = MediaAnalysisUtils.ParseDuration(analysisFormat.Duration),
                StartTime = MediaAnalysisUtils.ParseDuration(analysisFormat.StartTime),
                FormatName = analysisFormat.FormatName,
                FormatLongName = analysisFormat.FormatLongName,
                StreamCount = analysisFormat.NbStreams,
                ProbeScore = analysisFormat.ProbeScore,
                BitRate = long.Parse(analysisFormat.BitRate ?? "0"),
                Tags = analysisFormat.Tags.ToCaseInsensitive(),
            };
        }

        private string GetValue(string tagName, Dictionary<string, string>? tags, string defaultValue) =>
            tags == null ? defaultValue : tags.TryGetValue(tagName, out var value) ? value : defaultValue;

        private ChapterData ParseChapter(Chapter analysisChapter) {
            var title = GetValue("title", analysisChapter.Tags, "TitleValueNotSet");
            var start = MediaAnalysisUtils.ParseDuration(analysisChapter.StartTime);
            var end = MediaAnalysisUtils.ParseDuration(analysisChapter.EndTime);

            return new ChapterData(title, start, end);
        }

        public TimeSpan Duration => new[]
        {
            Format.Duration,
            PrimaryVideoStream?.Duration ?? TimeSpan.Zero,
            PrimaryAudioStream?.Duration ?? TimeSpan.Zero
        }.Max();

        public MediaFormat Format { get; }

        public List<ChapterData> Chapters { get; }

        public AudioStream? PrimaryAudioStream => AudioStreams.OrderBy(stream => stream.Index).FirstOrDefault();
        public VideoStream? PrimaryVideoStream => VideoStreams.OrderBy(stream => stream.Index).FirstOrDefault();
        public SubtitleStream? PrimarySubtitleStream => SubtitleStreams.OrderBy(stream => stream.Index).FirstOrDefault();

        public List<VideoStream> VideoStreams { get; }
        public List<AudioStream> AudioStreams { get; }
        public List<SubtitleStream> SubtitleStreams { get; }
        public IReadOnlyList<string> ErrorData { get; }

        private int? GetBitDepth(FFProbeStream stream) {
            var bitDepth = int.TryParse(stream.BitsPerRawSample, out var bprs) ? bprs :
                stream.BitsPerSample;
            return bitDepth == 0 ? null : bitDepth;
        }

        private VideoStream ParseVideoStream(FFProbeStream stream) {
            return new VideoStream(this) {
                Index = stream.Index,
                AvgFrameRate = MediaAnalysisUtils.DivideRatio(MediaAnalysisUtils.ParseRatioDouble(stream.AvgFrameRate, '/')),
                BitRate = !string.IsNullOrEmpty(stream.BitRate) ? MediaAnalysisUtils.ParseLongInvariant(stream.BitRate) : default,
                BitsPerRawSample = !string.IsNullOrEmpty(stream.BitsPerRawSample) ? MediaAnalysisUtils.ParseIntInvariant(stream.BitsPerRawSample) : default,
                CodecName = stream.CodecName,
                CodecLongName = stream.CodecLongName,
                CodecTag = stream.CodecTag,
                CodecTagString = stream.CodecTagString,
                DisplayAspectRatio = MediaAnalysisUtils.ParseRatioInt(stream.DisplayAspectRatio, ':'),
                SampleAspectRatio = MediaAnalysisUtils.ParseRatioInt(stream.SampleAspectRatio, ':'),
                Duration = MediaAnalysisUtils.ParseDuration(stream.Duration),
                StartTime = MediaAnalysisUtils.ParseDuration(stream.StartTime),
                FrameRate = MediaAnalysisUtils.DivideRatio(MediaAnalysisUtils.ParseRatioDouble(stream.FrameRate, '/')),
                Height = stream.Height ?? 0,
                Width = stream.Width ?? 0,
                Profile = stream.Profile,
                PixelFormat = stream.PixelFormat,
                Level = stream.Level,
                ColorRange = stream.ColorRange,
                ColorSpace = stream.ColorSpace,
                ColorTransfer = stream.ColorTransfer,
                ColorPrimaries = stream.ColorPrimaries,
                Rotation = MediaAnalysisUtils.ParseRotation(stream),
                Language = ParseLanguage(stream.Language),
                Disposition = MediaAnalysisUtils.FormatDisposition(stream.Disposition),
                Tags = stream.Tags.ToCaseInsensitive(),
                BitDepth = GetBitDepth(stream),
                NumberOfFrames = !string.IsNullOrEmpty(stream.NumberOfFrames) ? MediaAnalysisUtils.ParseUIntInvariant(stream.NumberOfFrames) : default
            };
        }

        private AudioStream ParseAudioStream(FFProbeStream stream) {
            return new AudioStream(this) {
                Index = stream.Index,
                BitRate = !string.IsNullOrEmpty(stream.BitRate) ? MediaAnalysisUtils.ParseLongInvariant(stream.BitRate) : default,
                CodecName = stream.CodecName,
                CodecLongName = stream.CodecLongName,
                CodecTag = stream.CodecTag,
                CodecTagString = stream.CodecTagString,
                Channels = stream.Channels ?? default,
                ChannelLayout = stream.ChannelLayout,
                Duration = MediaAnalysisUtils.ParseDuration(stream.Duration),
                StartTime = MediaAnalysisUtils.ParseDuration(stream.StartTime),
                SampleRateHz = !string.IsNullOrEmpty(stream.SampleRate) ? MediaAnalysisUtils.ParseIntInvariant(stream.SampleRate) : default,
                Profile = stream.Profile,
                Language = ParseLanguage(stream.Language),
                Disposition = MediaAnalysisUtils.FormatDisposition(stream.Disposition),
                Tags = stream.Tags.ToCaseInsensitive(),
                BitDepth = GetBitDepth(stream),
            };
        }

        private SubtitleStream ParseSubtitleStream(FFProbeStream stream) {
            return new SubtitleStream(this) {
                Index = stream.Index,
                BitRate = !string.IsNullOrEmpty(stream.BitRate) ? MediaAnalysisUtils.ParseLongInvariant(stream.BitRate) : default,
                CodecName = stream.CodecName,
                CodecLongName = stream.CodecLongName,
                Duration = MediaAnalysisUtils.ParseDuration(stream.Duration),
                StartTime = MediaAnalysisUtils.ParseDuration(stream.StartTime),
                Language = ParseLanguage(stream.Language),
                Disposition = MediaAnalysisUtils.FormatDisposition(stream.Disposition),
                Tags = stream.Tags.ToCaseInsensitive(),
            };
        }

        private static Language? ParseLanguage(string? code) {
            if (code == null) return null;

            if (code.Length == 2)
            {
                return Language.FromPart1(code);
            } else if (code.Length == 3)
            {
                return Language.FromPart2(code) ?? Language.FromPart3(code);
            }

            return null;
        }
    }

    internal static class MediaAnalysisUtils {
        private static readonly Regex DurationRegex = new(@"^(\d+):(\d{1,2}):(\d{1,2})\.(\d{1,3})", RegexOptions.Compiled);

        internal static Dictionary<string, string> ToCaseInsensitive(this Dictionary<string, string>? dictionary) {
            return dictionary?.ToDictionary(tag => tag.Key, tag => tag.Value, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, string>();
        }
        public static double DivideRatio((double, double) ratio) => ratio.Item1 / ratio.Item2;

        public static (int, int) ParseRatioInt(string input, char separator) {
            if (string.IsNullOrEmpty(input))
            {
                return (0, 0);
            }

            var ratio = input.Split(separator);
            return (ParseIntInvariant(ratio[0]), ParseIntInvariant(ratio[1]));
        }

        public static (double, double) ParseRatioDouble(string input, char separator) {
            if (string.IsNullOrEmpty(input))
            {
                return (0, 0);
            }

            var ratio = input.Split(separator);
            return (ratio.Length > 0 ? ParseDoubleInvariant(ratio[0]) : 0, ratio.Length > 1 ? ParseDoubleInvariant(ratio[1]) : 0);
        }

        public static double ParseDoubleInvariant(string line) =>
            double.Parse(line, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

        public static int ParseIntInvariant(string line) =>
            int.Parse(line, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

        public static uint ParseUIntInvariant(string line) =>
            uint.Parse(line, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

        public static long ParseLongInvariant(string line) =>
            long.Parse(line, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

        public static TimeSpan ParseDuration(string duration) {
            if (!string.IsNullOrEmpty(duration))
            {
                var match = DurationRegex.Match(duration);
                if (match.Success)
                {
                    // ffmpeg may provide < 3-digit number of milliseconds (omitting trailing zeros), which won't simply parse correctly
                    // e.g. 00:12:02.11 -> 12 minutes 2 seconds and 110 milliseconds
                    var millisecondsPart = match.Groups[4].Value;
                    if (millisecondsPart.Length < 3)
                    {
                        millisecondsPart = millisecondsPart.PadRight(3, '0');
                    }

                    var hours = int.Parse(match.Groups[1].Value);
                    var minutes = int.Parse(match.Groups[2].Value);
                    var seconds = int.Parse(match.Groups[3].Value);
                    var milliseconds = int.Parse(millisecondsPart);
                    return new TimeSpan(0, hours, minutes, seconds, milliseconds);
                } else
                {
                    return TimeSpan.Zero;
                }
            } else
            {
                return TimeSpan.Zero;
            }
        }

        public static int ParseRotation(FFProbeStream fFProbeStream) {
            var displayMatrixSideData = fFProbeStream.SideData?.Find(item => item.TryGetValue("side_data_type", out var rawSideDataType) && rawSideDataType.ToString() == "Display Matrix");

            if (displayMatrixSideData?.TryGetValue("rotation", out var rawRotation) ?? false)
            {
                return (int)float.Parse(rawRotation.ToString());
            } else
            {
                return (int)float.Parse(fFProbeStream.Rotate ?? "0");
            }
        }

        public static Dictionary<string, bool>? FormatDisposition(Dictionary<string, int>? disposition) {
            if (disposition == null)
            {
                return null;
            }

            var result = new Dictionary<string, bool>(disposition.Count, StringComparer.Ordinal);

            foreach (var pair in disposition)
            {
                result.Add(pair.Key, ToBool(pair.Value));
            }

            static bool ToBool(int value) => value switch {
                0 => false,
                1 => true,
                _ => throw new ArgumentOutOfRangeException(nameof(value),
                    $"Not expected disposition state value: {value}")
            };

            return result;
        }
    }
}