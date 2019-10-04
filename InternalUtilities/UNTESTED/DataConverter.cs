using System.IO;
using AudibleDotCom;
using FileManager;
using Newtonsoft.Json;

namespace InternalUtilities
{
    public static partial class DataConverter
    {
        // also need: htm file => PageSource

        public static AudiblePageSource HtmFile_2_AudiblePageSource(string htmFilepath)
        {
            var htmContentsDeclawed = File.ReadAllText(htmFilepath);
            var htmContents = FileUtility.RestoreDeclawed(htmContentsDeclawed);
            return AudiblePageSource.Deserialize(htmContents);
        }

        public static FileInfo Value_2_JsonFile(object value, string jsonFilepath)
        {
            var json = JsonConvert.SerializeObject(value, Formatting.Indented);

            File.WriteAllText(jsonFilepath, json);

            return new FileInfo(jsonFilepath);
        }

        /// <summary>AudiblePageSource => declawed htm file</summary>
        /// <returns>path of htm file</returns>
        public static FileInfo AudiblePageSource_2_HtmFile_Batch(AudiblePageSource audiblePageSource, string batchName)
        {
            var source = audiblePageSource.Declawed().Serialized();
            var htmFile = WebpageStorage.SavePageToBatch(source, batchName, "htm");
            return new FileInfo(htmFile);
        }

        /// <summary>AudiblePageSource => declawed htm file</summary>
        /// <returns>path of htm file</returns>
        public static FileInfo AudiblePageSource_2_HtmFile_Product(AudiblePageSource audiblePageSource)
        {
            if (audiblePageSource.AudiblePage == AudiblePageType.ProductDetails)
            {
                var source = audiblePageSource.Declawed().Serialized();
                var htmFile = WebpageStorage.SaveBookDetailsToHtm(audiblePageSource.PageId, source);
                return htmFile;
            }

            throw new System.NotImplementedException();
        }
    }
}
