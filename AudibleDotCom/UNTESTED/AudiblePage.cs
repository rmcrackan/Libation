using System;
using System.Linq;
using Dinah.Core;

namespace AudibleDotCom
{
    public enum AudiblePageType
    {
        ProductDetails = 1,

        Library = 2
    }
    public static class AudiblePageExt
    {
        public static AudiblePage GetAudiblePageRobust(this AudiblePageType audiblePage) => AudiblePage.FromPageType(audiblePage);
    }

    public abstract partial class AudiblePage : Enumeration<AudiblePage>
    {
        // useful for generic classes:
        //     public abstract class PageScraper<T> where T : AudiblePageRobust {
        //         public AudiblePage AudiblePage => AudiblePageRobust.GetAudiblePageFromType(typeof(T));
        public static AudiblePageType GetAudiblePageFromType(Type audiblePageRobustType)
            => (AudiblePageType)GetAll().Single(t => t.GetType() == audiblePageRobustType).Id;

        public AudiblePageType AudiblePageType { get; }

        protected AudiblePage(AudiblePageType audiblePage, string abbreviation) : base((int)audiblePage, abbreviation) => AudiblePageType = audiblePage;

        public static AudiblePage FromPageType(AudiblePageType audiblePage) => FromValue((int)audiblePage);

        /// <summary>For pages which need a param, the param is marked with {0}</summary>
        protected abstract string Url { get; }
        public string GetUrl(string id) => string.Format(Url, id);

        public string Abbreviation => DisplayName;
    }
    public abstract partial class AudiblePage : Enumeration<AudiblePage>
    {
        public static AudiblePage Library { get; } = LibraryPage.Instance;
        public class LibraryPage : AudiblePage
        {
            #region singleton stuff
            public static LibraryPage Instance { get; } = new LibraryPage();
            static LibraryPage() { }
            private LibraryPage() : base(AudiblePageType.Library, "LIB") { }
            #endregion

            protected override string Url => "http://www.audible.com/lib";
        }
    }
    public abstract partial class AudiblePage : Enumeration<AudiblePage>
    {
        public static AudiblePage Product { get; } = ProductDetailPage.Instance;
        public class ProductDetailPage : AudiblePage
        {
            #region singleton stuff
            public static ProductDetailPage Instance { get; } = new ProductDetailPage();
            static ProductDetailPage() { }
            private ProductDetailPage() : base(AudiblePageType.ProductDetails, "PD") { }
            #endregion

            protected override string Url => "http://www.audible.com/pd/{0}";
        }
    }
}
