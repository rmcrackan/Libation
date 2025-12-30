using DataLayer;
using Dinah.Core.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibationUiBase.GridView
{
	/// <summary>The View Model for a LibraryBook that is ContentType.Parent</summary>
	public class SeriesEntry : GridEntry
	{
		public List<LibraryBookEntry> Children { get; }
		public override DateTime DateAdded => Children.Max(c => c.DateAdded);

		private bool suspendCounting = false;
		public void ChildRemoveUpdate()
		{
			if (suspendCounting) return;

			var removeCount = Children.Count(c => c.Remove == true);

			remove = removeCount == 0 ? false : removeCount == Children.Count ? true : null;
			RaisePropertyChanged(nameof(Remove));
		}

		public override bool? Remove
		{
			get => remove;
			set
			{
				remove = value ?? false;

				suspendCounting = true;

				foreach (var item in Children)
					item.Remove = value;

				suspendCounting = false;
				RaisePropertyChanged(nameof(Remove));
			}
		}

		public SeriesEntry(LibraryBook parent, LibraryBook child) : this(parent, new[] { child }) { }
		public SeriesEntry(LibraryBook parent, IEnumerable<LibraryBook> children) : base(parent)
		{
			LastDownload = new();
			SeriesIndex = -1;

			Children = children
				.Select(c => new LibraryBookEntry(c, this))
				.OrderByDescending(c => c.SeriesOrder)
				.ToList<LibraryBookEntry>();

			UpdateLibraryBook(parent);
			LoadCover();
		}

		/// <summary>
		/// Creates <see cref="SeriesEntry{TStatus}"/> for all episodic series in an enumeration of <see cref="LibraryBook"/>.
		/// </summary>
		/// <remarks>Can be called from any thread, but requires the calling thread's <see cref="SynchronizationContext.Current"/> to be valid.</remarks>
		public static async Task<List<SeriesEntry>> GetAllSeriesEntriesAsync(IEnumerable<LibraryBook> libraryBooks)
		{
			var seriesEntries = await GetAllProductsAsync(libraryBooks, lb => lb.Book.IsEpisodeParent(), lb => new SeriesEntry(lb, []));
			var seriesDict = seriesEntries.ToDictionarySafe(s => s.AudibleProductId);
			await GetAllProductsAsync(libraryBooks, lb => lb.Book.IsEpisodeChild(), CreateAndLinkEpisodeEntry);

			//sort episodes by series order descending and update SeriesEntry
			foreach (var series in seriesEntries)
			{
				series.Children.Sort((a, b) => -SeriesOrder.Compare(a.SeriesOrder, b.SeriesOrder));
				series.UpdateLibraryBook(series.LibraryBook);
			}

			return seriesEntries.Where(s => s.Children.Count != 0).ToList();

			//Create a LibraryBookEntry for an episode and link it to its series parent
			LibraryBookEntry? CreateAndLinkEpisodeEntry(LibraryBook episode)
			{
				foreach (var s in episode.Book.SeriesLink)
				{
					if (seriesDict.TryGetValue(s.Series.AudibleSeriesId, out var seriesParent))
					{
						var entry = new LibraryBookEntry(episode, seriesParent);
						seriesParent.Children.Add(entry);
						return entry;
					}
				}
				return null;
			}
		}

		public void RemoveChild(LibraryBookEntry lbe)
		{
			Children.Remove(lbe);
			PurchaseDate = GetPurchaseDateString();
			Length = GetBookLengthString();
		}

		protected override string? GetBookTags() => null;
		protected override int GetLengthInMinutes() => Children.Count == 0 ? 0 : Children.Sum(c => c.LibraryBook.Book.LengthInMinutes);
		protected override DateTime GetPurchaseDate() => Children.Count == 0 ? default : Children.Min(c => c.LibraryBook.DateAdded);
		protected override DateTime? GetIncludedUntil() => Children.Count == 0 ? default : Children.Min(c => c.LibraryBook.IncludedUntil);
	}
}
