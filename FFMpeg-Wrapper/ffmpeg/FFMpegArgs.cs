using FFMpeg_Wrapper.ffprobe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg {
    public interface IFFMpegArgs {
        internal void Begin();
        internal IEnumerable<string> GetArguments();
        internal void Cleanup();
    }

    public interface FFMpegInput : IFFMpegArgs {
        internal TimeSpan Duration { get; }
    }

    public class FFMpegArgs : FFMpegCliArgs {

        List<FFMpegInput> InputFiles = new();
        List<FilterArguments> InputFilters = new();
        List<IFFMpegArgs> Arguments = new();
        int InputFileCount = 0;
        int OutputStreamCount = 0;
        OutputFile OutputFile;
        TimeSpan LongestInput = new();

        internal FFMpegArgs(FFMpeg core, OutputFile outputFile) : base(core) {
            this.OutputFile = outputFile;
        }

        protected override TimeSpan DetermineProcessTime() {
            if (OutputFile.Duration != null)
            {
                // Duration takes priority over stop time
                return OutputFile.Duration.Value;
            } else
            {
                if (OutputFile.StopTime != null)
                {
                    TimeSpan startTime = new();
                    if (OutputFile.StartTime != null)
                    {
                        startTime = OutputFile.StartTime.Value;
                    }
                    return OutputFile.StopTime.Value - startTime;
                }
            }

            // Duration will be determined by input files
            // OutputFile.ToTime is not defined, so the output length is uncapped.
            // There may still be output seeking
            if (OutputFile.StartTime != null)
            {
                TimeSpan seekTime = OutputFile.StartTime.Value;
                if (seekTime > this.LongestInput) return new TimeSpan();
                else return this.LongestInput - seekTime;
            } else
            {
                return this.LongestInput;
            }
        }

        protected override void Begin() {
            foreach(var input in this.InputFiles)
            {
                input.Begin();
            }
        }

        protected override IEnumerable<string> GetArguments() {
            return this.InputFiles.SelectMany(input => input.GetArguments())
                .Concat(this.InputFilters.SelectMany(x => x.GetArguments()))
                .Concat(Arguments.SelectMany(args => args.GetArguments()))
                .Concat(this.OutputFile.GetArguments());
        }

        protected override void Cleanup() {
            foreach (var input in this.InputFiles)
            {
                input.Cleanup();
            }
        }

        /// <summary>
        /// Returns the index for this input.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public int AddInput(FFMpegInput input) {
            if (input.Duration > this.LongestInput)
            {
                this.LongestInput = input.Duration;
            }

            this.InputFiles.Add(input);
            return InputFileCount++;
        }

        public FFMpegArgs AddInputFilter(FilterArguments filter) {
            this.InputFilters.Add(filter);
            return this;
        } 

        /// <summary>
        /// Appends the selected input stream to the output file with the given options
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public FFMpegArgs AddStream(StreamOptions stream) {
            this.Arguments.Add(new StreamArgs(stream, $":{this.OutputStreamCount}"));
            this.OutputStreamCount++;
            return this;
        }

        /// <summary>
        /// Appends all of the selected input streams to the output file with the given options
        /// </summary>
        /// <param name="streams"></param>
        /// <returns></returns>
        public FFMpegArgs AddStreams(IEnumerable<StreamOptions> streams) {
            foreach(var stream in streams)
            {
                this.AddStream(stream);
            }
            return this;
        }

        /// <inheritdoc cref="AddStreams(IEnumerable{StreamOptions})"/>
        public FFMpegArgs AddStreams(params StreamOptions[] streams) {
            return this.AddStreams(streams.AsEnumerable());
        }

        /// <summary>
        /// Shortcut method to quickly copy the named stream to the output file
        /// </summary>
        /// <param name="namedStream"></param>
        /// <returns></returns>
        public FFMpegArgs CopyStream(string namedStream) {
            return this.AddStream(new StreamOptions(namedStream));
        }

        /// <summary>
        /// Shortcut method to quickly copy the named streams to the output file
        /// </summary>
        /// <param name="namedStreams"></param>
        /// <returns></returns>
        public FFMpegArgs CopyStreams(IEnumerable<string> namedStreams) {
            foreach(var name in namedStreams)
            {
                this.CopyStream(name);
            }
            return this;
        }

        /// <inheritdoc cref="CopyStreams(IEnumerable{string})"/>
        public FFMpegArgs CopyStreams(params string[] namedStreams) {
            return this.CopyStreams(namedStreams.AsEnumerable());
        }

        /// <summary>
        /// Shortcut method to quickly copy the input stream to the output file
        /// </summary>
        /// <param name="inputFileIndex"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public FFMpegArgs CopyStream(int inputFileIndex, MediaStream stream) {
            return this.AddStream(new StreamOptions(inputFileIndex, stream.Index));
        }

        /// <summary>
        /// Shortcut method to quickly copy the input streams to the output file
        /// </summary>
        /// <param name="inputFileIndex"></param>
        /// <param name="streams"></param>
        /// <returns></returns>
        public FFMpegArgs CopyStreams(int inputFileIndex, IEnumerable<MediaStream> streams) {
            foreach(var stream in streams)
            {
                this.CopyStream(inputFileIndex, stream);
            }
            return this;
        }

        /// <inheritdoc cref="CopyStreams(int, IEnumerable{MediaStream})"/>
        public FFMpegArgs CopyStreams(int inputFileIndex, params MediaStream[] streams) {
            return this.CopyStreams(inputFileIndex, streams.AsEnumerable());
        }
    }

    internal class StreamArgs : IFFMpegArgs {
        StreamOptions Stream;
        string OutputStreamSpecifier;

        public StreamArgs(StreamOptions options, string outputStreamSpecifier) {
            this.Stream = options;
            this.OutputStreamSpecifier = outputStreamSpecifier;
        }

        void IFFMpegArgs.Begin() {
        }

        void IFFMpegArgs.Cleanup() {
        }

        IEnumerable<string> IFFMpegArgs.GetArguments() {
            return Stream.GetArguments(this.OutputStreamSpecifier);
        }
    }
}
