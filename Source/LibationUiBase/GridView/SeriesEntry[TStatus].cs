using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibationUiBase.GridView
{
	/// <summary>The View Model for a LibraryBook that is ContentType.Parent</summary>
	public class SeriesEntry<TStatus> : GridEntry<TStatus>, ISeriesEntry where TStatus : IEntryStatus
	{
		public List<ILibraryBookEntry> Children { get; }
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
		public SeriesEntry(LibraryBook parent, IEnumerable<LibraryBook> children)
		{
			LastDownload = new();
			SeriesIndex = -1;

			Children = children
				.Select(c => new LibraryBookEntry<TStatus>(c, this))
				.OrderByDescending(c => c.SeriesOrder)
				.ToList<ILibraryBookEntry>();

			UpdateLibraryBook(parent);
			LoadCover();
		}

		public static async Task<List<ISeriesEntry>> GetAllSeriesEntriesAsync(IEnumerable<LibraryBook> libraryBooks)
		{
			var seriesBooks = libraryBooks.Where(lb => lb.Book.IsEpisodeParent()).ToArray();
			var allEpisodes = libraryBooks.Where(lb => lb.Book.IsEpisodeChild()).ToArray();

			var seriesEntries = new ISeriesEntry[seriesBooks.Length];
			var seriesEpisodes = new ILibraryBookEntry[seriesBooks.Length][];

			var syncContext = SynchronizationContext.Current;
			var options = new ParallelOptions { MaxDegreeOfParallelism = int.Max(1, Environment.ProcessorCount - 1) };

			//Asynchronously create an ILibraryBookEntry for every episode in the library
			await Parallel.ForEachAsync(getAllEpisodes(), options,  createEpisodeEntry);

			//Match all episode entries to their corresponding parents
			for (int i = seriesEntries.Length - 1; i >= 0; i--)
			{
				var series = seriesEntries[i];

				//Sort episodes by series order descending, then add them to their parent's entry
				Array.Sort(seriesEpisodes[i], (a, b) => -a.SeriesOrder.CompareTo(b.SeriesOrder));
				series.Children.AddRange(seriesEpisodes[i]);
				series.UpdateLibraryBook(series.LibraryBook);
			}

			return seriesEntries.Where(s => s.Children.Count != 0).Cast<ISeriesEntry>().ToList();

			//Create a LibraryBookEntry for a single episode
			ValueTask createEpisodeEntry((int seriesIndex, int episodeIndex, LibraryBook episode) data, CancellationToken cancellationToken)
			{
				SynchronizationContext.SetSynchronizationContext(syncContext);
				var parent = seriesEntries[data.seriesIndex];
				seriesEpisodes[data.seriesIndex][data.episodeIndex] = new LibraryBookEntry<TStatus>(data.episode, parent);
				return ValueTask.CompletedTask;
			}

			//Enumeration all series episodes, along with the index to its seriesEntries entry
			//and an index to its seriesEpisodes entry
			IEnumerable<(int seriesIndex, int episodeIndex, LibraryBook episode)> getAllEpisodes()
			{
				for (int i = 0; i < seriesBooks.Length; i++)
				{
					var series = seriesBooks[i];
					var childEpisodes = allEpisodes.FindChildren(series);

					SynchronizationContext.SetSynchronizationContext(syncContext);
					seriesEntries[i] = new SeriesEntry<TStatus>(series, []);
					seriesEpisodes[i] = new ILibraryBookEntry[childEpisodes.Count];

					for (int j = 0; j < childEpisodes.Count; j++)
						yield return (i, j, childEpisodes[j]);
				}
			}
		}

		public void RemoveChild(ILibraryBookEntry lbe)
		{
			Children.Remove(lbe);
			PurchaseDate = GetPurchaseDateString();
			Length = GetBookLengthString();
		}

		protected override string GetBookTags() => null;
		protected override int GetLengthInMinutes() => Children.Count == 0 ? 0 : Children.Sum(c => c.LibraryBook.Book.LengthInMinutes);
		protected override DateTime GetPurchaseDate() => Children.Count == 0 ? default : Children.Min(c => c.LibraryBook.DateAdded);
	}
}
