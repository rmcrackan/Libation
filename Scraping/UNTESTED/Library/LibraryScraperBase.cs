using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudibleDotCom;
using Scraping.Selectors;

namespace Scraping.Library
{
    internal abstract class LibraryScraperBase
    {
        private AudiblePageSource source { get; }
        private WebElement docRoot { get; }

        protected LibraryScraperBase(AudiblePageSource pageSource)
        {
            source = pageSource;

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(source.Source);
            docRoot = new WebElement(doc.DocumentNode);
        }
        
        public abstract IEnumerable<LibraryDTO> ScrapeCurrentPage();

        #region // parallel-izing tests: selenium vs html agility pack
        // iterate:          foreach (var r in rows) extractProductItem(r, ruleFamily, returnProductItems)
        // yield:            foreach (var r in driver.FindElements(ruleFamily.RowsLocator).ToList()) yield return extractProductItem(r, ruleFamily, returnProductItems)
        // Parallel_ForEach: Parallel.ForEach(rows, (r) => extractProductItem(r, ruleFamily, returnProductItems))
        // WaitAll:          Task.WaitAll(tasks)
        // AsParallel:       rows.AsParallel().Select(r => extractProductItem(r, ruleFamily, returnProductItems))
        //
        // in milliseconds
        // selenium. [1] slow [2] is/has a bottleneck which resists parallelization
        //   iterate:          394424 - 439711
        //   yield:            387854
        //   Parallel_ForEach: 345149 - 371547
        //   WaitAll:          363970
        //   AsParallel:       369904
        // html agility pack
        //   iterate:           15024 -  19092  55-60% of this time is downloading images
        //   Parallel_ForEach:   4060 -   4271  <<<<<<<<<<<<<<<<<<<<<<<
        //   WaitAll:            3646 -   8702 . mostly ~6-8k
        //   AsParallel:         4318 -   8378
        #endregion

        protected IEnumerable<LibraryDTO> scrapeRows(RuleFamilyLib ruleFamily)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();


            var rows = ruleFamily.GetRows(docRoot).ToList();

            var returnProductItems = new List<LibraryDTO>();

            scrape1row_parallel(rows, ruleFamily, returnProductItems);
            //scrape1row_iterate(rows, ruleFamily, returnProductItems);


            sw.Stop();
            var ms = sw.ElapsedMilliseconds;

            return returnProductItems;
        }

        private IEnumerable<LibraryDTO> scrapeRows_YIELD(WebElement driver, RuleFamilyLib ruleFamily)
        {
            var returnProductItems = new List<LibraryDTO>();

            var rows = driver.FindElements(ruleFamily.RowsLocator).ToList();
//rows = rows.Take(3).ToList(); // TOP3ONLY
            for (var i = 0; i < rows.Count; i++)
            {
                string currentRow = $"{i + 1} of {rows.Count}";
                // break here to see which row we're on
                var r = rows[i];

                yield return extractLibraryDTO(r, ruleFamily, returnProductItems);
            }
        }

        private void scrape1row_iterate(IEnumerable<WebElement> rows, RuleFamilyLib ruleFamily, List<LibraryDTO> returnProductItems)
        {
            foreach (var r in rows)
                extractLibraryDTO(r, ruleFamily, returnProductItems);
        }

        private void scrape1row_parallel(IEnumerable<WebElement> rows, RuleFamilyLib ruleFamily, List<LibraryDTO> returnProductItems)
            => Parallel.ForEach(rows, r => extractLibraryDTO(r, ruleFamily, returnProductItems));

        private object _locker { get; } = new object();
        private LibraryDTO extractLibraryDTO(WebElement row, RuleFamilyLib ruleFamily, List<LibraryDTO> returnProductItems)
        {
            var productItem = new LibraryDTO();
            ruleFamily.Rules.Run(row, productItem);

            // local lock is slightly faster than ConcurrentBag
            // https://stackoverflow.com/questions/2950955/concurrentbagmytype-vs-listmytype/34016915#34016915
            lock (_locker)
                returnProductItems.Add(productItem);

            // having a return object is for testing with yield
            return productItem;
        }
    }
}
