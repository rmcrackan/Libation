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

        /// <summary>
        /// proper bools are single keywords which are turned into keyword:True
        /// if bordered by colons or inside brackets, they are not stand-alone bool keywords
        /// the negative lookbehind and lookahead patterns prevent bugs where a bool keyword is also a user-defined tag:
        ///   [israted]
        ///     parseTag => tags:israted
        ///     replaceBools => tags:israted:True
        ///   or
        ///     [israted]
        ///       replaceBools => israted:True
        ///         parseTag => [israted:True]
        /// also don't want to apply :True where the value already exists:
        ///   israted:false => israted:false:True
        ///   
        /// despite using parans, lookahead and lookbehind are zero-length assertions which do not capture. therefore the bool search keyword is still $1 since it's the first and only capture
        /// </summary>
        private static string boolPattern_parameterized { get; }
            = @"
### IMPORTANT: 'ignore whitespace' is only partially honored in character sets
### - new lines are ok
### - ANY leading whitespace is treated like actual matching spaces  :(

                    ### can't begin with colon. incorrect syntax
                    ### can't begin with open bracket: this signals the start of a tag
(?<!                # begin negative lookbehind
  [:\[]             #   char set: colon and open bracket, escaped
  \s*               #   optional space
)                   # end negative lookbehind

\b                  # word boundary
  ({0})             #   captured bool search keyword. this is the $1 reference used in regex.Replace
\b                  # word boundary

                    ### can't end with colon. this signals that the bool's value already exists
                    ### can't begin with close bracket: this signals the end of a tag
(?!                 # begin negative lookahead
  \s*               #   optional space
  [:\]]             #   char set: colon and close bracket, escaped
)                   # end negative lookahead
";
        private static Dictionary<string, Regex> boolRegexDic { get; } = new Dictionary<string, Regex>();
        public static Regex GetBoolRegex(string boolSearch)
        {
            if (boolRegexDic.TryGetValue(boolSearch, out var regex))
                return regex;

            var boolPattern = string.Format(boolPattern_parameterized, boolSearch);
            regex = new Regex(boolPattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Compiled);
            boolRegexDic.Add(boolSearch, regex);

            return regex;
        }
    }
}
