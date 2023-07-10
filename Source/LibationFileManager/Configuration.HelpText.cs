using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LibationFileManager
{
    public partial class Configuration
    {
        public static ReadOnlyDictionary<string, string> HelpText { get; } = new Dictionary<string, string>
        {
            { nameof(CombineNestedChapterTitles),"""
                If the book has nested chapters, e.g. a chapter named "Part 1"
                that contains chapters "Chapter 1" and "Chapter 2", then combine
                the chapter titles like the following example:

                Part 1: Chapter 1
                Part 1: Chapter 2
                """},
            {nameof(AllowLibationFixup), """
                In addition to the options that are enabled if you allow
                "fixing up" the audiobook, it does the following:
                
                * Sets the ©gen metadata tag for the genres.
                * Adds the TCOM (@wrt in M4B files) metadata tag for the narrators.
                * Unescapes the copyright symbol (replace &#169; with ©)
                * Replaces the recording copyright (P) string with ℗
                * Adds various other metadata tags recognized by AudiobookShelf
                * Sets the embedded cover art image with cover art retrieved from Audible
                """ },
        }
        .AsReadOnly();

        public static string GetHelpText(string settingName)
            => HelpText.TryGetValue(settingName, out var value) ? value : null;

	}
}
