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

        public static string DefaultMultipartFilename(MultiConvertFileProperties multiConvertFileProperties)
        {
            var template = Path.ChangeExtension(multiConvertFileProperties.OutputFileName, null) + " - <ch# 0> - <title>" + Path.GetExtension(multiConvertFileProperties.OutputFileName);

            var fileNamingTemplate = new FileNamingTemplate(template) { IllegalCharacterReplacements = " " };
            fileNamingTemplate.AddParameterReplacement("ch# 0", FileUtility.GetSequenceFormatted(multiConvertFileProperties.PartsPosition, multiConvertFileProperties.PartsTotal));
            fileNamingTemplate.AddParameterReplacement("title", multiConvertFileProperties.Title ?? "");

            return fileNamingTemplate.GetFilePath();
        }
    }
}
