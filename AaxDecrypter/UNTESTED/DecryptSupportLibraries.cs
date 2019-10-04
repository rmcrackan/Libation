using System.IO;

namespace AaxDecrypter
{
    public static class DecryptSupportLibraries
    {
        // OTHER EXTERNAL DEPENDENCIES
        // ffprobe has these pre-req.s as I'm using it:
        // avcodec-57.dll, avdevice-57.dll, avfilter-6.dll, avformat-57.dll, avutil-55.dll, postproc-54.dll, swresample-2.dll, swscale-4.dll, taglib-sharp.dll
        //
        // something else needs the cygwin files (cyg*.dll)

        private static string appPath_ { get; } = Path.GetDirectoryName(Dinah.Core.Exe.FileLocationOnDisk);
        private static string decryptLib_ { get; } = Path.Combine(appPath_, "DecryptLib");

        public static string ffmpegPath { get; } = Path.Combine(decryptLib_, "ffmpeg.exe");
        public static string ffprobePath { get; } = Path.Combine(decryptLib_, "ffprobe.exe");
        public static string atomicParsleyPath { get; } = Path.Combine(decryptLib_, "AtomicParsley.exe");
        public static string mp4trackdumpPath { get; } = Path.Combine(decryptLib_, "mp4trackdump.exe");
    }
}
