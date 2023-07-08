using DataLayer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
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

			int parallelism = int.Max(1, Environment.ProcessorCount - 1);

			var tasks = new Task[parallelism];
			var syncContext = SynchronizationContext.Current;

			var q = new BlockingCollection<(int, LibraryBook episode)>();

			var seriesEntries = new ISeriesEntry[seriesBooks.Length];
			var seriesEpisodes = new ConcurrentBag<ILibraryBookEntry>[seriesBooks.Length];

			for (int i = 0; i < parallelism; i++)
			{
				tasks[i] = Task.Run(() =>
				{
					SynchronizationContext.SetSynchronizationContext(syncContext);

					while (q.TryTake(out var entry, -1))
					{
						var parent = seriesEntries[entry.Item1];
						var episodeBag = seriesEpisodes[entry.Item1];
						episodeBag.Add(new LibraryBookEntry<TStatus>(entry.episode, parent));
					}
				});
			}

			for (int i = 0; i <seriesBooks.Length; i++)
			{
				var series = seriesBooks[i];
				seriesEntries[i] = new SeriesEntry<TStatus>(series, Enumerable.Empty<LibraryBook>());
				seriesEpisodes[i] = new ConcurrentBag<ILibraryBookEntry>();

				foreach (var ep in allEpisodes.FindChildren(series))
					q.Add((i, ep));
			}

			q.CompleteAdding();

			await Task.WhenAll(tasks);

			for (int i = 0; i < seriesBooks.Length; i++)
			{
				var series = seriesEntries[i];
				series.Children.AddRange(seriesEpisodes[i].OrderByDescending(c => c.SeriesOrder));
				series.UpdateLibraryBook(series.LibraryBook);
			}

			return seriesEntries.Where(s => s.Children.Count != 0).ToList();
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
