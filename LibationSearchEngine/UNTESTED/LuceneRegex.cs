using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibationSearchEngine
{
    internal static class LuceneRegex
    {
        #region pattern pieces
        //  negative lookbehind: cannot be preceeded by an escaping \
        const string NOT_ESCAPED = @"(?<!\\)";

        // disallow spaces and lucene reserved characters
        //     + - && || ! ( ) { } [ ] ^ " ~ * ? : \
        // define chars
        // escape and concat
        // create regex. also disallow spaces
        private static char[] disallowedChars { get; } = new[] {
            '+', '-', '&', '|', '!', '(', ')', '{', '}', '[', ']', '^', '"', '~', '*', '?', ':', '\\' };
        private static string disallowedCharsEscaped { get; } = disallowedChars.Select(c => $@"\{c}").Aggregate((a, b) => a + b);
        private static string WORD_CAPTURE { get; } = $@"([^\s{disallowedCharsEscaped}]+)";

        // : with optional preceeding spaces. capture these so i don't accidentally replace a non-field name
        const string FIELD_END = @"(\s*:)";

        const string BEGIN_TAG = @"\[";
        const string END_TAG = @"\]";

        // space is forgiven at beginning and end of tag but not in the middle
        // literal space character only. do NOT allow new lines, tabs, ...
        const string OPTIONAL_SPACE_LITERAL = @"\u0020*";
        #endregion

        private static string tagPattern { get; } = NOT_ESCAPED + BEGIN_TAG + OPTIONAL_SPACE_LITERAL + WORD_CAPTURE + OPTIONAL_SPACE_LITERAL + END_TAG;
        public static Regex TagRegex { get; } = new Regex(tagPattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static string fieldPattern { get; } = NOT_ESCAPED + WORD_CAPTURE + FIELD_END;
        public static Regex FieldRegex { get; } = new Regex(fieldPattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        // auto-pad numbers to 8 char.s. This will match int.s and dates (yyyyMMdd)
        //   positive look behind: beginning  space  {  [  :
        //   positive look ahead: end  space  ]  }
        public static Regex NumbersRegex { get; } = new Regex(@"(?<=^|\s|\{|\[|:)(\d+\.?\d*)(?=$|\s|\]|\})", RegexOptions.Compiled);
    }
}
