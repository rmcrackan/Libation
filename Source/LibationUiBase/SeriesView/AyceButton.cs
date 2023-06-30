using ApplicationServices;
using AudibleApi;
using AudibleApi.Common;
using DataLayer;
using FileLiberator;
using System;
using System.Linq;
using Dinah.Core;
using System.Threading.Tasks;

namespace LibationUiBase.SeriesView
{
	internal class AyceButton : SeriesButton
	{
		//Making this event and field static prevents concurrent additions to the library.
		//Search engine indexer does not support concurrent re-indexing.
		private static event EventHandler ButtonEnabled;
		private static bool globalEnabled = true;

		public override bool HasButtonAction => true;
		public override string DisplayText
			=> InLibrary ? "Remove\r\nFrom\r\nLibrary"
			: "FREE\r\n\r\nAdd to\r\nLibrary";

		public override bool Enabled
		{
			get => globalEnabled;
			protected set
			{
				if (globalEnabled != value)
				{
					globalEnabled = value;
					ButtonEnabled?.Invoke(null, EventArgs.Empty);
				}
			}
		}

		internal AyceButton(Item item, bool inLibrary) : base(item, inLibrary)
		{
			ButtonEnabled += DownloadButton_ButtonEnabled;
		}

		public override async Task PerformClickAsync(LibraryBook accountBook)
		{
			if (!Enabled) return;

			Enabled = false;

			try
			{
				if (InLibrary)
					await RemoveFromLibraryAsync(accountBook);
				else
					await AddToLibraryAsync(accountBook);
			}
			catch(Exception ex)
			{
				var addRemove = InLibrary ? "remove" : "add";
				var toFrom = InLibrary ? "from" : "to";

				Serilog.Log.Logger.Error(ex, $"Failed to {addRemove} {{book}} {toFrom} library", new { Item.ProductId, Item.TitleWithSubtitle });
			}
			finally { Enabled = true; }

		}

		private async Task RemoveFromLibraryAsync(LibraryBook accountBook)
		{
			Api api = await accountBook.GetApiAsync();

			if (await api.RemoveItemFromLibraryAsync(Item.ProductId))
			{
				using var context = DbContexts.GetContext();
				var lb = context.GetLibraryBook_Flat_NoTracking(Item.ProductId);
				int result = await Task.Run((new[] { lb }).PermanentlyDeleteBooks);
				InLibrary = result == 0;
			}
		}

		private async Task AddToLibraryAsync(LibraryBook accountBook)
		{
			Api api = await accountBook.GetApiAsync();

			if (!await api.AddItemToLibraryAsync(Item.ProductId)) return;

			Item item = null;

			for (int tryCount = 0; tryCount < 5 && item is null; tryCount++)
			{
				//Wait a half second to allow the server time to update
				await Task.Delay(500);
				item = await api.GetLibraryBookAsync(Item.ProductId, LibraryOptions.ResponseGroupOptions.ALL_OPTIONS);
			}

			if (item is null) return;

			if (item.IsEpisodes)
			{
				var seriesParent = DbContexts.GetLibrary_Flat_NoTracking(includeParents: true)
					.Select(lb => lb.Book)
					.FirstOrDefault(b => b.IsEpisodeParent() && b.AudibleProductId.In(item.Relationships.Select((Relationship r) => r.Asin)));

				if (seriesParent is null) return;

				item.Series = new[]
				{
					new AudibleApi.Common.Series
					{
						Asin = seriesParent.AudibleProductId,
						Sequence = item.Relationships.FirstOrDefault(r => r.Asin == seriesParent.AudibleProductId)?.Sort?.ToString() ?? "0",
						Title = seriesParent.TitleWithSubtitle
					}
				};
			}

			InLibrary = await LibraryCommands.ImportSingleToDbAsync(item, accountBook.Account, accountBook.Book.Locale) != 0;
		}

		private void DownloadButton_ButtonEnabled(object sender, EventArgs e)
			=> OnPropertyChanged(nameof(Enabled));

		public override int CompareTo(object ob)
		{
			if (ob is not AyceButton other) return 1;
			return other.InLibrary.CompareTo(InLibrary);
		}

		~AyceButton()
		{
			ButtonEnabled -= DownloadButton_ButtonEnabled;
		}
	}
}
