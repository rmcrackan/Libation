using LibationFileManager;
using System;
using System.Linq;
using DataLayer;
using Dinah.Core;
using LibationUiBase.GridView;
using ReactiveUI;

namespace LibationAvalonia.ViewModels
{
	partial class MainVM
	{
		private bool _queueOpen = false;

		/// <summary> The Process Queue panel is open </summary>
		public bool QueueOpen
		{
			get => _queueOpen;
			set
			{
				this.RaiseAndSetIfChanged(ref _queueOpen, value);
				QueueButtonAngle = value ? 180 : 0;
				this.RaisePropertyChanged(nameof(QueueButtonAngle));
			}
		}

		public double QueueButtonAngle { get; private set; }

		private void Configure_ProcessQueue()
		{
			var collapseState = !Configuration.Instance.GetNonString(defaultValue: true, nameof(QueueOpen));
			setQueueCollapseState(collapseState);
		}

		public async void LiberateClicked(LibraryBook libraryBook)
		{
			try
			{
				if (libraryBook.Book.UserDefinedItem.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload)
				{
					Serilog.Log.Logger.Information("Begin single book backup of {libraryBook}", libraryBook);
					setQueueCollapseState(false);
					ProcessQueue.AddDownloadDecrypt(libraryBook);
				}
				else if (libraryBook.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated)
				{
					Serilog.Log.Logger.Information("Begin single pdf backup of {libraryBook}", libraryBook);
					setQueueCollapseState(false);
					ProcessQueue.AddDownloadPdf(libraryBook);
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

		public void LiberateSeriesClicked(ISeriesEntry series)
		{
			try
			{
				setQueueCollapseState(false);

				Serilog.Log.Logger.Information("Begin backing up all {series} episodes", series.LibraryBook);

				ProcessQueue.AddDownloadDecrypt(series.Children.Select(c => c.LibraryBook).UnLiberated());
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while backing up {series} episodes", series.LibraryBook);
			}
		}

		public void ConvertToMp3Clicked(LibraryBook libraryBook)
		{
			try
			{
				if (libraryBook.Book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated)
				{
					Serilog.Log.Logger.Information("Begin convert to mp3 {libraryBook}", libraryBook);
					setQueueCollapseState(false);
					ProcessQueue.AddConvertMp3(libraryBook);
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while handling the stop light button click for {libraryBook}", libraryBook);
			}
		}

		public void ToggleQueueHideBtn() => setQueueCollapseState(QueueOpen);
	}
}
