using DataLayer;
using Dinah.Core;
using LibationFileManager;
using System;
using System.Linq;

namespace LibationAvalonia.Views
{
	public partial class MainWindow
	{
		private void Configure_ProcessQueue()
		{
			var collapseState = !Configuration.Instance.GetNonString(defaultValue: true, nameof(_viewModel.QueueOpen));
			SetQueueCollapseState(collapseState);
		}

		public async void ProductsDisplay_LiberateClicked(object sender, LibraryBook libraryBook)
		{
			try
			{
				if (libraryBook.Book.UserDefinedItem.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload)
				{
					Serilog.Log.Logger.Information("Begin single book backup of {libraryBook}", libraryBook);
					SetQueueCollapseState(false);
					_viewModel.ProcessQueue.AddDownloadDecrypt(libraryBook);
				}
				else if (libraryBook.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated)
				{
					Serilog.Log.Logger.Information("Begin single pdf backup of {libraryBook}", libraryBook);
					SetQueueCollapseState(false);
					_viewModel.ProcessQueue.AddDownloadPdf(libraryBook);
				}
				else if (libraryBook.Book.Audio_Exists())
				{
					// liberated: open explorer to file
					var filePath = AudibleFileStorage.Audio.GetPath(libraryBook.Book.AudibleProductId);

					if (!Go.To.File(filePath?.ShortPathName))
					{
						var suffix = string.IsNullOrWhiteSpace(filePath) ? "" : $":\r\n{filePath}";
						await MessageBox.Show($"File not found" + suffix);
					}
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while handling the stop light button click for {libraryBook}", libraryBook);
			}
		}

		public void ProductsDisplay_ConvertToMp3Clicked(object sender, LibraryBook libraryBook)
		{
			try
			{
				if (libraryBook.Book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated)
				{
					Serilog.Log.Logger.Information("Begin single pdf backup of {libraryBook}", libraryBook);
					SetQueueCollapseState(false);
					_viewModel.ProcessQueue.AddConvertMp3(libraryBook);
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while handling the stop light button click for {libraryBook}", libraryBook);
			}
		}
		private void SetQueueCollapseState(bool collapsed)
		{
			_viewModel.QueueOpen = !collapsed;
			Configuration.Instance.SetNonString(!collapsed, nameof(_viewModel.QueueOpen));
		}

		public void ToggleQueueHideBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			SetQueueCollapseState(_viewModel.QueueOpen);
		}
	}
}
