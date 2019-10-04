using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Diagnostics;

namespace AaxDecrypter
{
    public static class BytesCracker
    {
        public static string GetChecksum(string aaxPath)
        {
            var info = new ProcessStartInfo
            {
                FileName = BytesCrackerSupportLibraries.ffprobePath,
                Arguments = aaxPath.SurroundWithQuotes(),
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            // checksum is in the debug info. ffprobe's debug info is written to stderr, not stdout
            var readErrorOutput = true;
            var ffprobeStderr = info.RunHidden(readErrorOutput).Output;

            // example checksum line:
            // ... [aax] file checksum == 0c527840c4f18517157eb0b4f9d6f9317ce60cd1
            var checksum = ffprobeStderr.ExtractString("file checksum == ", 40);

            return checksum;
        }

        /// <summary>use checksum to get activation bytes. activation bytes are unique per audible customer. only have to do this 1x/customer</summary>
        public static string GetActivationBytes(string checksum)
        {
            var info = new ProcessStartInfo
            {
                FileName = BytesCrackerSupportLibraries.rcrackPath,
                Arguments = @". -h " + checksum,
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            var rcrackStdout = info.RunHidden().Output;

            // example result
            // 0c527840c4f18517157eb0b4f9d6f9317ce60cd1  \xbd\x89X\x09  hex:bd895809
            var activation_bytes = rcrackStdout.ExtractString("hex:", 8);

            return activation_bytes;
        }
    }
}
