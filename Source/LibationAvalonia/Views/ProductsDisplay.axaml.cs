using ApplicationServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using DataLayer;
using Dinah.Core;
using FileLiberator;
using LibationAvalonia.Controls;
using LibationAvalonia.Dialogs;
using LibationAvalonia.ViewModels;
using LibationFileManager;
using LibationFileManager.Templates;
using LibationUiBase.Forms;
using LibationUiBase.GridView;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Views
{
	public partial class ProductsDisplay : UserControl
	{
		public event LiberateClickedHandler? LiberateClicked;
		public event EventHandler<SeriesEntry>? LiberateSeriesClicked;
		public event EventHandler<LibraryBook[]>? ConvertToMp3Clicked;
		public event EventHandler<LibraryBook>? TagsButtonClicked;

		private ProductsDisplayViewModel? _viewModel => DataContext as ProductsDisplayViewModel;
		ImageDisplayDialog? imageDisplayDialog;

		public ProductsDisplay()
		{
			InitializeComponent();
			DataGridContextMenus.CellContextMenuStripNeeded += ProductsGrid_CellContextMenuStripNeeded;

			var cellSelector = Selectors.Is<DataGridCell>(null);
			rowHeightStyle = new Style(_ => cellSelector);
			rowHeightStyle.Setters.Add(rowHeightSetter);

			var tboxSelector = cellSelector.Descendant().Is<TextBlock>();
			fontSizeStyle = new Style(_ => tboxSelector);
			fontSizeStyle.Setters.Add(fontSizeSetter);

			var tboxH1Selector = cellSelector.Child().Is<Panel>().Child().Is<TextBlock>().Class("h1");
			fontSizeH1Style = new Style(_ => tboxH1Selector);
			fontSizeH1Style.Setters.Add(fontSizeH1Setter);

			var tboxH2Selector = cellSelector.Child().Is<Panel>().Child().Is<TextBlock>().Class("h2");
			fontSizeH2Style = new Style(_ => tboxH2Selector);
			fontSizeH2Style.Setters.Add(fontSizeH2Setter);

			Configuration.Instance.PropertyChanged += Configuration_GridScaleChanged;
			Configuration.Instance.PropertyChanged += Configuration_FontChanged;

			#region Design Mode Testing
#if DEBUG
			if (Design.IsDesignMode)
			{
				MainVM.Configure_NonUI();
				LibraryBook[] sampleEntries = [
					MockLibraryBook.CreateBook(title: "Book 1"),
					MockLibraryBook.CreateBook(title: "Book 2"),
					MockLibraryBook.CreateBook(title: "Book 3"),
					MockLibraryBook.CreateBook(title: "Book 4"),
					MockLibraryBook.CreateBook(title: "Book 5"),
					MockLibraryBook.CreateBook(title: "Book 6")];

				var pdvm = new ProductsDisplayViewModel();
				_ = pdvm.BindToGridAsync(sampleEntries.OfType<LibraryBook>().ToList());
				DataContext = pdvm;

				setGridScale(1);
				setFontScale(1);
				return;
			}
#endif
			#endregion

			setGridScale(Configuration.Instance.GridScaleFactor);
			setFontScale(Configuration.Instance.GridFontScaleFactor);
			Configure_ColumnCustomization();

			foreach (var column in productsGrid.Columns)
			{
				column.CustomSortComparer = new RowComparer(column);
			}
		}

		private void ProductsDisplay_LoadingRow(object sender, DataGridRowEventArgs e)
		{
			if (e.Row.DataContext is LibraryBookEntry entry && entry.Liberate?.IsEpisode is true)
				e.Row.DynamicResource(DataGridRow.BackgroundProperty, "SeriesEntryGridBackgroundBrush");
			else
				e.Row.DynamicResource(DataGridRow.BackgroundProperty, "SystemRegionColor");
		}

		private void RemoveColumn_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
		{
			if (sender is DataGridColumn col && e.Property == DataGridColumn.IsVisibleProperty)
			{
				col.DisplayIndex = 0;
				col.CanUserReorder = false;
			}
		}

		#region Scaling

		[PropertyChangeFilter(nameof(Configuration.GridScaleFactor))]
		private void Configuration_GridScaleChanged(object sender, Dinah.Core.PropertyChangedEventArgsEx e)
		{
			if (e.NewValue is float value)
				setGridScale(value);
		}

		[PropertyChangeFilter(nameof(Configuration.GridFontScaleFactor))]
		private void Configuration_FontChanged(object sender, Dinah.Core.PropertyChangedEventArgsEx e)
		{
			if (e.NewValue is float value)
				setFontScale(value);
		}

		private readonly Style rowHeightStyle;
		private readonly Setter rowHeightSetter = new() { Property = DataGridCell.HeightProperty };

		private readonly Style fontSizeStyle;
		private readonly Setter fontSizeSetter = new() { Property = TextBlock.FontSizeProperty };

		private readonly Style fontSizeH1Style;
		private readonly Setter fontSizeH1Setter = new() { Property = TextBlock.FontSizeProperty };

		private readonly Style fontSizeH2Style;
		private readonly Setter fontSizeH2Setter = new() { Property = TextBlock.FontSizeProperty };

		private void setFontScale(double scaleFactor)
		{
			const double TextBlockFontSize = 11;
			const double H1FontSize = 14;
			const double H2FontSize = 12;

			fontSizeSetter.Value = TextBlockFontSize * scaleFactor;
			fontSizeH1Setter.Value = H1FontSize * scaleFactor;
			fontSizeH2Setter.Value = H2FontSize * scaleFactor;

			productsGrid.Styles.Remove(fontSizeStyle);
			productsGrid.Styles.Remove(fontSizeH1Style);
			productsGrid.Styles.Remove(fontSizeH2Style);
			productsGrid.Styles.Add(fontSizeStyle);
			productsGrid.Styles.Add(fontSizeH1Style);
			productsGrid.Styles.Add(fontSizeH2Style);
		}

		private void setGridScale(double scaleFactor)
		{
			const float BaseRowHeight = 80;
			const float BaseLiberateWidth = 75;
			const float BaseCoverWidth = 80;

			foreach (var column in productsGrid.Columns)
			{
				switch (column.SortMemberPath)
				{
					case nameof(GridEntry.Liberate):
						column.Width = new DataGridLength(BaseLiberateWidth * scaleFactor);
						break;
					case nameof(GridEntry.Cover):
						column.Width = new DataGridLength(BaseCoverWidth * scaleFactor);
						break;
				}
			}
			rowHeightSetter.Value = BaseRowHeight * scaleFactor;
			productsGrid.Styles.Remove(rowHeightStyle);
			productsGrid.Styles.Add(rowHeightStyle);
		}

		#endregion

		#region Cell Context Menu

		public void ProductsGrid_CellContextMenuStripNeeded(object? sender, DataGridCellContextMenuStripNeededEventArgs args)
		{
			var entries = args.GridEntries;
			var ctx = new GridContextMenu(entries, '_');

			if (App.MainWindow?.Clipboard is IClipboard clipboard)
			{
				//Avalonia's DataGrid can't select individual cells, so add separate
				//options for copying single cell's contents and who row contents.
				if (entries.Length == 1 && args.Column.SortMemberPath is not "Liberate" and not "Cover")
				{
					args.ContextMenuItems.Add(new MenuItem
					{
						Header = ctx.CopyCellText,
						Command = ReactiveCommand.CreateFromTask(() => clipboard?.SetTextAsync(args.CellClipboardContents) ?? Task.CompletedTask)
					});
				}

				args.ContextMenuItems.Add(new MenuItem
				{
					Header = "_Copy Row Contents",
					Command = ReactiveCommand.CreateFromTask(() => clipboard?.SetTextAsync(args.GetRowClipboardContents()) ?? Task.CompletedTask)
				});

				args.ContextMenuItems.Add(new Separator());
			}
			

			#region Liberate all Episodes (Single series only)

			if (entries.Length == 1 && entries[0] is SeriesEntry seriesEntry)
			{
				args.ContextMenuItems.Add(new MenuItem()
				{
					Header = ctx.LiberateEpisodesText,
					IsEnabled = ctx.LiberateEpisodesEnabled,
					Command = ReactiveCommand.Create(() => LiberateSeriesClicked?.Invoke(this, seriesEntry))
				});
			}

			#endregion
			#region Set Download status to Downloaded

			args.ContextMenuItems.Add(new MenuItem()
			{
				Header = ctx.SetDownloadedText,
				IsEnabled = ctx.SetDownloadedEnabled,
				Command = ReactiveCommand.Create(ctx.SetDownloaded)
			});

			#endregion
			#region Set Download status to Not Downloaded

			args.ContextMenuItems.Add(new MenuItem()
			{
				Header = ctx.SetNotDownloadedText,
				IsEnabled = ctx.SetNotDownloadedEnabled,
				Command = ReactiveCommand.Create(ctx.SetNotDownloaded)
			});

			#endregion
			#region Locate file (Single book only)

			if (entries.Length == 1 && entries[0] is LibraryBookEntry entry)
			{
				args.ContextMenuItems.Add(new MenuItem
				{
					Header = ctx.LocateFileText,
					Command = ReactiveCommand.CreateFromTask(async () =>
					{
						try
						{
							if (this.GetParentWindow() is not Window window)
								return;

							var openFileDialogOptions = new FilePickerOpenOptions
							{
								Title = ctx.LocateFileDialogTitle,
								AllowMultiple = false,
								SuggestedStartLocation = await window.StorageProvider.TryGetFolderFromPathAsync(Configuration.Instance.Books?.PathWithoutPrefix!),
								FileTypeFilter = new FilePickerFileType[]
								{
									new("All files (*.*)") { Patterns = new[] { "*" } },
								}
							};

							var selectedFiles = await window.StorageProvider.OpenFilePickerAsync(openFileDialogOptions);
							var selectedFile = selectedFiles.SingleOrDefault()?.TryGetLocalPath();

							if (selectedFile is not null)
								FilePathCache.Insert(entry.AudibleProductId, selectedFile);
						}
						catch (Exception ex)
						{
							await MessageBox.ShowAdminAlert(null, ctx.LocateFileErrorMessage, ctx.LocateFileErrorMessage, ex);
						}
					})
				});
			}

			#endregion
			#region Remove from library

			args.ContextMenuItems.Add(new MenuItem
			{
				Header = ctx.RemoveText,
				Command = ReactiveCommand.CreateFromTask(ctx.RemoveAsync)
			});

			#endregion
			#region Liberate All (multiple books only)
			if (entries.OfType<LibraryBookEntry>().Count() > 1)
			{
				args.ContextMenuItems.Add(new MenuItem
				{
					Header = ctx.DownloadSelectedText,
					Command = ReactiveCommand.Create(() => LiberateClicked?.Invoke(this, ctx.LibraryBookEntries.Select(e => e.LibraryBook).ToArray(), Configuration.Instance))
				});
			}

			#endregion
			#region Download split by chapters
			if (entries.Length == 1 && entries[0] is LibraryBookEntry entry3_a)
			{
				args.ContextMenuItems.Add(new MenuItem()
				{
					Header = ctx.DownloadAsChapters,
					IsEnabled = ctx.DownloadAsChaptersEnabled,
					Command = ReactiveCommand.Create(() =>
					{
						var config = Configuration.Instance.CreateEphemeralCopy();
						config.AllowLibationFixup = config.SplitFilesByChapter = true;
						var books = ctx.LibraryBookEntries.Select(e => e.LibraryBook).Where(lb => lb.Book.UserDefinedItem.BookStatus is not LiberatedStatus.Error).ToList();
						//No need to persist BookStatus changes. They only needs to last long for the files to start downloading
						books.ForEach(b => b.Book.UserDefinedItem.BookStatus = LiberatedStatus.NotLiberated);
						LiberateClicked?.Invoke(this, [entry3_a.LibraryBook], config);
					})
				});
			}
			#endregion
			#region Convert to Mp3

			if (ctx.LibraryBookEntries.Length > 0)
			{
				args.ContextMenuItems.Add(new MenuItem
				{
					Header = ctx.ConvertToMp3Text,
					IsEnabled = ctx.ConvertToMp3Enabled,
					Command = ReactiveCommand.Create(() => ConvertToMp3Clicked?.Invoke(this, ctx.LibraryBookEntries.Select(e => e.LibraryBook).ToArray()))
				});
			}

			#endregion
			#region Force Re-Download (Single book only)
			if (entries.Length == 1 && entries[0] is LibraryBookEntry entry4)
			{
				args.ContextMenuItems.Add(new MenuItem()
				{
					Header = ctx.ReDownloadText,
					IsEnabled = ctx.ReDownloadEnabled,
					Command = ReactiveCommand.Create(() =>
					{
						//No need to persist these changes. They only needs to last long for the files to start downloading
						entry4.Book.UserDefinedItem.BookStatus = LiberatedStatus.NotLiberated;
						if (entry4.Book.HasPdf)
							entry4.Book.UserDefinedItem.SetPdfStatus(LiberatedStatus.NotLiberated);
						LiberateClicked?.Invoke(this, [entry4.LibraryBook], Configuration.Instance);
					})
				});
			}
			#endregion

			if (entries.Length > 1)
				return;

			args.ContextMenuItems.Add(new Separator());

			#region Edit Templates (Single book only)

			async Task editTemplate<T>(LibraryBook libraryBook, string existingTemplate, Action<string> setNewTemplate)
				where T : Templates, LibationFileManager.Templates.ITemplate, new()
			{
				var template = ctx.CreateTemplateEditor<T>(libraryBook, existingTemplate);
				var form = new EditTemplateDialog(template);
				if (this.GetParentWindow() is Window window && await form.ShowDialog<DialogResult>(window) == DialogResult.OK)
				{
					setNewTemplate(template.EditingTemplate.TemplateText);
				}
			}

			if (entries.Length == 1 && entries[0] is LibraryBookEntry entry2)
			{
				args.ContextMenuItems.Add(new MenuItem
				{
					Header = ctx.EditTemplatesText,
					ItemsSource = new[]
					{
						new MenuItem
						{
							Header = ctx.FolderTemplateText,
							Command = ReactiveCommand.CreateFromTask(() => editTemplate<Templates.FolderTemplate>(entry2.LibraryBook, Configuration.Instance.FolderTemplate, t => Configuration.Instance.FolderTemplate = t))
						},
						new MenuItem
						{
							Header = ctx.FileTemplateText,
							Command = ReactiveCommand.CreateFromTask(() => editTemplate<Templates.FileTemplate>(entry2.LibraryBook, Configuration.Instance.FileTemplate, t => Configuration.Instance.FileTemplate = t))
						},
						new MenuItem
						{
							Header = ctx.MultipartTemplateText,
							Command = ReactiveCommand.CreateFromTask(() => editTemplate<Templates.ChapterFileTemplate>(entry2.LibraryBook, Configuration.Instance.ChapterFileTemplate, t => Configuration.Instance.ChapterFileTemplate = t))
						}
					}
				});
				args.ContextMenuItems.Add(new Separator());
			}

			#endregion
			#region View Bookmarks/Clips (Single book only)

			if (entries.Length == 1 && entries[0] is LibraryBookEntry entry3 && VisualRoot is Window window)
			{
				args.ContextMenuItems.Add(new MenuItem
				{
					Header = ctx.ViewBookmarksText,
					Command = ReactiveCommand.CreateFromTask(() => new BookRecordsDialog(entry3.LibraryBook).ShowDialog(window))
				});
			}

			#endregion
			#region View All Series (Single book only)

			if (entries.Length == 1 && entries[0].Book.SeriesLink.Any())
			{
				args.ContextMenuItems.Add(new MenuItem
				{
					Header = ctx.ViewSeriesText,
					Command = ReactiveCommand.Create(() => new SeriesViewDialog(entries[0].LibraryBook).Show())
				});
			}

			#endregion
		}

		#endregion

		#region Column Customizations

		private void Configure_ColumnCustomization()
		{
			if (Design.IsDesignMode) return;

			productsGrid.ColumnDisplayIndexChanged += ProductsGrid_ColumnDisplayIndexChanged;

			foreach (var column in productsGrid.Columns)
			{
				var itemName = column.SortMemberPath;
				if (itemName == nameof(GridEntry.Remove))
					continue;

				GridHeaderContextMenu.Items.Add(new MenuItem
				{
					Header = new CheckBox { Content = new TextBlock { Text = ((string)column.Header).Replace('\n', ' ') } },
					Tag = column,
				});

				column.IsVisible = Configuration.Instance.GetColumnVisibility(itemName);
			}

			//We must set DisplayIndex properties in ascending order
			var displayIndices = Configuration.Instance.GridColumnsDisplayIndices;
			foreach (var itemName in displayIndices.OrderBy(i => i.Value).Select(i => i.Key))
			{
				if (!productsGrid.Columns.Any(c => c.SortMemberPath == itemName))
					continue;

				var column = productsGrid.Columns
					.Single(c => c.SortMemberPath == itemName);

				column.DisplayIndex = displayIndices.GetValueOrDefault(itemName, productsGrid.Columns.IndexOf(column));
			}
		}

		public void ContextMenu_ContextMenuOpening(object? sender, System.ComponentModel.CancelEventArgs e)
		{
			if (sender is not ContextMenu contextMenu)
				return;
			foreach (var mi in contextMenu.Items.OfType<MenuItem>())
			{
				if (mi.Tag is DataGridColumn column && mi.Header is CheckBox cbox)
				{
					cbox.IsChecked = column.IsVisible;
				}
			}
		}

		public void ContextMenu_MenuClosed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (sender is not ContextMenu contextMenu)
				return;
			var config = Configuration.Instance;
			var dictionary = config.GridColumnsVisibilities;

			foreach (var mi in contextMenu.Items.OfType<MenuItem>())
			{
				if (mi.Tag is DataGridColumn column && mi.Header is CheckBox cbox)
				{
					column.IsVisible = cbox.IsChecked == true;
					dictionary[column.SortMemberPath] = cbox.IsChecked == true;
				}
			}

			//If all columns are hidden, register the context menu on the grid so users can unhide.
			if (!productsGrid.Columns.Any(c => c.IsVisible))
				productsGrid.ContextMenu = contextMenu;
			else
				productsGrid.ContextMenu = null;

			config.GridColumnsVisibilities = dictionary;
		}

		private void ProductsGrid_ColumnDisplayIndexChanged(object? sender, DataGridColumnEventArgs e)
		{
			var config = Configuration.Instance;

			var dictionary = config.GridColumnsDisplayIndices;
			dictionary[e.Column.SortMemberPath] = e.Column.DisplayIndex;
			config.GridColumnsDisplayIndices = dictionary;
		}

		#endregion

		#region Button Click Handlers

		public async void LiberateButton_Click(object sender, EventArgs e)
		{
			if (sender is not LiberateStatusButton button)
				return;

			if (button.DataContext is SeriesEntry sEntry && _viewModel is not null)
			{
				await _viewModel.ToggleSeriesExpanded(sEntry);

				//Expanding and collapsing reset the list, which will cause focus to shift
				//to the topright cell. Reset focus onto the clicked button's cell.
				button.Focus();
			}
			else if (button.DataContext is LibraryBookEntry lbEntry)
			{
				LiberateClicked?.Invoke(this, [lbEntry.LibraryBook], Configuration.Instance);
			}
		}

		public void CloseImageDisplay()
		{
			if (imageDisplayDialog is not null && imageDisplayDialog.IsVisible)
				imageDisplayDialog.Close();
		}

		public void Version_DoubleClick(object sender, Avalonia.Input.TappedEventArgs args)
		{
			if (sender is Control panel && panel.DataContext is LibraryBookEntry lbe && lbe.LastDownload?.IsValid is true)
				lbe.LastDownload.OpenReleaseUrl();
		}

		public void Cover_Click(object sender, Avalonia.Input.TappedEventArgs args)
		{
			if (sender is not Image tblock || tblock.DataContext is not GridEntry gEntry)
				return;

			if (imageDisplayDialog is null || !imageDisplayDialog.IsVisible)
			{
				imageDisplayDialog = new ImageDisplayDialog();
			}

			var picDef = new PictureDefinition(gEntry.LibraryBook.Book.PictureLarge ?? gEntry.LibraryBook.Book.PictureId, PictureSize.Native);

			void PictureCached(object? sender, PictureCachedEventArgs e)
			{
				if (e.Definition.PictureId == picDef.PictureId)
					imageDisplayDialog.SetCoverBytes(e.Picture);

				PictureStorage.PictureCached -= PictureCached;
			}

			PictureStorage.PictureCached += PictureCached;
			(bool isDefault, byte[] initialImageBts) = PictureStorage.GetPicture(picDef);


			var windowTitle = $"{gEntry.Title} - Cover";


			imageDisplayDialog.BookSaveDirectory = AudibleFileStorage.Audio.GetDestinationDirectory(gEntry.LibraryBook);
			imageDisplayDialog.PictureFileName = System.IO.Path.GetFileName(AudibleFileStorage.Audio.GetBooksDirectoryFilename(gEntry.LibraryBook, ".jpg"));
			imageDisplayDialog.Title = windowTitle;
			imageDisplayDialog.SetCoverBytes(initialImageBts);

			if (!isDefault)
				PictureStorage.PictureCached -= PictureCached;

			if (imageDisplayDialog.IsVisible)
				imageDisplayDialog.Activate();
			else
				imageDisplayDialog.Show();
		}

		public void Description_Click(object sender, Avalonia.Input.TappedEventArgs args)
		{
			if (sender is Control tblock && tblock.DataContext is GridEntry gEntry)
			{
				var pt = tblock.PointToScreen(tblock.Bounds.TopRight);
				var displayWindow = new DescriptionDisplayDialog
				{
					SpawnLocation = new Point(pt.X, pt.Y),
					DescriptionText = gEntry.Description,
				};

				void CloseWindow(object? o, DataGridRowEventArgs e)
				{
					displayWindow.Close();
				}
				productsGrid.LoadingRow += CloseWindow;
				displayWindow.Closing += (_, _) =>
				{
					productsGrid.LoadingRow -= CloseWindow;
				};

				displayWindow.Show();
			}
		}

		public void OnTagsButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var button = args.Source as Button;

			if (button?.DataContext is LibraryBookEntry lbEntry)
			{
				TagsButtonClicked?.Invoke(this, lbEntry.LibraryBook);
			}
		}

		#endregion
	}
}
