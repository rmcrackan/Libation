using AAXClean;
using Dinah.Core;

namespace AaxDecrypter
{
    public static class NFO
    {
        public static string CreateContents(string ripper, Mp4File aaxcTagLib, ChapterInfo chapters)
        {
            var _hours = (int)aaxcTagLib.Duration.TotalHours;
            var myDuration
                = (_hours > 0 ? _hours + " hours, " : string.Empty)
                + aaxcTagLib.Duration.Minutes + " minutes, "
                + aaxcTagLib.Duration.Seconds + " seconds";

            var nfoString
                = "General Information\r\n"
                + "======================\r\n"
                + $" Title:                  {aaxcTagLib.AppleTags.TitleSansUnabridged?.UnicodeToAscii() ?? "[unknown]"}\r\n"
                + $" Author:                 {aaxcTagLib.AppleTags.FirstAuthor?.UnicodeToAscii() ?? "[unknown]"}\r\n"
                + $" Read By:                {aaxcTagLib.AppleTags.Narrator?.UnicodeToAscii() ?? "[unknown]"}\r\n"
                + $" Release Date:           {aaxcTagLib.AppleTags.ReleaseDate ?? "[unknown]"}\r\n"
                + $" Book Copyright:         {aaxcTagLib.AppleTags.BookCopyright ?? "[unknown]"}\r\n"
                + $" Recording Copyright:    {aaxcTagLib.AppleTags.RecordingCopyright ?? "[unknown]"}\r\n"
                + $" Genre:                  {aaxcTagLib.AppleTags.Generes ?? "[unknown]"}\r\n"
                + $" Publisher:              {aaxcTagLib.AppleTags.Publisher ?? "[unknown]"}\r\n"
                + $" Duration:               {myDuration}\r\n"
                + $" Chapters:               {chapters.Count}\r\n"
                + "\r\n"
                + "\r\n"
                + "Media Information\r\n"
                + "======================\r\n"
                + " Source Format:          Audible AAXC\r\n"
                + $" Source Sample Rate:     {aaxcTagLib.TimeScale} Hz\r\n"
                + $" Source Channels:        {aaxcTagLib.AudioChannels}\r\n"
                + $" Source Bitrate:         {aaxcTagLib.AverageBitrate} Kbps\r\n"
                + "\r\n"
                + " Lossless Encode:        Yes\r\n"
                + " Encoded Codec:          AAC / M4B\r\n"
                + $" Encoded Sample Rate:    {aaxcTagLib.TimeScale} Hz\r\n"
                + $" Encoded Channels:       {aaxcTagLib.AudioChannels}\r\n"
                + $" Encoded Bitrate:        {aaxcTagLib.AverageBitrate} Kbps\r\n"
                + "\r\n"
                + $" Ripper:                 {ripper}\r\n"
                + "\r\n"
                + "\r\n"
                + "Book Description\r\n"
                + "================\r\n"
                + (!string.IsNullOrWhiteSpace(aaxcTagLib.AppleTags.LongDescription) ? aaxcTagLib.AppleTags.LongDescription.UnicodeToAscii() : aaxcTagLib.AppleTags.Comment?.UnicodeToAscii());

            return nfoString;
        }
    }
}
