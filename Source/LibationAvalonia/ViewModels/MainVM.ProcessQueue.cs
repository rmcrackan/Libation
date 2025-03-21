using LibationFileManager;
using System;
using System.Linq;
using DataLayer;
using Dinah.Core;
using LibationUiBase.GridView;
using ReactiveUI;

#nullable enable
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

		public async void LiberateClicked(LibraryBook[] libraryBooks)
		{
			try
			{
				if (libraryBooks.Length == 1)
				{
					var item = libraryBooks[0];
					if (item.Book.UserDefinedItem.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload)
					{
						Serilog.Log.Logger.Information("Begin single book backup of {libraryBook}", item);
						setQueueCollapseState(false);
						ProcessQueue.AddDownloadDecrypt(item);
					}
					else if (item.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated)
					{
						Serilog.Log.Logger.Information("Begin single pdf backup of {libraryBook}", item);
						setQueueCollapseState(false);
						ProcessQueue.AddDownloadPdf(item);
					}
					else if (item.Book.Audio_Exists())
					{
						// liberated: open explorer to file
						var filePath = AudibleFileStorage.Audio.GetPath(item.Book.AudibleProductId);

						if (!Go.To.File(filePath?.ShortPathName))
						{
							var suffix = string.IsNullOrWhiteSpace(filePath) ? "" : $":\r\n{filePath}";
							await MessageBox.Show($"File not found" + suffix);
						}
					}
				}
				else
				{
					var toLiberate
						= libraryBooks
						.Where(x => x.Book.UserDefinedItem.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload || x.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated)
						.ToArray();

					if (toLiberate.Length > 0)
					{
						setQueueCollapseState(false);
						ProcessQueue.AddDownloadDecrypt(toLiberate);
					}
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while handling the stop light button click for {libraryBook}", libraryBooks);
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

		public void ConvertToMp3Clicked(LibraryBook[] libraryBooks)
		{
			try
			{
				var preLiberated = libraryBooks.Where(lb => lb.Book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated).ToArray();
				if (preLiberated.Length > 0)
				{
					Serilog.Log.Logger.Information("Begin convert {count} books to mp3", preLiberated.Length);
					setQueueCollapseState(false);
					ProcessQueue.AddConvertMp3(preLiberated);
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while handling the stop light button click for {libraryBook}", libraryBooks);
			}
		}

		public void ToggleQueueHideBtn() => setQueueCollapseState(QueueOpen);
	}
}
