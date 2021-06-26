
namespace AaxDecrypter
{
    public static class NFO
    {
        public static string CreateContents(string ripper, TagLib.File tags, ChapterInfo chapters)
        {
            var tag = tags.GetTag(TagLib.TagTypes.Apple);

            string narator = string.IsNullOrWhiteSpace(tags.Tag.FirstComposer) ? tag.Narrator : tags.Tag.FirstComposer;

            var _hours = (int)tags.Properties.Duration.TotalHours;
            var myDuration
                = (_hours > 0 ? _hours + " hours, " : "")
                + tags.Properties.Duration.Minutes + " minutes, "
                + tags.Properties.Duration.Seconds + " seconds";

            var header
                = "General Information\r\n"
                + "===================\r\n"
                + $" Title:                  {tags.Tag.Title.Replace(" (Unabridged)", "")}\r\n"
                + $" Author:                 {tags.Tag.FirstPerformer ?? "[unknown]"}\r\n"
                + $" Read By:                {tags.Tag.FirstPerformer??"[unknown]"}\r\n"
                + $" Copyright:              {tags.Tag.Year}\r\n"
                + $" Audiobook Copyright:    {tags.Tag.Year}\r\n";
            if (!string.IsNullOrEmpty(tags.Tag.FirstGenre))
                header += $" Genre:                  {tags.Tag.FirstGenre}\r\n";

            var s
                = header
                + $" Publisher:              {tag.Publisher ?? ""}\r\n"
                + $" Duration:               {myDuration}\r\n"
                + $" Chapters:               {chapters.Count}\r\n"
                + "\r\n"
                + "\r\n"
                + "Media Information\r\n"
                + "=================\r\n"
                + " Source Format:          Audible AAX\r\n"
                + $" Source Sample Rate:     {tags.Properties.AudioSampleRate} Hz\r\n"
                + $" Source Channels:        {tags.Properties.AudioChannels}\r\n"
                + $" Source Bitrate:         {tags.Properties.AudioBitrate} kbits\r\n"
                + "\r\n"
                + " Lossless Encode:        Yes\r\n"
                + " Encoded Codec:          AAC / M4B\r\n"
                + $" Encoded Sample Rate:    {tags.Properties.AudioSampleRate} Hz\r\n"
                + $" Encoded Channels:       {tags.Properties.AudioChannels}\r\n"
                + $" Encoded Bitrate:        {tags.Properties.AudioBitrate} kbits\r\n"
                + "\r\n"
                + $" Ripper:                 {ripper}\r\n"
                + "\r\n"
                + "\r\n"
                + "Book Description\r\n"
                + "================\r\n"
                + (!string.IsNullOrWhiteSpace(tag.LongDescription) ? tag.LongDescription : tag.Description);

            return s;
        }
    }
}
