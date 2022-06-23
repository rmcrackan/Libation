using System;
using System.IO;
using FileManager;

namespace AaxDecrypter
{
    public class MultiConvertFileProperties
    {
        public string OutputFileName { get; set; }
        public int PartsPosition { get; set; }
        public int PartsTotal { get; set; }
        public string Title { get; set; }

    }
}
