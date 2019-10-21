using System;
using System.Collections.Generic;
using System.Linq;
using AudibleDotCom;
using Dinah.Core;
using DTOs;
using Scraping.BookDetail;
using Scraping.Library;

namespace Scraping
{
    public static class AudibleScraper
    {
        public static List<LibraryDTO> ScrapeLibrarySources(params AudiblePageSource[] pageSources)
        {
            if (pageSources == null || !pageSources.Any())
                return new List<LibraryDTO>();

            if (pageSources.Select(ps => ps.AudiblePage).Distinct().Single() != AudiblePageType.Library)
                throw new Exception("only Library items allowed");

            return pageSources.SelectMany(s => scrapeLibraryPageSource(s)).ToList();
        }
        private static List<LibraryDTO> scrapeLibraryPageSource(AudiblePageSource pageSource)
            => new LibraryScraper(pageSource)
                .ScrapeCurrentPage()
                // ScrapeCurrentPage() is long running. do not taunt delayed execution
                .ToList();

        public static BookDetailDTO ScrapeBookDetailsSource(AudiblePageSource pageSource)
        {
            ArgumentValidator.EnsureNotNull(pageSource, nameof(pageSource));

            if (pageSource.AudiblePage != AudiblePageType.ProductDetails)
                throw new Exception("only Product Details items allowed");

            try
            {
                return new BookDetailScraper(pageSource).ScrapePage();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
