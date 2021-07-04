using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AaxDecrypter
{
    class AaxcProcessUpdate
    {
        public AaxcProcessUpdate(TimeSpan position, double speed)
        {
            ProcessPosition = position;
            ProcessSpeed = speed;
            EventTime = DateTime.Now;
        }
        public TimeSpan ProcessPosition { get; }
        public double ProcessSpeed { get; }
        public DateTime EventTime { get; }
    }

    /// <summary>
    /// Download audible aaxc, decrypt, and remux with chapters.
    /// </summary>
    class FFMpegAaxcProcesser
    {
        public event EventHandler<AaxcProcessUpdate> ProgressUpdate;
        public string FFMpegPath { get; }
        public DownloadLicense DownloadLicense { get; }
        public bool IsRunning { get; private set; }
        public bool Succeeded { get; private set; }
        public string FFMpegRemuxerStandardError => remuxerError.ToString();
        public string FFMpegDecrypterStandardError => decrypterError.ToString();


        private StringBuilder remuxerError { get; } = new StringBuilder();
        private StringBuilder decrypterError { get; } = new StringBuilder();
        private static Regex processedTimeRegex { get; } = new Regex("time=(\\d{2}):(\\d{2}):(\\d{2}).\\d{2}.*speed=\\s{0,1}([0-9]*[.]?[0-9]+)(?:e\\+([0-9]+)){0,1}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private Process decrypter;
        private Process remuxer;
        private Stream inputFile;
        private bool isCanceled = false;

        public FFMpegAaxcProcesser(DownloadLicense downloadLicense)
        {
            FFMpegPath = DecryptSupportLibraries.ffmpegPath;
            DownloadLicense = downloadLicense;
        }

        public async Task ProcessBook(Stream inputFile, string outputFile, string ffmetaChaptersPath)
        {
            this.inputFile = inputFile;

            //This process gets the aaxc from the url and streams the decrypted
            //aac stream to standard output
            decrypter = new Process
            {
                StartInfo = getDownloaderStartInfo()
            };

            //This process retreves an aac stream from standard input and muxes
            // it into an m4b along with the cover art and metadata.
            remuxer = new Process
            {
                StartInfo = getRemuxerStartInfo(outputFile, ffmetaChaptersPath)
            };

            IsRunning = true;

            decrypter.ErrorDataReceived += Downloader_ErrorDataReceived;
            decrypter.Start();
            decrypter.BeginErrorReadLine();

            remuxer.ErrorDataReceived += Remuxer_ErrorDataReceived;
            remuxer.Start();
            remuxer.BeginErrorReadLine();

            //Thic check needs to be placed after remuxer has started.
            if (isCanceled) return;

            var decrypterInput = decrypter.StandardInput.BaseStream;
            var decrypterOutput = decrypter.StandardOutput.BaseStream;
            var remuxerInput = remuxer.StandardInput.BaseStream;

            //Read inputFile into decrypter stdin in the background
            var t = new Thread(() => CopyStream(inputFile, decrypterInput, decrypter));           
            t.Start();

            //All the work done here. Copy download standard output into
            //remuxer standard input
            await Task.Run(() => CopyStream(decrypterOutput, remuxerInput, remuxer));

            //If the remuxer exited due to failure, downloader will still have
            //data in the pipe. Force kill downloader to continue.
            if (remuxer.HasExited && !decrypter.HasExited)
                decrypter.Kill();

            remuxer.WaitForExit();
            decrypter.WaitForExit();

            IsRunning = false;
            Succeeded = decrypter.ExitCode == 0 && remuxer.ExitCode == 0;
        }

        private void CopyStream(Stream inputStream, Stream outputStream, Process returnOnProcExit)
        {
            try
            {
                byte[] buffer = new byte[32 * 1024];
                int lastRead;
                do
                {
                    lastRead = inputStream.Read(buffer, 0, buffer.Length);
                    outputStream.Write(buffer, 0, lastRead);
                } while (lastRead > 0 && !returnOnProcExit.HasExited);
            }
            catch (IOException ex)
            { 
                //There is no way to tell if the process closed the input stream
                //before trying to write to it. If it did close, throws IOException.
                isCanceled = true; 
            }
            finally
            {
                outputStream.Close();
            }
        }

        public void Cancel()
        {
            isCanceled = true;

            if (IsRunning && !remuxer.HasExited)
                remuxer.Kill();
            if (IsRunning && !decrypter.HasExited)
                decrypter.Kill();
            inputFile?.Close();
        }
        private void Downloader_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            decrypterError.AppendLine(e.Data);            
        }

        private void Remuxer_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            remuxerError.AppendLine(e.Data);

            if (processedTimeRegex.IsMatch(e.Data))
            {
                //get timestamp of of last processed audio stream position
                //and processing speed
                var match = processedTimeRegex.Match(e.Data);

                int hours = int.Parse(match.Groups[1].Value);
                int minutes = int.Parse(match.Groups[2].Value);
                int seconds = int.Parse(match.Groups[3].Value);

                var position = new TimeSpan(hours, minutes, seconds);

                double speed = double.Parse(match.Groups[4].Value);
                int exp = match.Groups[5].Success ? int.Parse(match.Groups[5].Value) : 0;
                speed *= Math.Pow(10, exp);

                ProgressUpdate?.Invoke(this, new AaxcProcessUpdate(position, speed));
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
             RedirectStandardInput = true,
             RedirectStandardOutput = true,
             CreateNoWindow = true,
             WindowStyle = ProcessWindowStyle.Hidden,
             UseShellExecute = false,
             WorkingDirectory = Path.GetDirectoryName(FFMpegPath),
             ArgumentList ={
                    "-audible_key",
                    DownloadLicense.AudibleKey,
                    "-audible_iv",
                    DownloadLicense.AudibleIV,
                    "-f",
                    "mp4",
                    "-i",
                    "pipe:",
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

            //copy metadata from supplied metadata file
            startInfo.ArgumentList.Add("-f");
            startInfo.ArgumentList.Add("ffmetadata");
            startInfo.ArgumentList.Add("-i");
            startInfo.ArgumentList.Add(ffmetaChaptersPath);

            startInfo.ArgumentList.Add("-map"); //map file 0 (aac audio stream)
            startInfo.ArgumentList.Add("0");
            startInfo.ArgumentList.Add("-map_chapters"); //copy chapter data from file metadata file
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