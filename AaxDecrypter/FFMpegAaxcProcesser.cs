using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AaxDecrypter
{
    /// <summary>
    /// Download audible aaxc, decrypt, remux,and add metadata.
    /// </summary>
    class FFMpegAaxcProcesser
    {
        public event EventHandler<TimeSpan> ProgressUpdate;
        public string FFMpegPath { get; }
        public bool IsRunning { get; private set; }
        public bool Succeeded { get; private set; }
        public string FFMpegStandardError => ffmpegError.ToString();


        private StringBuilder ffmpegError = new StringBuilder();
        private static Regex processedTimeRegex = new Regex("time=(\\d{2}):(\\d{2}):(\\d{2}).\\d{2}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public FFMpegAaxcProcesser(string ffmpegPath)
        {
            FFMpegPath = ffmpegPath;
        }
        public async Task ProcessBook(string aaxcUrl, string userAgent, string audibleKey, string audibleIV, string outputFile, string metadataPath = null)
        {
            //This process gets the aaxc from the url and streams the decrypted
            //m4b to the output file.
            var StartInfo = new ProcessStartInfo
            {
                FileName = FFMpegPath,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(FFMpegPath),
               
            };

            if (metadataPath != null)
            {
                StartInfo.ArgumentList.Add("-ignore_chapters"); //prevents ffmpeg from copying chapter info from aaxc to output file
                StartInfo.ArgumentList.Add("true");
            }

            StartInfo.ArgumentList.Add("-audible_key");
            StartInfo.ArgumentList.Add(audibleKey);
            StartInfo.ArgumentList.Add("-audible_iv");
            StartInfo.ArgumentList.Add(audibleIV);
            StartInfo.ArgumentList.Add("-user_agent");
            StartInfo.ArgumentList.Add(userAgent);
            StartInfo.ArgumentList.Add("-i");
            StartInfo.ArgumentList.Add(aaxcUrl);

            if (metadataPath != null)
            {
                StartInfo.ArgumentList.Add("-f");
                StartInfo.ArgumentList.Add("ffmetadata");
                StartInfo.ArgumentList.Add("-i");
                StartInfo.ArgumentList.Add(metadataPath);
                StartInfo.ArgumentList.Add("-map_metadata");
                StartInfo.ArgumentList.Add("1");
            }

            StartInfo.ArgumentList.Add("-c"); //copy all codecs to output
            StartInfo.ArgumentList.Add("copy");
            StartInfo.ArgumentList.Add("-f"); //force output format: mp4
            StartInfo.ArgumentList.Add("mp4");
            StartInfo.ArgumentList.Add("-movflags"); //don't add nero format chapter flags
            StartInfo.ArgumentList.Add("disable_chpl+faststart");
            StartInfo.ArgumentList.Add(outputFile);
            StartInfo.ArgumentList.Add("-y"); //overwrite existing file

            await ProcessBook(StartInfo);
        }

        private async Task ProcessBook(ProcessStartInfo startInfo)
        {
            var downloader = new Process
            {
                StartInfo = startInfo
            };

            IsRunning = true;

            downloader.ErrorDataReceived += Remuxer_ErrorDataReceived;
            downloader.Start();
            downloader.BeginErrorReadLine();

            //All the work done here. Copy download standard output into
            //remuxer standard input
            await downloader.WaitForExitAsync();

            IsRunning = false;
            Succeeded = downloader.ExitCode == 0;
        }

        private void Remuxer_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            ffmpegError.AppendLine(e.Data);

            if (processedTimeRegex.IsMatch(e.Data))
            {
                //get timestamp of of last processed audio stream position
                var match = processedTimeRegex.Match(e.Data);

                int hours = int.Parse(match.Groups[1].Value);
                int minutes = int.Parse(match.Groups[2].Value);
                int seconds = int.Parse(match.Groups[3].Value);

                var position = new TimeSpan(hours, minutes, seconds);

                ProgressUpdate?.Invoke(sender, position);
            }

            if (e.Data.Contains("aac bitstream error"))
            {
                var process = sender as Process;
                process.Kill();
            }
        }
    }
}
