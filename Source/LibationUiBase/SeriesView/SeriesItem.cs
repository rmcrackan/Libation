using ApplicationServices;
using AudibleApi;
using AudibleApi.Common;
using AudibleUtilities;
using DataLayer;
using Dinah.Core;
using FileLiberator;
using LibationFileManager;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibationUiBase.SeriesView
{
	public class SeriesItem : ReactiveObject
	{
		public object? Cover { get; private set; }
		public SeriesOrder Order { get; }
		public string Title => Item.TitleWithSubtitle;
		public SeriesButton Button { get; }
		public Item Item { get; }

		private SeriesItem(Item item, string order, bool inLibrary, bool inWishList)
		{
			Item = item;
			Order = new SeriesOrder(order);
			Button = Item.Plans.Any(p => p.IsAyce) ? new AyceButton(item, inLibrary) : new WishlistButton(item, inLibrary, inWishList);
			LoadCover(item.PictureId);
			Button.PropertyChanged += DownloadButton_PropertyChanged;
		}

		public void ViewOnAudible(string localeString)
		{
			var locale = Localization.Get(localeString);
			var link = $"https://www.audible.{locale.TopDomain}/pd/{Item.ProductId}";
			Go.To.Url(link);
		}

		private void DownloadButton_PropertyChanged(object? sender, PropertyChangedEventArgs e)
			=> RaisePropertyChanged(nameof(Button));

		private void LoadCover(string pictureId)
		{
			var (isDefault, picture) = PictureStorage.GetPicture(new PictureDefinition(pictureId, PictureSize._80x80));
			if (isDefault)
			{
				PictureStorage.PictureCached += PictureStorage_PictureCached;
			}
			Cover = BaseUtil.LoadImage(picture, PictureSize._80x80);
		}

		private void PictureStorage_PictureCached(object? sender, PictureCachedEventArgs e)
		{
			if (e?.Definition.PictureId != null && Item?.PictureId != null)
			{
				byte[] picture = e.Picture;
				if ((picture == null || picture.Length != 0) && e.Definition.PictureId == Item.PictureId)
				{
					Cover = BaseUtil.LoadImage(e.Picture, PictureSize._80x80);
					PictureStorage.PictureCached -= PictureStorage_PictureCached;
					RaisePropertyChanged(nameof(Cover));
				}
			}
		}

		public static async Task<Dictionary<Item, List<SeriesItem>>> GetAllSeriesItemsAsync(LibraryBook libraryBook)
		{
			var api = await libraryBook.GetApiAsync();

			//Get Item for each series that this book belong to
			var seriesItemsTask = api.GetCatalogProductsAsync(libraryBook.Book.SeriesLink.Select(s => s.Series.AudibleSeriesId), CatalogOptions.ResponseGroupOptions.Media | CatalogOptions.ResponseGroupOptions.Relationships);

			using var semaphore = new SemaphoreSlim(10);

			//Start getting the wishlist in the background
			var wishlistTask = api.GetWishListProductsAsync(
				new WishListOptions
				{
					PageNumber = 0,
					NumberOfResultPerPage = 50,
					ResponseGroups = WishListOptions.ResponseGroupOptions.None
				},
				numItemsPerRequest: 50,
				semaphore);

			var items = new Dictionary<Item, List<Item>>();

			//Get all children of all series
			foreach (var series in await seriesItemsTask)
			{
				//Books that are part of series have RelationshipType.Series
				//Podcast episodes have RelationshipType.Episode
				var childrenAsins = series.Relationships
					.Where(r => r.RelationshipType is RelationshipType.Series or RelationshipType.Episode && r.RelationshipToProduct is RelationshipToProduct.Child)
					.Select(r => r.Asin)
					.ToList();

				if (childrenAsins.Count > 0)
				{
					var children = await api.GetCatalogProductsAsync(childrenAsins, CatalogOptions.ResponseGroupOptions.ALL_OPTIONS, 50, semaphore);
					
					//If the price is null, this item is not available to the user
					var childrenWithPrices = children.Where(p => p.Price != null).ToList();

					if (childrenWithPrices.Count > 0)
						items[series] = childrenWithPrices;
				}
			}

			//Await the wishlist asins
			var wishlistAsins = (await wishlistTask).Select(w => w.Asin).ToHashSet();

			var fullLib = DbContexts.GetLibrary_Flat_NoTracking();
			var seriesEntries = new Dictionary<Item, List<SeriesItem>>();

			//Create a SeriesItem liste for each series.
			foreach (var series in items.Keys)
			{
				ApiExtended.SetSeries(series, items[series]);

				seriesEntries[series] = new List<SeriesItem>();

				foreach (var item in items[series].Where(i => !string.IsNullOrEmpty(i.PictureId)))
				{
					var order = item.Series.Single(s => s.Asin == series.Asin).Sequence;
					//Match the account/book in the database
					var inLibrary = fullLib.Any(lb => lb.Account == libraryBook.Account && lb.Book.AudibleProductId == item.ProductId && !lb.AbsentFromLastScan);
					var inWishList = wishlistAsins.Contains(item.Asin);

					seriesEntries[series].Add(new SeriesItem(item, order, inLibrary, inWishList));
				}
			}

			return seriesEntries;
		}

		~SeriesItem()
		{
			PictureStorage.PictureCached -= PictureStorage_PictureCached;
			Button.PropertyChanged -= DownloadButton_PropertyChanged;
		}
	}
}
