using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AaxDecrypter
{

    /// <summary>
    /// Download audible aaxc, decrypt, and remux with chapters.
    /// </summary>
    class FFMpegAaxcProcesser
    {
        public event EventHandler<TimeSpan> ProgressUpdate;
        public string FFMpegPath { get; }
        public DownloadLicense DownloadLicense { get; }
        public bool IsRunning { get; private set; }
        public bool Succeeded { get; private set; }
        public string FFMpegRemuxerStandardError => remuxerError.ToString();
        public string FFMpegDownloaderStandardError => downloaderError.ToString();


        private StringBuilder remuxerError = new StringBuilder();
        private StringBuilder downloaderError = new StringBuilder();
        private static Regex processedTimeRegex = new Regex("time=(\\d{2}):(\\d{2}):(\\d{2}).\\d{2}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public FFMpegAaxcProcesser( DownloadLicense downloadLicense)
        {
            FFMpegPath = DecryptSupportLibraries.ffmpegPath;
            DownloadLicense = downloadLicense;
        }

        public async Task ProcessBook(string outputFile, string ffmetaChaptersPath = null)
        {
            //This process gets the aaxc from the url and streams the decrypted
            //aac stream to standard output
            var downloader = new Process
            {
                StartInfo = getDownloaderStartInfo()
            };

            //This process retreves an aac stream from standard input and muxes
            // it into an m4b along with the cover art and metadata.
            var remuxer = new Process
            {
                StartInfo = getRemuxerStartInfo(outputFile, ffmetaChaptersPath)
            };

            IsRunning = true;

            downloader.ErrorDataReceived += Downloader_ErrorDataReceived;
            downloader.Start();
            downloader.BeginErrorReadLine();

            remuxer.ErrorDataReceived += Remuxer_ErrorDataReceived;
            remuxer.Start();
            remuxer.BeginErrorReadLine();

            var pipedOutput = downloader.StandardOutput.BaseStream;
            var pipedInput = remuxer.StandardInput.BaseStream;


            //All the work done here. Copy download standard output into
            //remuxer standard input
            await Task.Run(() =>
            {
                int lastRead = 0;
                byte[] buffer = new byte[32 * 1024];

                do
                {
                    lastRead = pipedOutput.Read(buffer, 0, buffer.Length);
                    pipedInput.Write(buffer, 0, lastRead);
                } while (lastRead > 0 && !remuxer.HasExited);
            });

            //Closing input stream terminates remuxer
            pipedInput.Close();

            //If the remuxer exited due to failure, downloader will still have
            //data in the pipe. Force kill downloader to continue.
            if (remuxer.HasExited && !downloader.HasExited)
                downloader.Kill();

            remuxer.WaitForExit();
            downloader.WaitForExit();

            IsRunning = false;
            Succeeded = downloader.ExitCode == 0 && remuxer.ExitCode == 0;
        }

        private void Downloader_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            downloaderError.AppendLine(e.Data);            
        }

        private void Remuxer_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            remuxerError.AppendLine(e.Data);

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
                //This happens if input is corrupt (should never happen) or if caller
                //supplied wrong key/iv
                var process = sender as Process;
                process.Kill();
            }
        }

        private ProcessStartInfo getDownloaderStartInfo() =>
         new ProcessStartInfo
         {
             FileName = FFMpegPath,
             RedirectStandardError = true,
             RedirectStandardOutput = true,
             CreateNoWindow = true,
             WindowStyle = ProcessWindowStyle.Hidden,
             UseShellExecute = false,
             WorkingDirectory = Path.GetDirectoryName(FFMpegPath),
             ArgumentList ={
                    "-nostdin",
                    "-audible_key",
                    DownloadLicense.AudibleKey,
                    "-audible_iv",
                    DownloadLicense.AudibleIV,
                    "-user_agent",
                    DownloadLicense.UserAgent, //user-agent is requied for CDN to serve the file
                    "-i",
                    DownloadLicense.DownloadUrl, 
                    "-c:a", //audio codec
                    "copy", //copy stream
                    "-f", //force output format: adts
                    "adts",
                    "pipe:" //pipe output to stdout
                }
         };

        private ProcessStartInfo getRemuxerStartInfo(string outputFile, string ffmetaChaptersPath = null)
        {
           var startInfo = new ProcessStartInfo
            {
                FileName = FFMpegPath,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(FFMpegPath),
            };

            startInfo.ArgumentList.Add("-thread_queue_size");
            startInfo.ArgumentList.Add("1024");
            startInfo.ArgumentList.Add("-f"); //force input format: aac
            startInfo.ArgumentList.Add("aac");
            startInfo.ArgumentList.Add("-i"); //read input from stdin
            startInfo.ArgumentList.Add("pipe:");

            if (ffmetaChaptersPath is null)
            {
                //copy metadata from aaxc file.
                startInfo.ArgumentList.Add("-user_agent");
                startInfo.ArgumentList.Add(DownloadLicense.UserAgent);
                startInfo.ArgumentList.Add("-i");
                startInfo.ArgumentList.Add(DownloadLicense.DownloadUrl);
            }
            else
            {
                //copy metadata from supplied metadata file
                startInfo.ArgumentList.Add("-f");
                startInfo.ArgumentList.Add("ffmetadata");
                startInfo.ArgumentList.Add("-i");
                startInfo.ArgumentList.Add(ffmetaChaptersPath);
            }

            startInfo.ArgumentList.Add("-map"); //map file 0 (aac audio stream)
            startInfo.ArgumentList.Add("0");
            startInfo.ArgumentList.Add("-map_chapters"); //copy chapter data from file 1 (either metadata file or aaxc file)
            startInfo.ArgumentList.Add("1");
            startInfo.ArgumentList.Add("-c"); //copy all mapped streams
            startInfo.ArgumentList.Add("copy");
            startInfo.ArgumentList.Add("-f"); //force output format: mp4
            startInfo.ArgumentList.Add("mp4");
            startInfo.ArgumentList.Add("-movflags"); 
            startInfo.ArgumentList.Add("disable_chpl"); //Disable Nero chapters format
            startInfo.ArgumentList.Add(outputFile);
            startInfo.ArgumentList.Add("-y"); //overwrite existing

            return startInfo;
        }
    }
}