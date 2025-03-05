using System.Collections.Generic;
using System.Collections.ObjectModel;

#nullable enable
namespace LibationFileManager
{
    public partial class Configuration
    {
        private static ReadOnlyDictionary<string, string> HelpText { get; } = new Dictionary<string, string>
        {
            {nameof(CombineNestedChapterTitles),"""
                If the book has nested chapters, e.g. a chapter named
                "Part 1" that contains chapters "Chapter 1" and
                "Chapter 2", then combine the chapter titles like the
                following example:

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
            {nameof(MoveMoovToBeginning), """
                Moves the mpeg 'moov' box to the beginning of the file.
                Using this option will generally make the audiobook load
                faster, and will make streaming the file over the internet
                faster.

                This is an extra operation performed after the m4b file
                has been created, and the speed of it can vary greatly
                depending on how fast Libation can read and write from the
                book storage location.
                """ },
            {nameof(LameDownsampleMono), """
                Most "stereo" audiobooks just duplicate the same audio
                for both channels, so you can save on storage size and
                decrease encoding time by only using one audio channel.
                """ },
            {nameof(DecryptToLossy), """
                Audible delivers its audiobooks in the mpeg-4 audio
                file format (aka M4B). If you choose the "Lossless"
                option, Libation will leave the original Audible audio
                untouched. If you choose "MP3", Libation will re-
                encode the audio as an MP3 using the settings below.

                Note that podcasts are usually delivered as MP3s.
                """ },
            {nameof(MergeOpeningAndEndCredits), """
                This setting only affects the chapter metadata.
                In most audiobooks, the first chapter is "Opening
                Credits" and the last chapter is "End Credits".
                Enabling this option will remove the credits chapter
                markers and shift the adjacent chapter markers to
                fill the space.
                """ },
            {nameof(RetainAaxFile), """
                Libation will keep the Audible source aax file
                and move it to the book's destination directory.
                Libation will also create a .key file containing
                the decryption key and IV.
                """ },
            {nameof(StripUnabridged), """
                Many audiobooks contain "(Unabridged)" in the title.
                Enabling this option will remove that text from the
                Title and Album metadata tags.
                """ },
            {nameof(StripAudibleBrandAudio), """
                All audiobooks begin and end with a few seconds of
                Audible branding audio. In English it's "This is
                Audible" and "Audible hopes you have enjoyed this
                program".

                Enabling this option will remove that branded audio
                from the decrypted audiobook. This does not require
                re-encoding.
                """ },
        }
        .AsReadOnly();

        public static string GetHelpText(string? settingName)
            => settingName != null && HelpText.TryGetValue(settingName, out var value) ? value : "";
    }
}
