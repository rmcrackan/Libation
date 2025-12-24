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
            {nameof(SpatialAudioCodec), """
                The Dolby Digital Plus (E-AC-3) codec is more widely
                supported than the AC-4 codec, but E-AC-3 files are
                much larger than AC-4 files.
                """ },
			{nameof(UseWidevine), """
                Some audiobooks are only delivered in the highest
                available quality with special, third-party content
                protection. Enabling this option will allows you to
                request audiobooks in the xHE-AAC codec and in
                spatial (Dolby Atmos) audio formats.
                """ },
			{nameof(Request_xHE_AAC), """
                If selected, Libation will request audiobooks in the
                xHE-AAC codec. This codec is generally better quality
                than AAC-LC codec (which is what you'll get if this
                option isn't enabled), but it isn't as commonly
                supported by media players, so you may have some
                difficulty playing these audiobooks.
                """ },
            {nameof(RequestSpatial), """
                If selected, Libation will request audiobooks in the
                Dolby Atmos 'Spatial Audio' format. Audiobooks which
                don't have a spatial audio version will be download
                as usual based on your other audio format settings.
                """ },
            {"LocateAudiobooks","""
                Scan the contents a folder to find audio files that
                match books in Libation's database. This is useful
                if you moved your Books folder or re-installed 
                Libation and want it to be able to find your
                already downloaded audiobooks.

                Prerequisite: An audiobook must already exist in
                Libation's database (through an Audible account
                scan) for a matching audio file to be found.
                """ },
            {"LocateAudiobooksDialog","""
				Libation will search all .m4b and .mp3 files in a folder, looking for audio files belonging to library books in Libation's database.

				If an audiobook file is found that matches one of Libation's library books, Libation will mark that book as "Liberated" (green stoplight).

				For an audio file to be identified, Libation must have that library book in its database. If you're on a fresh installation of Libation, be sure to add and scan all of your Audible accounts before running this action.

				This may take a while, depending on the number of audio files in the folder and the speed of your storage device.
				""" }
        }.AsReadOnly();

        public static string GetHelpText(string? settingName)
            => settingName != null && HelpText.TryGetValue(settingName, out var value) ? value : "";
    }
}
