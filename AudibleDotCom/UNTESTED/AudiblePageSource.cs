using FileManager;

namespace AudibleDotCom
{
    public class AudiblePageSource
    {
        public AudiblePageType AudiblePage { get; }
        public string Source { get; }
        public string PageId { get; }

        public AudiblePageSource(AudiblePageType audiblePage, string source, string pageId)
        {
            AudiblePage = audiblePage;
            Source = source;
            PageId = pageId;
        }

        /// <summary>declawed allows local file to safely be reloaded in chrome
        /// NOTE ABOUT DECLAWED FILES
        /// making them safer also breaks functionality
        /// eg: previously hidden parts become visible. this changes how selenium can parse pages.
        ///     hidden elements don't expose .Text property</summary>
        public AudiblePageSource Declawed() => new AudiblePageSource(AudiblePage, FileUtility.Declaw(Source), PageId);

        public string Serialized() => $"<!-- |{AudiblePage.GetAudiblePageRobust().Abbreviation}|{(PageId ?? "").Trim()}| -->\r\n" + Source;

        public static AudiblePageSource Deserialize(string serializedSource)
        {
            var endOfLine1 = serializedSource.IndexOf('\n');

            var parameters = serializedSource
                .Substring(0, endOfLine1)
                .Split('|');
            var abbrev = parameters[1];
            var pageId = parameters[2];

            var source = serializedSource.Substring(endOfLine1 + 1);
            var audiblePage = AudibleDotCom.AudiblePage.FromDisplayName(abbrev).AudiblePageType;

            return new AudiblePageSource(audiblePage, source, pageId);
        }
    }
}
