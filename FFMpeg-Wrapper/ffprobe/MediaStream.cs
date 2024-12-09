using Iso639;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffprobe {
    public abstract class MediaStream {
        public MediaAnalysis SourceFile;
        public int Index { get; set; }
        public string CodecName { get; set; } = null!;
        public string CodecLongName { get; set; } = null!;
        public string CodecTagString { get; set; } = null!;
        public string CodecTag { get; set; } = null!;
        public long BitRate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public Language? Language { get; set; }
        public Dictionary<string, bool>? Disposition { get; set; }
        public Dictionary<string, string>? Tags { get; set; }
        public int? BitDepth { get; set; }

        protected MediaStream(MediaAnalysis sourceFile) {
            this.SourceFile = sourceFile;
        }
    }
}
