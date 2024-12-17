using FFMpeg_Wrapper.ffprobe;
using FFMpeg_Wrapper.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg {
    public class FilterArguments {

        List<string> Inputs = new();
        List<Filter> Filters = new();
        List<string> OutputNames = new();

        internal IEnumerable<string> GetArguments() {
            StringBuilder arg = new();
            arg.Append("-filter_complex \"");
            foreach(var input in this.Inputs)
            {
                arg.Append(input);
            }
            bool first = true;
            foreach (var filter in this.Filters)
            {
                if (!first)
                {
                    arg.Append(',');
                }
                arg.Append(filter.GetArguments());
                first = false;
            }
            foreach(var output in this.OutputNames)
            {
                arg.Append(output);
            }
            arg.Append("\"");

            yield return arg.ToString();
        }

        public FilterArguments AddInput(string namedInput) {
            Inputs.Add($"[{Utils.GetEscapedString(namedInput)}]");
            return this;
        }

        public FilterArguments AddInputs(IEnumerable<string> namedInputs) {
            foreach(var name in namedInputs)
            {
                this.AddInput(name);
            }
            return this;
        }

        public FilterArguments AddInputs(params string[] namedInputs) {
            return this.AddInputs(namedInputs.AsEnumerable());
        }

        public FilterArguments AddInput(int inputFileIndex, int streamIndex) {
            Inputs.Add($"[{inputFileIndex}:{streamIndex}]");
            return this;
        }

        public FilterArguments AddInputs(IEnumerable<(int InputFileIndex, int StreamIndex)> inputs) {
            foreach((int inputFileIndex, int streamIndex) in inputs)
            {
                AddInput(inputFileIndex, streamIndex);
            }
            return this;
        }

        public FilterArguments AddInputs(params (int InputFileIndex, int StreamIndex)[] inputs) {
            return this.AddInputs(inputs.AsEnumerable());
        }

        public FilterArguments AddFilter(Filter filter) {
            this.Filters.Add(filter);
            return this;
        }

        public FilterArguments AddOutputs(params string[] outputStreamNames) {
            return this.AddOutputs(outputStreamNames.AsEnumerable());
        }

        public FilterArguments AddOutputs(IEnumerable<string> outputStreamNames) {
            foreach(var name in outputStreamNames)
            {
                AddOutput(name);
            }
            return this;
        }

        public FilterArguments AddOutput(string outputStreamName) {
            this.OutputNames.Add($"[{Utils.GetEscapedString(outputStreamName)}]");
            return this;
        }
    }
}
