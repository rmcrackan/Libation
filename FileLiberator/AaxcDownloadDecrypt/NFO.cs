using AudibleApiDTOs;

namespace FileLiberator.AaxcDownloadDecrypt
{
    public static class NFO
    {
        public static string CreateContents(string ripper, Tags tags, ChapterInfo chapters)
        {
            var _hours = (int)tags.duration.TotalHours;
            var myDuration
                = (_hours > 0 ? _hours + " hours, " : "")
                + tags.duration.Minutes + " minutes, "
                + tags.duration.Seconds + " seconds";

            var header
                = "General Information\r\n"
                + "===================\r\n"
                + $" Title:                  {tags.title}\r\n"
                + $" Author:                 {tags.author}\r\n"
                + $" Read By:                {tags.narrator}\r\n"
                + $" Copyright:              {tags.year}\r\n"
                + $" Audiobook Copyright:    {tags.year}\r\n";
            if (tags.genre != "")
                header += $" Genre:                  {tags.genre}\r\n";

            var s
                = header
                + $" Publisher:              {tags.publisher}\r\n"
                + $" Duration:               {myDuration}\r\n"
                + $" Chapters:               {chapters.Chapters.Length}\r\n"
                + "\r\n"
                + "\r\n"
                + "Media Information\r\n"
                + "=================\r\n"
                + " Source Format:          Audible AAX\r\n"
                + $" Source Sample Rate:     {tags.sampleRate} Hz\r\n"
                + $" Source Channels:        {tags.channels}\r\n"
                + $" Source Bitrate:         {tags.bitrate} kbits\r\n"
                + "\r\n"
                + " Lossless Encode:        Yes\r\n"
                + " Encoded Codec:          AAC / M4B\r\n"
                + $" Encoded Sample Rate:    {tags.sampleRate} Hz\r\n"
                + $" Encoded Channels:       {tags.channels}\r\n"
                + $" Encoded Bitrate:        {tags.bitrate} kbits\r\n"
                + "\r\n"
                + $" Ripper:                 {ripper}\r\n"
                + "\r\n"
                + "\r\n"
                + "Book Description\r\n"
                + "================\r\n"
                + tags.comments;

            return s;
        }
    }
}
