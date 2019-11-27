using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using TagLib;
using TagLib.Mpeg4;
using Dinah.Core;

namespace AaxDecrypter
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

        public Tags(string file)
        {
			using TagLib.File tagLibFile = TagLib.File.Create(file, "audio/mp4", ReadStyle.Average);
			this.title = tagLibFile.Tag.Title.Replace(" (Unabridged)", "");
			this.album = tagLibFile.Tag.Album.Replace(" (Unabridged)", "");
			this.author = tagLibFile.Tag.FirstPerformer ?? "[unknown]";
			this.year = tagLibFile.Tag.Year.ToString();
			this.comments = tagLibFile.Tag.Comment;
			this.duration = tagLibFile.Properties.Duration;
			this.genre = tagLibFile.Tag.FirstGenre;

			var tag = tagLibFile.GetTag(TagTypes.Apple, true);
			this.publisher = tag.Publisher;
			this.narrator = string.IsNullOrWhiteSpace(tagLibFile.Tag.FirstComposer) ? tag.Narrator : tagLibFile.Tag.FirstComposer;
			this.comments = !string.IsNullOrWhiteSpace(tag.LongDescription) ? tag.LongDescription : tag.Description;
			this.id = tag.AudibleCDEK;
		}

        public void AddAppleTags(string file)
        {
			using var file1 = TagLib.File.Create(file, "audio/mp4", ReadStyle.Average);
			var tag = (AppleTag)file1.GetTag(TagTypes.Apple, true);
			tag.Publisher = this.publisher;
			tag.LongDescription = this.comments;
			tag.Description = this.comments;
			file1.Save();
		}

        public string GenerateFfmpegTags()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(";FFMETADATA1\n");
            stringBuilder.Append("major_brand=aax\n");
            stringBuilder.Append("minor_version=1\n");
            stringBuilder.Append("compatible_brands=aax M4B mp42isom\n");
            stringBuilder.Append("date=" + this.year + "\n");
            stringBuilder.Append("genre=" + this.genre + "\n");
            stringBuilder.Append("title=" + this.title + "\n");
            stringBuilder.Append("artist=" + this.author + "\n");
            stringBuilder.Append("album=" + this.album + "\n");
            stringBuilder.Append("composer=" + this.narrator + "\n");
            stringBuilder.Append("comment=" + this.comments.Truncate(254) + "\n");
            stringBuilder.Append("description=" + this.comments + "\n");

            return stringBuilder.ToString();
        }
    }
}
