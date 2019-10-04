using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AudibleDotCom;
using Dinah.Core;
using FileManager;
using Scraping.Selectors;

namespace Scraping.Library
{
    internal class LibraryScraper : LibraryScraperBase
    {
        // row_xpath_OLD = "//tr[td[@class='adbl-lib-multipart-toggle']]"
        static RuleFamilyLib ruleFamily { get; } = new RuleFamilyLib
        {
            // find all rows. ignores multi-part rows. ie: gets the first part only when multi-part
            RowsLocator = By.XPath("//*[starts-with(@id,'adbl-library-content-row-')]"),
            Rules = getRuleSet()
        };
        static string debug_lastGoodTitleOuterHtml;
        protected static RuleSetLib getRuleSet()
            => new RuleSetLib
            {
                // product id
                (row, productItem) => productItem.ProductId = row.FindElements(By.XPath(".//input[@name='asin']")).First().Value.Trim(),
                // title: 1st td, 1st ul, 1st li, within h2
                (row, productItem) => // (row, productItem) => productItem.Title = row.FindElement(By.XPath("(./td[1]//ul)[1]/li[1]/h2")).Text.Trim(),
                {
                    var debug_attemptTitleScriptOuterHtml = row.Node.OuterHtml;
                    try
                    {
                        productItem.Title = row.FindElement(By.XPath("(./td[1]//ul)[1]/li[1]/h2")).Text.Trim();

                        debug_lastGoodTitleOuterHtml = debug_attemptTitleScriptOuterHtml;
                    }
                    catch
                    {
                        var badTitle = debug_attemptTitleScriptOuterHtml;

                        throw;
                    }
                },
                // is episodes. these will not have a book download link or personal library ratings
                (row, productItem) => productItem.IsEpisodes = row.FindElements(By.XPath(".//a[starts-with(@href, '/a/library/subscription?')]")).Any(),
                // get picture id, download images
                (row, productItem) => {
                    productItem.PictureId = row
                        .FindElements(By.ClassName("bc-image-inset-border"))
						.First()
                        .GetAttribute("src")
                        .Split('/').Last()
                        .Split('.').First();
                    PictureStorage.DownloadImages(productItem.PictureId);
                },
                // all text links
                new LocatedRuleSetLib(By.XPath(".//a[not(img)]")) {
                    (link, ProductItem) => {
                        var href = link.GetAttribute("href");

                        if (href == null)
                            return;

                        // authors
                        var authorName = link.Text.Trim();
                        string authorId;

                        // with no id. DO NOT REPLACE THIS STEP. needed for valid early exit in 'else'
                        if (href.Contains("/search?searchAuthor="))
                            authorId = null;
                        // with id
                        else if (href.Contains("/author/"))
                            authorId = href
                                .Split('?')[0]
                                .Split('/').Last();
                        else // not an author
                            return;

                        ProductItem.Authors.Add((authorName, authorId));
                    },
                },
                // series. id only; not name
                new LocatedRuleSetLib(By.XPath(".//a[text()='View Series']/@href")) {
                    (link, productItem) => productItem.Series[link.GetAttribute("href").Replace("series?asin=", "")] = null
                },
                // pdf download link
                new LocatedRuleSetLib(By.ClassName("adbl-lib-action-pdf")) {
                    (link, productItem) => productItem.SupplementUrls.Add(link.GetAttribute("href"))
                },
                bookDownloadLink,
                // date added to library
                (row, productItem) => {
                    var dateAdded = row
                        .FindElements(By.ClassName("bc-text"))
                        .Select(l => l.Text.Trim())
                        .Where(str => Regex.IsMatch(str, @"^\d\d-\d\d-\d\d$"))
                        .Select(dateText => DateTime.ParseExact(dateText, "MM-dd-yy", System.Globalization.CultureInfo.InvariantCulture))
                        .ToList();
                    if (dateAdded.Any())
                        productItem.DateAdded = dateAdded.First();
                },
                // my library ratings
                (row, productItem) => {
                    if (productItem.IsEpisodes)
                        return;
                    productItem.MyUserRating_Overall = int.Parse(row
                        .FindElement(By.ClassName("adbl-prod-rate-review-bar-overall")).GetAttribute("data-star-count"));
                    productItem.MyUserRating_Performance = int.Parse(row
                        .FindElement(By.ClassName("adbl-prod-rate-review-bar-performance")).GetAttribute("data-star-count"));
                    productItem.MyUserRating_Story = int.Parse(row
                        .FindElement(By.ClassName("adbl-prod-rate-review-bar-story")).GetAttribute("data-star-count"));
                },
                // 1st td (summary panel) (xpath uses 1-based indexes), top bullets
                // to get the first, use parentheses. it will parse w/o parans but will fall through to the 2nd unwanted ul
                new LocatedRuleSetLib(By.XPath("(./td[1]//ul)[1]/li")) {
                    (li, productItem) => {
                        var text = li.Text.Trim();

                        // narrators
                        if (!text.StartsWith("Narrated by:"))
                            return;

                        var narratorNames = text.Replace("Narrated by:", "").Trim();
                        productItem.Narrators = sanitizeContributorNames(narratorNames.Split(',')).ToArray();
                    },
                    (li, productItem) => {
                        var text = li.Text.Trim();

                        // parse time
                        if (!text.StartsWith("Length:"))
                            return;
                        if (!text.Contains(" hr") && !text.Contains(" min"))
                            return;

                        var timeSplit = text
                            .Replace("Length:", "")
                            .Trim()
                            .Split(new string[] { " and " }, StringSplitOptions.RemoveEmptyEntries);
                        // do the math for 1 item then add to productItem.
                        // If we += directly to the productItem inside foreach(), then time will be doubled if this runs twice
                        var tempLengthInMinutes = 0;
                        foreach (var part in timeSplit)
                        {
                            if (part.Contains("sec"))
                                continue;
                            var intPart = int.Parse(part.Replace("hr", "").Replace("min", "").Replace("s", "").Trim());
                            if (part.Contains("hr"))
                                intPart *= 60;
                            tempLengthInMinutes += intPart;
                        }
                        productItem.LengthInMinutes = tempLengthInMinutes;
                    },
                },
                // 1st td (summary panel)
                // description
                new LocatedRuleSetLib(By.XPath("./td[1]//p")) {
                    (p, productItem) => {
                        var text = p.Text.Trim();
                        if (!string.IsNullOrWhiteSpace(text))
                            productItem.Description = sanitizeDescription(text);
                    },
                },
                // 1st td (summary panel)
                // 2nd set of bullets has product ratings
                new LocatedRuleSetLib(By.XPath("(./td[1]//ul)[2]/li")) {
                    (li, productItem) => {
                        // splitting on null assumes white space: https://docs.microsoft.com/en-us/dotnet/api/system.string.split
                        var text = li.Text.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);
                        if (text.Length < 2)
                            return;

                        var rating = float.Parse(text[1]);
                        var totalVotes = int.Parse(text[6].Replace(",", ""));
                        switch (text[0])
                        {
                            case "Overall": productItem.Product_OverallStars = rating; return;
                            case "Performance": productItem.Product_PerformanceStars = rating; return;
                            case "Story": productItem.Product_StoryStars = rating; return;
                        }
                    },
                },
            };

