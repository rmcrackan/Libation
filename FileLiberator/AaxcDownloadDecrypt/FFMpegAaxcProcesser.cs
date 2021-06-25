using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileLiberator.AaxcDownloadDecrypt
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
            //aac stream to standard output
            var downloader = new Process
            {
                StartInfo = getDownloaderStartInfo(aaxcUrl, userAgent, audibleKey, audibleIV)
            };

            //This process retreves an aac stream from standard input and muxes
            // it into an m4b along with the cover art and metadata.
            var remuxer = new Process
            {
                StartInfo = getRemuxerStartInfo(metadataPath, outputFile)
            };

            IsRunning = true;

            remuxer.ErrorDataReceived += Remuxer_ErrorDataReceived;

            downloader.Start();

            var pipedOutput = downloader.StandardOutput.BaseStream;

            remuxer.Start();
            remuxer.BeginErrorReadLine();

            var pipedInput = remuxer.StandardInput.BaseStream;

            int lastRead = 0;

            byte[] buffer = new byte[16 * 1024];


            //All the work done here. Copy download standard output into
            //remuxer standard input
            do
            {
                lastRead = await pipedOutput.ReadAsync(buffer, 0, buffer.Length);
                await pipedInput.WriteAsync(buffer, 0, lastRead);
            } while (lastRead > 0 && !remuxer.HasExited);

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
        private void Remuxer_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data) && processedTimeRegex.IsMatch(e.Data))
            {
                //get timestamp of of last processed audio stream position
                var match = processedTimeRegex.Match(e.Data);

                int hours = int.Parse(match.Groups[1].Value);
                int minutes = int.Parse(match.Groups[2].Value);
                int seconds = int.Parse(match.Groups[3].Value);

                var position = new TimeSpan(hours, minutes, seconds);

                ProgressUpdate?.Invoke(sender, position);
            }
        }

        private ProcessStartInfo getDownloaderStartInfo(string aaxcUrl, string userAgent, string audibleKey, string audibleIV) =>
         new ProcessStartInfo
            {
                FileName = FFMpegPath,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(FFMpegPath),
                ArgumentList ={
                    "-nostdin",
                    "-audible_key",
                    audibleKey,
                    "-audible_iv",
                    audibleIV,
                    "-i",
                    aaxcUrl,
                    "-user_agent",
                    userAgent, //user-agent is requied for CDN to serve the file
                    "-c:a", //audio codec
                    "copy", //copy stream
                    "-f", //force output format: adts
                    "adts",
                    "pipe:" //pipe output to standard output
                }
            };

        private ProcessStartInfo getRemuxerStartInfo(string metadataPath, string outputFile) =>
         new ProcessStartInfo
         {
             FileName = FFMpegPath,
             RedirectStandardError = true,
             RedirectStandardInput = true,
             CreateNoWindow = true,
             WindowStyle = ProcessWindowStyle.Hidden,
             UseShellExecute = false,
             WorkingDirectory = Path.GetDirectoryName(FFMpegPath),

             ArgumentList =
                {   
                    "-thread_queue_size",
                    "1024",
                    "-f", //force input format: aac
                    "aac",
                    "-i",
                    "pipe:", //input from standard input
                    "-i",
                    metadataPath,
                    "-map",
                    "0",
                    "-map_metadata",
                    "1",
                    "-c", //codec copy
                    "copy",                    
                    "-f", //force output format: mp4
                    "mp4",
                    outputFile,
                    "-y" //overwritte existing
             }
         };
    }
}
