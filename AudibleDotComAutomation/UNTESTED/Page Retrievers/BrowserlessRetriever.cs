using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AudibleDotCom;
using CookieMonster;
using Dinah.Core;
using Dinah.Core.Humanizer;

namespace AudibleDotComAutomation
{
    public class BrowserlessRetriever : IPageRetriever
    {
        Humanizer humanizer { get; } = new Humanizer();

        public async Task<IEnumerable<AudiblePageSource>> GetPageSourcesAsync(AudiblePageType audiblePage, string pageId = null)
        {
            switch (audiblePage)
            {
case AudiblePageType.Library: return await getLibraryPageSourcesAsync();
                default: throw new NotImplementedException();
            }
        }

        private async Task<IEnumerable<AudiblePageSource>> getLibraryPageSourcesAsync()
        {
            var collection = new List<AudiblePageSource>();

            var cookies = await getAudibleCookiesAsync();

            var currPageNum = 1;
            bool hasMorePages;
            do
            {
                // EACH CALL to audible should have a small random wait to reduce chances of scrape detection
                await humanizer.Wait();

var html = await getLibraryPageAsync(cookies, currPageNum);
var pageSource = new AudiblePageSource(AudiblePageType.Library, html, null);
                collection.Add(pageSource);

                hasMorePages = getHasMorePages(pageSource.Source);

                currPageNum++;
            } while (hasMorePages);

            return collection;
        }

        private static async Task<CookieContainer> getAudibleCookiesAsync()
        {
            var liveCookies = await CookiesHelper.GetLiveCookieValuesAsync();

            var audibleCookies = liveCookies.Where(c
                => c.Domain.ContainsInsensitive("audible.com")
                || c.Domain.ContainsInsensitive("adbl")
                || c.Domain.ContainsInsensitive("amazon.com"))
            .ToList();

            var cookies = new CookieContainer();
            foreach (var c in audibleCookies)
                cookies.Add(new Cookie(c.Name, c.Value, "/", c.Domain));

            return cookies;
        }

        private static bool getHasMorePages(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            // final page, invalid page:
            //   <span class="bc-button
            //     bc-button-secondary
            //     nextButton
            //     bc-button-disabled">
            // only page: ???
            // has more pages:
            //   <span class="bc-button
            //     bc-button-secondary
            //     refinementFormButton
            //     nextButton">
            var next_active_link = doc
                .DocumentNode
                .Descendants()
                .FirstOrDefault(n =>
                    n.HasClass("nextButton") &&
                    !n.HasClass("bc-button-disabled"));

            return next_active_link != null;
        }

        private static async Task<string> getLibraryPageAsync(CookieContainer cookies, int pageNum)
        {
			#region // POST example (from 2017 ajax)
			// var destination = "https://www.audible.com/lib-ajax";
			// var webRequest = (HttpWebRequest)WebRequest.Create(destination);
			// webRequest.Method = "POST";
			// webRequest.Accept = "*/*";
			// webRequest.AllowAutoRedirect = false;
			// webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.0.3705)";
			// webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
			// webRequest.Credentials = null;
			//
			// webRequest.CookieContainer = new CookieContainer();
			// webRequest.CookieContainer.Add(cookies.GetCookies(new Uri(destination)));
			//
			// var postData = $"progType=all&timeFilter=all&itemsPerPage={itemsPerPage}&searchTerm=&searchType=&sortColumn=&sortType=down&page={pageNum}&mode=normal&subId=&subTitle=";
			// var data = Encoding.UTF8.GetBytes(postData);
			// webRequest.ContentLength = data.Length;
			// using var dataStream = webRequest.GetRequestStream();
			// dataStream.Write(data, 0, data.Length);
			#endregion

            var destination = "https://" + $"www.audible.com/lib?purchaseDateFilter=all&programFilter=all&sortBy=PURCHASE_DATE.dsc&page={pageNum}";
            var webRequest = (HttpWebRequest)WebRequest.Create(destination);
            webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.0.3705)";

            webRequest.CookieContainer = new CookieContainer();
            webRequest.CookieContainer.Add(cookies.GetCookies(new Uri(destination)));

            var webResponse = await webRequest.GetResponseAsync();
            return new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
        }

        public void Dispose() { }
    }
}