        public LibraryScraper(AudiblePageSource pageSource) : base(pageSource) { }

        public override IEnumerable<LibraryDTO> ScrapeCurrentPage()
        {
            #region // example for once per page rules
            //var onePerPageProductItemValues = scrapeRows(onePerPageRules).Single();
            //foreach (var productItem in scrapeRows(old_family))
            //{
            //    productItem.CustomerId = onePerPageProductItemValues.CustomerId;
            //    productItem.UserName = onePerPageProductItemValues.UserName;
            //    productItem.Last_DownloadCustId = onePerPageProductItemValues.Last_DownloadCustId;
            //    yield return productItem;
            //}
            #endregion

            #region // example for mutiple rule sets
            // var i = 0;
            // foreach (var oldResult in scrapeRows(oldRuleFamily))
            // {
            //     i++;
            //     yield return oldResult;
            // }
            // if (i > 0)
            //     yield break;
            // foreach (var newResult in scrapeRows(newRuleFamily))
            //     yield return newResult;
            #endregion

            return scrapeRows(ruleFamily);
        }

        // this is broken out into its own method as a proof of concept. it may also help with debugging
        static void bookDownloadLink(WebElement row, LibraryDTO productItem)
        {
            if (productItem.IsEpisodes)
                return;

            var downloadLink = row.FindElements(By.ClassName("adbl-lib-action-download")).FirstOrDefault();
            // ToNode switches to HtmlAgilityPack style. could also have used xpath .//a (or ./a since it happens to be the immediate descendant)
            productItem.DownloadBookLink = downloadLink.Node
                .Descendants("a").Single()
                .Attributes["href"].Value;

            // check for
            // href="/howtolisten"
            if (productItem.DownloadBookLink.ContainsInsensitive("howtolisten"))
                throw new Exception("BAD DOWNLOAD LINKS"
                    + "\r\n" + "PROBLEM:  Library download button is a link to the 'howtolisten' page"
                    + "\r\n" + "SOLUTION: Toggle this checkbox: Accounts Details > Update Settings > Software Verification > Check for Audible Download Manager");
        }

        #region static scrape page helpers
        private static Regex removeIntroductionsRegex = new Regex(
            #region regex: remove "introduction" variants
            @"
# keep this. non-greedy
(.*?)

# non-capture. this is what to throw away
(?:
    # this will capture
    # (introduction)
    # (introductions)
    # - introduction
    # - introductions
    \s*\-?\s*\(?introductions?\)?
)?
            ", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled
            #endregion
        );
        // input: comma delimited list of names. possibly with " - introductions", "(introduction)", etc
        // output: clean up string[]
        private static IEnumerable<string> sanitizeContributorNames(IEnumerable<string> names)
            => names.Select(n => removeIntroductionsRegex.Replace(n.Replace(",", "").Trim(), ""));
        // room for improvement. all kinds of other things are tagged onto names with hyphens and parans. eg: "(cover illustration)", " - essay"

        private static string sanitizeDescription(string desc) => desc
            .Replace("â€™", "'") //            '
            .Replace("’", "'") //              '
            .Replace("â€¦", "...") //          …
            .Replace("â€œ", "\"") //           "
            .Replace("â€" + '\u009d', "\"") // "
            .Replace("“", "\"") //             "
            .Replace("”", "\"") //             "
            .Trim();
        #endregion
    }
}
