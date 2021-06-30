
namespace AaxDecrypter
{
    public static class NFO
    {
        public static string CreateContents(string ripper, AaxcTagLibFile aaxcTagLib, ChapterInfo chapters)
        {
            var _hours = (int)aaxcTagLib.Properties.Duration.TotalHours;
            var myDuration
                = (_hours > 0 ? _hours + " hours, " : string.Empty)
                + aaxcTagLib.Properties.Duration.Minutes + " minutes, "
                + aaxcTagLib.Properties.Duration.Seconds + " seconds";

            var header
                = "General Information\r\n"
                + "===================\r\n"
                + $" Title:                  {aaxcTagLib.TitleSansUnabridged ?? "[unknown]"}\r\n"
                + $" Author:                 {aaxcTagLib.FirstAuthor ?? "[unknown]"}\r\n"
                + $" Read By:                {aaxcTagLib.Narrator ?? "[unknown]"}\r\n"
                + $" Release Date:           {aaxcTagLib.ReleaseDate ?? "[unknown]"}\r\n"
                + $" Book Copyright:         {aaxcTagLib.BookCopyright ?? "[unknown]"}\r\n"
                + $" Recording Copyright:    {aaxcTagLib.RecordingCopyright ?? "[unknown]"}\r\n"
                + $" Genre:                  {aaxcTagLib.AppleTags.FirstGenre ?? "[unknown]"}\r\n";

            var s
                = header
                + $" Publisher:              {aaxcTagLib.Publisher ?? "[unknown]"}\r\n"
                + $" Duration:               {myDuration}\r\n"
                + $" Chapters:               {chapters.Count}\r\n"
                + "\r\n"
                + "\r\n"
                + "Media Information\r\n"
                + "=================\r\n"
                + " Source Format:          Audible AAX\r\n"
                + $" Source Sample Rate:     {aaxcTagLib.Properties.AudioSampleRate} Hz\r\n"
                + $" Source Channels:        {aaxcTagLib.Properties.AudioChannels}\r\n"
                + $" Source Bitrate:         {aaxcTagLib.Properties.AudioBitrate} Kbps\r\n"
                + "\r\n"
                + " Lossless Encode:        Yes\r\n"
                + " Encoded Codec:          AAC / M4B\r\n"
                + $" Encoded Sample Rate:    {aaxcTagLib.Properties.AudioSampleRate} Hz\r\n"
                + $" Encoded Channels:       {aaxcTagLib.Properties.AudioChannels}\r\n"
                + $" Encoded Bitrate:        {aaxcTagLib.Properties.AudioBitrate} Kbps\r\n"
                + "\r\n"
                + $" Ripper:                 {ripper}\r\n"
                + "\r\n"
                + "\r\n"
                + "Book Description\r\n"
                + "================\r\n"
                + (!string.IsNullOrWhiteSpace(aaxcTagLib.LongDescription) ? aaxcTagLib.LongDescription : aaxcTagLib.Comment);

            return s;
        }
    }
}
