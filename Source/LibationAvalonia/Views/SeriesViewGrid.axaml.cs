using AudibleApi.Common;
using Avalonia.Collections;
using Avalonia.Controls;
using DataLayer;
using Dinah.Core;
using LibationAvalonia.Controls;
using LibationAvalonia.Dialogs;
using LibationFileManager;
using LibationUiBase.SeriesView;
using System.Collections.Generic;
using System.Linq;

namespace LibationAvalonia.Views
{
	public partial class SeriesViewGrid : UserControl
	{
		private ImageDisplayDialog? imageDisplayDialog;
		private readonly LibraryBook? LibraryBook;

		public AvaloniaList<SeriesItem> SeriesEntries { get; } = new();

		public SeriesViewGrid()
		{
			InitializeComponent();
			DataContext = this;
		}

		public SeriesViewGrid(LibraryBook libraryBook, Item series, List<SeriesItem> entries) : this()
		{
			LibraryBook = ArgumentValidator.EnsureNotNull(libraryBook, nameof(libraryBook));
			ArgumentValidator.EnsureNotNull(series, nameof(series));
			ArgumentValidator.EnsureNotNull(entries, nameof(entries));

			SeriesEntries.AddRange(entries.OrderBy(s => s.Order));
		}

		public async void Availability_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			if (LibraryBook is not null && sender is Button button && button.DataContext is SeriesItem sentry && sentry.Button.HasButtonAction)
			{
				await sentry.Button.PerformClickAsync(LibraryBook);
			}
		}
		public void Title_Click(object sender, Avalonia.Input.TappedEventArgs args)
		{
			if (LibraryBook is null || sender is not LinkLabel label || label.DataContext is not SeriesItem sentry)
				return;

			sentry.ViewOnAudible(LibraryBook.Book.Locale);
		}

		public void Cover_Click(object sender, Avalonia.Input.TappedEventArgs args)
		{
			if (sender is not Image tblock || tblock.DataContext is not SeriesItem sentry)
				return;

			Item libraryBook = sentry.Item;

			if (imageDisplayDialog is null || !imageDisplayDialog.IsVisible)
			{
				imageDisplayDialog = new ImageDisplayDialog();
			}

			var picDef = new PictureDefinition(libraryBook.PictureLarge ?? libraryBook.PictureId, PictureSize.Native);

			void PictureCached(object? sender, PictureCachedEventArgs e)
			{
				if (e.Definition.PictureId == picDef.PictureId)
					imageDisplayDialog.SetCoverBytes(e.Picture);

				PictureStorage.PictureCached -= PictureCached;
			}

			PictureStorage.PictureCached += PictureCached;
			(bool isDefault, byte[] initialImageBts) = PictureStorage.GetPicture(picDef);
			var windowTitle = $"{libraryBook.Title} - Cover";

			imageDisplayDialog.Title = windowTitle;
			imageDisplayDialog.SetCoverBytes(initialImageBts);

			if (!isDefault)
				PictureStorage.PictureCached -= PictureCached;

			if (imageDisplayDialog.IsVisible)
				imageDisplayDialog.Activate();
			else
				imageDisplayDialog.Show();
		}
	}
}
