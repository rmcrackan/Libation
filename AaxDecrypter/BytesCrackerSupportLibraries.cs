using System.IO;

namespace AaxDecrypter
{
    public static class BytesCrackerSupportLibraries
    {
        // GetActivationBytes dependencies
        //   rcrack.exe
        //   alglib1.dll
        //   RainbowCrack files to recover your own Audible activation data (activation_bytes) in an offline manner
        //   audible_byte#4-4_0_10000x789935_0.rtc
        //   audible_byte#4-4_1_10000x791425_0.rtc
        //   audible_byte#4-4_2_10000x790991_0.rtc
        //   audible_byte#4-4_3_10000x792120_0.rtc
        //   audible_byte#4-4_4_10000x790743_0.rtc
        //   audible_byte#4-4_5_10000x790568_0.rtc
        //   audible_byte#4-4_6_10000x791458_0.rtc
        //   audible_byte#4-4_7_10000x791707_0.rtc
        //   audible_byte#4-4_8_10000x790202_0.rtc
        //   audible_byte#4-4_9_10000x791022_0.rtc

        private static string appPath_ { get; } = Path.GetDirectoryName(Dinah.Core.Exe.FileLocationOnDisk);
        private static string bytesCrackerLib_ { get; } = Path.Combine(appPath_, "BytesCrackerLib");

        public static string ffprobePath { get; } = Path.Combine(bytesCrackerLib_, "ffprobe.exe");
        public static string rcrackPath { get; } = Path.Combine(bytesCrackerLib_, "rcrack.exe");
    }
}
