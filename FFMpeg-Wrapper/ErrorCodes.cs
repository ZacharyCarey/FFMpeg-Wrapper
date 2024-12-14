using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper {
    internal static class ErrorCodes {

        internal static Dictionary<int, string> Errors = new () {
            { FFERRTAG(0xF8,'B','S','F'), "AVERROR_BSF_NOT_FOUND: Bitstream filter not found" },
            { FFERRTAG( 'B','U','G','!'), "AVERROR_BUG: Internal bug, also see AVERROR_BUG2" },
            { FFERRTAG( 'B','U','F','S'), "AVERROR_BUFFER_TOO_SMALL: Buffer too small" },
            { FFERRTAG(0xF8,'D','E','C'), "AVERROR_DECODER_NOT_FOUND: Decoder not found" },
            { FFERRTAG(0xF8,'D','E','M'), "AVERROR_DEMUXER_NOT_FOUND: Demuxer not found" },
            { FFERRTAG(0xF8,'E','N','C'), "AVERROR_ENCODER_NOT_FOUND: Encoder not found" },
            { FFERRTAG( 'E','O','F',' '), "AVERROR_EOF: End of file" },
            { FFERRTAG( 'E','X','I','T'), "AVERROR_EXIT: Immediate exit was requested; the called function should not be restarted" },
            { FFERRTAG( 'E','X','T',' '), "AVERROR_EXTERNAL: Generic error in an external library" },
            { FFERRTAG(0xF8,'F','I','L'), "AVERROR_FILTER_NOT_FOUND: Filter not found" },
            { FFERRTAG( 'I','N','D','A'), "AVERROR_INVALIDDATA: Invalid data found when processing input" },
            { FFERRTAG(0xF8,'M','U','X'), "AVERROR_MUXER_NOT_FOUND: Muxer not found" },
            { FFERRTAG(0xF8,'O','P','T'), "AVERROR_OPTION_NOT_FOUND: Option not found" },
            { FFERRTAG( 'P','A','W','E'), "AVERROR_PATCHWELCOME: Not yet implemented in FFmpeg, patches welcome" },
            { FFERRTAG(0xF8,'P','R','O'), "AVERROR_PROTOCOL_NOT_FOUND: Protocol not found" },
            { FFERRTAG(0xF8,'S','T','R'), "AVERROR_STREAM_NOT_FOUND: Stream not found" },

            /**
              * This is semantically identical to AVERROR_BUG
              * it has been introduced in Libav after our AVERROR_BUG and with a modified value.
              */
            { FFERRTAG( 'B','U','G',' '), "AVERROR_BUG2" },
            { FFERRTAG( 'U','N','K','N'), "AVERROR_UNKNOWN: Unknown error, typically from an external library" },
            { (-0x2bb2afa8), "AVERROR_EXPERIMENTAL: Requested feature is flagged experimental. Set strict_std_compliance if you really want to use it." },
            { (-0x636e6701), "AVERROR_INPUT_CHANGED: Input changed between calls. Reconfiguration is required. (can be OR-ed with AVERROR_OUTPUT_CHANGED)" }, 
            { (-0x636e6702), "AVERROR_OUTPUT_CHANGED: Output changed between calls. Reconfiguration is required. (can be OR-ed with AVERROR_INPUT_CHANGED)" },

            { FFERRTAG(0xF8,'4','0','0'), "AVERROR_HTTP_BAD_REQUEST" },
            { FFERRTAG(0xF8,'4','0','1'), "AVERROR_HTTP_UNAUTHORIZED" },
            { FFERRTAG(0xF8,'4','0','3'), "AVERROR_HTTP_FORBIDDEN" },
            { FFERRTAG(0xF8,'4','0','4'), "AVERROR_HTTP_NOT_FOUND" },
            { FFERRTAG(0xF8,'4','2','9'), "AVERROR_HTTP_TOO_MANY_REQUESTS" },
            { FFERRTAG(0xF8,'4','X','X'), "AVERROR_HTTP_OTHER_4XX" },
            { FFERRTAG(0xF8,'5','X','X'), "AVERROR_HTTP_SERVER_ERROR" },
        };

        private static int FFERRTAG(char a, char b, char c, char d) {
            return FFERRTAG((byte)a, b, c, d);
        }

        private static int FFERRTAG(byte a, char b, char c, char d) {
            return -MKTAG(a, (byte)b, (byte)c, (byte)d);
        }

        private static int MKTAG(byte a, byte b, byte c, byte d) {
            return (int)((uint)a | ((uint)b << 8) | ((uint)c << 16) | ((uint)d << 24));
        }
    }
}
