using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Novaroma.Interface.EventHandler;
using Novaroma.Interface.Model;
using Novaroma.Model;

namespace Novaroma.Plugins.MkvPackager
{
    public class MkvPackagerPlugin : IDownloadEventHandler {
        public string ServiceName {
            get { return "Mkv Packager Plugin"; }
        }

        public void MovieDownloaded(Movie movie) {
        }

        public void MovieSubtitleDownloaded(Movie movie) {
            MkvPackage(movie);
        }

        public void TvShowEpisodeDownloaded(TvShowEpisode episode) {
        }

        public void TvShowEpisodeSubtitleDownloaded(TvShowEpisode episode) {
            MkvPackage(episode);
        }

        private void MkvPackage(IDownloadable media) {
            var outputPath = Path.ChangeExtension(media.FilePath, "mkv");

            if (string.IsNullOrEmpty(media.FilePath) || File.Exists(media.FilePath)) return;

            var mediafileName = Path.GetFileName(media.FilePath);
            if (string.IsNullOrEmpty(mediafileName)) return;

            var directory = Path.GetDirectoryName(media.FilePath);
            if (string.IsNullOrEmpty(directory)) return;

            var mkvPackageDirectory = Directory.CreateDirectory(Path.Combine(directory, "MkvPackageContent"));
            var subtitleFilePath = Directory.GetFiles(directory).FirstOrDefault(Helper.IsSubtitleFile);
            if (string.IsNullOrEmpty(subtitleFilePath)) return;
            var subtitleFileName = Path.GetFileName(subtitleFilePath);


            new FileInfo(media.FilePath).MoveTo(Path.Combine(mkvPackageDirectory.FullName, mediafileName));
            new FileInfo(subtitleFilePath).MoveTo(Path.Combine(mkvPackageDirectory.FullName, subtitleFileName));

            var mediaFilePathNew = Path.Combine(mkvPackageDirectory.FullName, mediafileName);
            var subtitleFilePathNew = Path.Combine(mkvPackageDirectory.FullName, subtitleFileName);

            MkvMerge(mediaFilePathNew, subtitleFilePathNew, outputPath);

            media.FilePath = mediaFilePathNew;
            media.BackgroundDownload = false;
            media.SubtitleDownloaded = false;

            File.Delete(mediaFilePathNew);
            File.Delete(subtitleFilePathNew);
            Directory.Delete(mkvPackageDirectory.FullName);
        }

        private void MkvMerge(string videoInputPath, string subtitleInputPath, string mkvOutputPath) {
            var mkvMergeExePath = Path.Combine(Environment.CurrentDirectory, "mkvmerge.exe");
            var process = new Process();
            var startInfo = new ProcessStartInfo {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = mkvMergeExePath
            };

            var parameters = string.Format(" -o \"{0}\" \"{1}\" \"{2}\"", mkvOutputPath, videoInputPath, subtitleInputPath);
            startInfo.Arguments = parameters;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

    }
}
