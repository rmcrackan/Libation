using System;
using System.Diagnostics;
using System.IO;
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

        private static Regex processedTimeRegex = new Regex("time=(\\d{2}):(\\d{2}):(\\d{2}).\\d{2}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public FFMpegAaxcProcesser(string ffmpegPath)
        {
            FFMpegPath = ffmpegPath;
        }

        public async Task ProcessBook(string aaxcUrl, string userAgent, string audibleKey, string audibleIV, string metadataPath, string outputFile)
        {

            //This process gets the aaxc from the url and streams the decrypted
            //m4b to the output file. Preserves album art, but replaces metadata.
            var downloader = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = FFMpegPath,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(FFMpegPath),
                    ArgumentList =
                    {
                        "-ignore_chapters", //prevents ffmpeg from copying chapter info from aaxc to output file
                        "true",
                        "-audible_key",
                        audibleKey,
                        "-audible_iv",
                        audibleIV,
                        "-user_agent",
                        userAgent,
                        "-i",
                        aaxcUrl,
                        "-f",
                        "ffmetadata",
                        "-i",
                        metadataPath,
                        "-map_metadata",
                        "1",
                        "-c", //audio codec
                        "copy", //copy stream
                        "-f", //force output format: adts
                        "mp4",
                        outputFile, //pipe output to standard output
                        "-y"
                    }
                }
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
