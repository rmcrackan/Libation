using System;
using System.Collections.Generic;
using System.Linq;
using AudibleDotCom;
using Dinah.Core;
using Newtonsoft.Json.Linq;
using Scraping.Selectors;

namespace Scraping.BookDetail
{
    static class NewtonsoftExt
    {
        public static string GetDecodedTokenString(this JToken jToken) => System.Net.WebUtility.HtmlDecode(((string)jToken).Trim());
    }
    internal class BookDetailScraper
    {
        private AudiblePageSource source { get; }
        private WebElement docRoot { get; }

        public BookDetailScraper(AudiblePageSource pageSource)
        {
            source = pageSource;

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(source.Source);
            docRoot = new WebElement(doc.DocumentNode);
        }

        static RuleFamilyBD ruleFamily { get; } = new RuleFamilyBD
        {
            RowsLocator = By.XPath("/*"),
            Rules = new RuleSetBD
            {
                parseJson,
                parseSeries
            }
        };

        public BookDetailDTO ScrapePage()
        {
            //debug//var sw = System.Diagnostics.Stopwatch.StartNew();

            var returnBookDetailDto = new BookDetailDTO { ProductId = source.PageId };

            var wholePage = ruleFamily.GetRows(docRoot).Single();
            ruleFamily.Rules.Run(wholePage, returnBookDetailDto);

            //debug//sw.Stop(); var ms = sw.ElapsedMilliseconds;

            return returnBookDetailDto;
        }

        static void parseJson(WebElement row, BookDetailDTO productItem)
        {
            // structured data is in the 2nd of the 3 json embedded sections <script type="application/ld+json">
            var ldJson = row
                .FindElements(By.XPath("//script[@type='application/ld+json']"))
                [1]
                // use InnerText NOT webElement.Text
                // .Text decodes which will break json if it contains &quot;
                // eg: "foo &quot; bar" => "foo " bar"
                .Node.InnerText;
            var jsonArray = JArray.Parse(ldJson);

            var json0 = jsonArray[0] as JObject;

            //// ways to enumerate properties
            //foreach (var kvp in json0) Console.WriteLine(kvp.Key);
            //foreach (var prop in json0.Properties()) Console.WriteLine(prop.Name);
            var properties = json0.Properties().Select(p => p.Name).ToList();

            // mandatory
            productItem.Title = json0["name"].GetDecodedTokenString();
            productItem.Description = json0["description"].GetDecodedTokenString();
            productItem.Publisher = json0["publisher"].GetDecodedTokenString();
            productItem.DatePublished = DateTime.Parse(json0["datePublished"].GetDecodedTokenString());

            // optional
            if (properties.Contains("abridged"))
                productItem.IsAbridged = Convert.ToBoolean(json0["abridged"].GetDecodedTokenString());
            // not all books have narrators
            if (properties.Contains("readBy"))
                foreach (var narrator in json0["readBy"])
                    productItem.Narrators.Add(narrator["name"].GetDecodedTokenString());

            var json1 = jsonArray[1]["itemListElement"];
            foreach (var element in json1)
            {
                var item = element["item"];
                var id = item["@id"].GetDecodedTokenString();

                if (!id.ContainsInsensitive("/cat/"))
                    continue;

                var categoryId = id.Split('?')[0].Split('/').Last();
                var categoryName = item["name"].GetDecodedTokenString();

                productItem.Categories.Add((categoryId, categoryName));
            }
        }

        static void parseSeries(WebElement row, BookDetailDTO productItem)
        {
            var element = row.FindElements(By.ClassName("seriesLabel")).SingleOrDefault();
            if (element == null)
                return;

            var currEntry = new SeriesEntry();

            var children = element.Node.ChildNodes;
            // skip 0. It's just the label "Series:"
            for (var i = 1; i < children.Count; i++)
            {
                var c = children[i];

                // if contains html: // series name and id. begin new entry
                // new book entry
                if (c.HasChildNodes)
                {
                    string seriesId = null;

                    var href = c.Attributes["href"].Value;
                    var h2 = href.Split('?')[1];
                    var h3 = h2.Split('&');
                    foreach (var h in h3)
                    {
                        var h4 = h.Split('=');
                        if (h4[0].EqualsInsensitive("asin"))
                        {
                            seriesId = h4[1];
                            break;
                        }
                    }

					if (seriesId == null)
					{
						// try this format instead
						if (href.StartsWithInsensitive("/series/"))
						{
							// href
							// /series/The-Interdependency-Audiobooks/B06XKNK664?pf_rd_p=52918805-f7fc-40f4-a76b-cf1c79f7d10a&pf_rd_r=GV7000W2BM97V9Z35ZQD&ref=a_pd_The-Co_c1_series_1
							var mainUrl = href.Split('?')[0];

							// mainUrl
							// /series/The-Interdependency-Audiobooks/B06XKNK664
							var urlAsin = mainUrl.Split('/').Last();

							// sanity check
							if (urlAsin.StartsWithInsensitive("B") && urlAsin.Length == "B07CM5ZDJL".Length)
								seriesId = urlAsin;
						}
					}

					if (seriesId == null)
						throw new Exception("series id not found");

					currEntry = new SeriesEntry { SeriesId = seriesId, SeriesName = c.FirstChild.InnerText };
                    productItem.Series.Add(currEntry);
                }
                // else: is the index in prev series. not all books have an index
                else
                {
                    var indexString = c.InnerText
                        .ToLower()
                        .Replace("book", "")
                        .Replace(",", "")
                        .Trim();
                    if (float.TryParse(indexString, out float index))
                        currEntry.Index = index;
                }
            }
        }
    }
}
