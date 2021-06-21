using System.IO;

namespace AaxDecrypter
{
    public static class DecryptSupportLibraries
    {
        // OTHER EXTERNAL DEPENDENCIES
        // ffprobe has these pre-req.s as I'm using it:
        // avcodec-58.dll, avdevice-58.dll, avfilter-7.dll, avformat-58.dll, avutil-56.dll, postproc-54.dll, swresample-3.dll, swscale-5.dll, taglib-sharp.dll
        //
        // something else needs the cygwin files (cyg*.dll)

        private static string appPath_ { get; } = Path.GetDirectoryName(Dinah.Core.Exe.FileLocationOnDisk);
        private static string decryptLib_ { get; } = Path.Combine(appPath_, "DecryptLib");

        public static string ffmpegPath { get; } = Path.Combine(decryptLib_, "ffmpeg.exe");
        public static string ffprobePath { get; } = Path.Combine(decryptLib_, "ffprobe.exe");
        public static string atomicParsleyPath { get; } = Path.Combine(decryptLib_, "AtomicParsley.exe");
    }
}
