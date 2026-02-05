using System;

namespace AaxDecrypter
{
    public class MultiConvertFileProperties
    {
        public required string OutputFileName { get; set; }
        public int PartsPosition { get; set; }
        public int PartsTotal { get; set; }
        public string? Title { get; set; }
        public DateTime FileDate { get; } = DateTime.Now;
    }
}
