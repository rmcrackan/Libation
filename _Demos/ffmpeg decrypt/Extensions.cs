using System;
using System.Windows.Forms;

namespace ffmpeg_decrypt
{
	public static class StringExt
    {
        public static string SurroundWithQuotes(this string str) => "\"" + str + "\"";

        public static string ExtractString(this string haystack, string before, int needleLength)
        {
            var index = haystack.IndexOf(before);
            var needle = haystack.Substring(index + before.Length, needleLength);

            return needle;
        }
    }

    public static class ControlExt
    {
        /// <summary>
        /// Executes the Action asynchronously on the UI thread, does not block execution on the calling thread.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="code"></param>
        public static void UIThread(this Control control, Action code)
        {
            if (control.InvokeRequired)
                control.BeginInvoke(code);
            else
                code.Invoke();
        }
    }
}
