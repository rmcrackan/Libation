using ApplicationServices;
using Avalonia.Collections;
using DataLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	public class ProductsDisplayViewModel : ViewModelBase
	{
		public string Greeting => "Welcome to Avalonia!";
		public GridEntryBindingList2 People { get; set; }
		public ProductsDisplayViewModel(IEnumerable<LibraryBook> dbBooks)
		{
			var geList = dbBooks
				.Where(lb => lb.Book.IsProduct())
				.Select(b => new LibraryBookEntry2(b))
				.Cast<GridEntry2>()
				.ToList();

			var episodes = dbBooks.Where(lb => lb.Book.IsEpisodeChild());

			var seriesBooks = dbBooks.Where(lb => lb.Book.IsEpisodeParent()).ToList();

			foreach (var parent in seriesBooks)
			{
				var seriesEpisodes = episodes.FindChildren(parent);

				if (!seriesEpisodes.Any()) continue;

				var seriesEntry = new SeriesEntrys2(parent, seriesEpisodes);

				geList.Add(seriesEntry);
				geList.AddRange(seriesEntry.Children);
			}

			People = new GridEntryBindingList2(geList.OrderByDescending(e => e.DateAdded));
			People.CollapseAll();
		}
	}

}
