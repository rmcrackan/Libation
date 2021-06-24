using System;
using TagLib;
using TagLib.Mpeg4;
using Dinah.Core;

namespace FileLiberator.AaxcDownloadDecrypt
{
    public class Tags
    {
        public string title { get; }
        public string album { get; }
        public string author { get; }
        public string comments { get; }
        public string narrator { get; }
        public string year { get; }
        public string publisher { get; }
        public string id { get; }
        public string genre { get; }
        public TimeSpan duration { get; }
        public int channels { get; }
        public int bitrate { get; }
        public int sampleRate { get; }

        public bool hasCoverArt { get; }
        public byte[] coverArt { get; }

        // input file
        public Tags(TagLib.File tagLibFile)
        {
            title = tagLibFile.Tag.Title?.Replace(" (Unabridged)", "");
            album = tagLibFile.Tag.Album?.Replace(" (Unabridged)", "");
            author = tagLibFile.Tag.FirstPerformer ?? "[unknown]";
            year = tagLibFile.Tag.Year.ToString();
            comments = tagLibFile.Tag.Comment ?? "";
            genre = tagLibFile.Tag.FirstGenre ?? "";

            var tag = tagLibFile.GetTag(TagTypes.Apple, true);
            publisher = tag.Publisher ?? "";
            narrator = string.IsNullOrWhiteSpace(tagLibFile.Tag.FirstComposer) ? tag.Narrator : tagLibFile.Tag.FirstComposer;
            comments = !string.IsNullOrWhiteSpace(tag.LongDescription) ? tag.LongDescription : tag.Description;
            id = tag.AudibleCDEK;

            hasCoverArt = tagLibFile.Tag.Pictures.Length > 0;
            if (hasCoverArt)
                coverArt = tagLibFile.Tag.Pictures[0].Data.Data;

            duration = tagLibFile.Properties.Duration;

            bitrate = tagLibFile.Properties.AudioBitrate;
            channels = tagLibFile.Properties.AudioChannels;
            sampleRate = tagLibFile.Properties.AudioSampleRate;
        }

        // my best guess of what this step is doing:
        // re-publish the data we read from the input file => output file
        public void AddAppleTags(string file)
        {
            using var tagLibFile = TagLib.File.Create(file, "audio/mp4", ReadStyle.Average);
            var tag = (AppleTag)tagLibFile.GetTag(TagTypes.Apple, true);
			tag.Publisher = publisher;
			tag.LongDescription = comments;
			tag.Description = comments;
            tagLibFile.Save();
		}

        public string GenerateFfmpegTags()
            => $";FFMETADATA1"
            + $"\nmajor_brand=aax"
            + $"\nminor_version=1"
            + $"\ncompatible_brands=aax M4B mp42isom"
            + $"\ndate={year}"
            + $"\ngenre={genre}"
            + $"\ntitle={title}"
            + $"\nartist={author}"
            + $"\nalbum={album}"
            + $"\ncomposer={narrator}"
            + $"\ncomment={comments.Truncate(254)}"
            + $"\ndescription={comments}"
            + $"\n";
    }
}
