using System;
using System.Text.RegularExpressions;

namespace ffmpeg_decrypt
{
    // from: http://csharptest.net/529/how-to-correctly-escape-command-line-arguments-in-c/index.html
    public static class Escaper
    {
        /// <summary>
        /// Quotes all arguments that contain whitespace, or begin with a quote and returns a single
        /// argument string for use with Process.Start().
        /// </summary>
        /// <param name="args">A list of strings for arguments, may not contain null, '\0', '\r', or '\n'</param>
        /// <returns>The combined list of escaped/quoted strings</returns>
        /// <exception cref="System.ArgumentNullException">Raised when one of the arguments is null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Raised if an argument contains '\0', '\r', or '\n'</exception>
        public static string EscapeArguments(params string[] args)
        {
            var arguments = new System.Text.StringBuilder();
            var invalidChar = new Regex("[\x00\x0a\x0d]");//  these can not be escaped
            var needsQuotes = new Regex(@"\s|""");//          contains whitespace or two quote characters
            var escapeQuote = new Regex(@"(\\*)(""|$)");//    one or more '\' followed with a quote or end of string

            for (int carg = 0; args != null && carg < args.Length; carg++)
            {
                if (args[carg] == null)
                    throw new ArgumentNullException("args[" + carg + "]");

                if (invalidChar.IsMatch(args[carg]))
                    throw new ArgumentOutOfRangeException("args[" + carg + "]");

                if (args[carg] == string.Empty)
                    arguments.Append("\"\"");
                else if (!needsQuotes.IsMatch(args[carg]))
                    arguments.Append(args[carg]);
                else
                {
                    arguments.Append('"');
                    arguments.Append(escapeQuote.Replace(args[carg], m =>
                        m.Groups[1].Value + m.Groups[1].Value +
                        (m.Groups[2].Value == "\"" ? "\\\"" : "")
                    ));
                    arguments.Append('"');
                }

                if (carg + 1 < args.Length)
                    arguments.Append(' ');
            }
            return arguments.ToString();
        }
    }
}
