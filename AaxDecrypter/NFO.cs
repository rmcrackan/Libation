
namespace AaxDecrypter
{
    public static class NFO
    {
        public static string CreateContents(string ripper, TagLib.File aaxcTagLib, ChapterInfo chapters)
        {
            var tag = aaxcTagLib.GetTag(TagLib.TagTypes.Apple);

            string narator = string.IsNullOrWhiteSpace(aaxcTagLib.Tag.FirstComposer) ? tag.Narrator : aaxcTagLib.Tag.FirstComposer;

            var _hours = (int)aaxcTagLib.Properties.Duration.TotalHours;
            var myDuration
                = (_hours > 0 ? _hours + " hours, " : "")
                + aaxcTagLib.Properties.Duration.Minutes + " minutes, "
                + aaxcTagLib.Properties.Duration.Seconds + " seconds";

            var header
                = "General Information\r\n"
                + "===================\r\n"
                + $" Title:                  {aaxcTagLib.Tag.Title.Replace(" (Unabridged)", "")}\r\n"
                + $" Author:                 {aaxcTagLib.Tag.FirstPerformer ?? "[unknown]"}\r\n"
                + $" Read By:                {aaxcTagLib.GetTag(TagLib.TagTypes.Apple).Narrator??"[unknown]"}\r\n"
                + $" Copyright:              {aaxcTagLib.Tag.Year}\r\n"
                + $" Audiobook Copyright:    {aaxcTagLib.Tag.Year}\r\n";
            if (!string.IsNullOrEmpty(aaxcTagLib.Tag.FirstGenre))
                header += $" Genre:                  {aaxcTagLib.Tag.FirstGenre}\r\n";

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
                + $" Source Sample Rate:     {aaxcTagLib.Properties.AudioSampleRate} Hz\r\n"
                + $" Source Channels:        {aaxcTagLib.Properties.AudioChannels}\r\n"
                + $" Source Bitrate:         {aaxcTagLib.Properties.AudioBitrate} kbits\r\n"
                + "\r\n"
                + " Lossless Encode:        Yes\r\n"
                + " Encoded Codec:          AAC / M4B\r\n"
                + $" Encoded Sample Rate:    {aaxcTagLib.Properties.AudioSampleRate} Hz\r\n"
                + $" Encoded Channels:       {aaxcTagLib.Properties.AudioChannels}\r\n"
                + $" Encoded Bitrate:        {aaxcTagLib.Properties.AudioBitrate} kbits\r\n"
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
