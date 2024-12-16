using FFMpeg_Wrapper.ffprobe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace FFMpeg_Wrapper.ffmpeg {
    public class ConcatArguments : FFMpegInput {

        internal string? TempFolder = null;
        List<string> FileData = new();
        TimeSpan TotalDuration = new();

        TimeSpan FFMpegInput.Duration => TotalDuration;

        public ConcatArguments() {
        }

        void IFFMpegArgs.Begin() {
            if (this.TempFolder != null) throw new Exception("Begin() was called twice or not properly cleaned.");

            DirectoryInfo di = Directory.CreateTempSubdirectory();
            if (!di.Exists) throw new DirectoryNotFoundException();
            this.TempFolder = di.FullName;

            // We only need to create one file no matter how many times this is ran
            File.WriteAllLines(Path.Combine(this.TempFolder, "FileList.txt"), FileData);
        }

        IEnumerable<string> IFFMpegArgs.GetArguments() {
            yield return "-f concat";
            yield return "-safe 0";
            yield return $"-i \"{Path.Combine(this.TempFolder, "FileList.txt")}\"";
        }

        void IFFMpegArgs.Cleanup() {
            try
            {
                Directory.Delete(this.TempFolder, true);
            } catch (Exception) { }
            this.TempFolder = null;
        }

        public ConcatArguments AddFile(FileInput file) {
            this.TotalDuration += ((FFMpegInput)file).Duration;
            
            string filePath = file.FilePath.Replace("'", "\\'");
            this.FileData.Add($"file '{filePath}'");

            if (file.Duration != null) this.FileData.Add($"duration {file.Duration.Value.GetFFMpegFormat()}");
            if (file.StartTime != null) this.FileData.Add($"inpoint {file.StartTime.Value.GetFFMpegFormat()}");
            if (file.StopTime != null) this.FileData.Add($"outpoint {file.StopTime.Value.GetFFMpegFormat()}");

            return this;
        }

        public ConcatArguments AddFiles(IEnumerable<FileInput> files) {
            foreach(var file in files)
            {
                this.AddFile(file);
            }
            return this;
        }

        public ConcatArguments AddFiles(params FileInput[] files) {
            return this.AddFiles(files.AsEnumerable());
        }

        public ConcatArguments AddFile(MediaAnalysis file) {
            return this.AddFile(new FileInput(file));
        }

        public ConcatArguments AddFiles(IEnumerable<MediaAnalysis> files) {
            return this.AddFiles(files.Select(x => new FileInput(x)));
        }

        public ConcatArguments AddFiles(params MediaAnalysis[] files) {
            return this.AddFiles(files.AsEnumerable());
        }

        public ConcatArguments AddChapter(string ID, TimeSpan start, TimeSpan end) {
            this.FileData.Add($"chapter {ID} {start.GetFFMpegFormat()} {end.GetFFMpegFormat()}");
            return this;
        }
    }
}
