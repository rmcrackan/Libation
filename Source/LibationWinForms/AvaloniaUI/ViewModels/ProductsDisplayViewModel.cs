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
		public GridEntryBindingList2 GridEntries { get; set; }
		public DataGridCollectionView GridCollectionView { get; set; }
		public ProductsDisplayViewModel(IEnumerable<LibraryBook> dbBooks)
		{
			GridEntries = new GridEntryBindingList2(CreateGridEntries(dbBooks));
			GridEntries.CollapseAll();

			/*
			 * Would be nice to use built-in groups, but Avalonia doesn't yet let you customize the row group header.
			 * 
			GridCollectionView = new DataGridCollectionView(GridEntries);
			GridCollectionView.GroupDescriptions.Add(new CustonGroupDescription());
			*/
		}

		public static IEnumerable<GridEntry2> CreateGridEntries(IEnumerable<LibraryBook> dbBooks)
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
			return geList.OrderByDescending(e => e.DateAdded);
		}
	}
	class CustonGroupDescription : DataGridGroupDescription
	{
		public override object GroupKeyFromItem(object item, int level, CultureInfo culture)
		{
			if (item is SeriesEntrys2 sEntry)
				return sEntry;
			else if (item is LibraryBookEntry2 lbEntry && lbEntry.Parent is SeriesEntrys2 sEntry2)
				return sEntry2;
			else return null;
		}
		public override bool KeysMatch(object groupKey, object itemKey)
		{
			return base.KeysMatch(groupKey, itemKey);
		}
	}
}
