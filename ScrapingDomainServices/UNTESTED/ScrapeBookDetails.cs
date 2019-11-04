using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AudibleDotCom;
using DataLayer;
using Dinah.Core.ErrorHandling;
using DTOs;
using InternalUtilities;
using Scraping;

namespace ScrapingDomainServices
{
    /// <summary>
    /// book detail page:
    /// - audible webpage => AudiblePageSource
    /// - AudiblePageSource => declaw => htm file
    /// - AudiblePageSource => scrape => DTO
    /// - DTO => json file
    /// - DTO => db
    /// - update lucene
    /// </summary>
    public class ScrapeBookDetails : DownloadableBase
    {
        public enum NoLongerAvailableEnum { None, Abort, MarkAsMissing }

        /// <summary>Returns product id of book which was successfully imported and re-indexed</summary>
        public event EventHandler<string> BookSuccessfullyImported;

        /// <summary>Hook for handling book no-longer-available. String 1: book title. String 2: book url</summary>
        public Func<string, string, NoLongerAvailableEnum> NoLongerAvailableAction { get; set; }

        public override Task<bool> ValidateAsync(LibraryBook libraryBook)
            => Task.FromResult(!libraryBook.Book.HasBookDetails);

        public override async Task<StatusHandler> ProcessItemAsync(LibraryBook libraryBook)
        {
            var productId = libraryBook.Book.AudibleProductId;

            #region // TEST CODE
            //productId = "B0787DGS2T"; // book with only 1 category, no sub category
            //productId = "B002V1OF70"; // mult series, more narrators here than in library
            //productId = "B0032N8Q58"; // abridged
            //productId = "B07GXW7KHG"; // categories in product details block. no narrators
            //productId = "B002ZEEDAW"; // categores above image
            //productId = "B075Y4SWJ8"; // lots of narrators, no 'abridged'
            #endregion

            BookDetailDTO bookDetailDTO;

            // if json file exists, then htm is irrelevant. important b/c in cases of no-longer-available items, json is generated but no htm
            var jsonFileInfo = FileManager.WebpageStorage.GetBookDetailJsonFileInfo(productId);
            if (jsonFileInfo.Exists)
            {
                var serialized = File.ReadAllText(jsonFileInfo.FullName);
                bookDetailDTO = Newtonsoft.Json.JsonConvert.DeserializeObject<BookDetailDTO>(serialized);
            }
            // no json. download htm
            else
            {
                var htmFile = FileManager.WebpageStorage.GetBookDetailHtmFileInfo(productId);
                // htm exists, json doesn't. load existing htm
                if (htmFile.Exists)
                {
                    var detailsAudiblePageSource = DataConverter.HtmFile_2_AudiblePageSource(htmFile.FullName);
                    bookDetailDTO = AudibleScraper.ScrapeBookDetailsSource(detailsAudiblePageSource);
                }
                // no htm. download and parse
                else
                {
                    // download htm
                    string source;
                    var url = AudiblePage.Product.GetUrl(productId);
					using var webClient = await GetWebClientAsync($"Getting Book Details for {libraryBook.Book.Title}");
					try
					{
						source = await webClient.DownloadStringTaskAsync(url);
						var detailsAudiblePageSource = new AudiblePageSource(AudiblePageType.ProductDetails, source, productId);

						// good habit to persist htm before attempting to parse it. this way, if there's a parse error, we can test errors on a local copy
						DataConverter.AudiblePageSource_2_HtmFile_Product(detailsAudiblePageSource);

						bookDetailDTO = AudibleScraper.ScrapeBookDetailsSource(detailsAudiblePageSource);
					}
					catch (System.Net.WebException webEx)
					{
						// cannot continue if NoLongerAvailableAction is null,
						// else we'll be right back here next loop (and infinitely) with no failure condition
						if (webEx.Status != System.Net.WebExceptionStatus.ConnectionClosed || NoLongerAvailableAction == null)
							throw;

						var nlaEnum = NoLongerAvailableAction.Invoke(
							libraryBook.Book.Title,
							AudiblePage.Product.GetUrl(libraryBook.Book.AudibleProductId));
						if (nlaEnum == NoLongerAvailableEnum.Abort)
							return new StatusHandler { "Cannot scrape book details. Aborting." };
						else if (nlaEnum == NoLongerAvailableEnum.MarkAsMissing)
							bookDetailDTO = new BookDetailDTO { ProductId = productId };
						else
							throw;
					}
				}

                DataConverter.Value_2_JsonFile(bookDetailDTO, jsonFileInfo.FullName);
            }

            await Indexer.IndexBookDetailsAsync(bookDetailDTO);

            BookSuccessfullyImported?.Invoke(this, productId);

            return new StatusHandler();
        }
    }
}
