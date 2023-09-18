using ApplicationServices;
using AudibleApi;
using AudibleApi.Common;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using FileLiberator;
using LibationFileManager;
using LibationUiBase;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels
{
	public enum ProcessBookResult
	{
		None,
		Success,
		Cancelled,
		ValidationFail,
		FailedRetry,
		FailedSkip,
		FailedAbort,
		LicenseDenied,
		LicenseDeniedPossibleOutage
	}

	public enum ProcessBookStatus
	{
		Queued,
		Cancelled,
		Working,
		Completed,
		Failed
	}

	/// <summary>
	/// This is the viewmodel for queued processables
	/// </summary>
	public class ProcessBookViewModel : ViewModelBase
	{
		public event EventHandler Completed;

		public LibraryBook LibraryBook { get; private set; }

		private ProcessBookResult _result = ProcessBookResult.None;
		private ProcessBookStatus _status = ProcessBookStatus.Queued;
		private string _narrator;
		private string _author;
		private string _title;
		private int _progress;
		private string _eta;
		private Bitmap _cover;

		#region Properties exposed to the view
		public ProcessBookResult Result { get => _result; set { this.RaiseAndSetIfChanged(ref _result, value); this.RaisePropertyChanged(nameof(StatusText)); } }
		public ProcessBookStatus Status { get => _status; set { this.RaiseAndSetIfChanged(ref _status, value); this.RaisePropertyChanged(nameof(BackgroundColor)); this.RaisePropertyChanged(nameof(IsFinished)); this.RaisePropertyChanged(nameof(IsDownloading)); this.RaisePropertyChanged(nameof(Queued)); } }
		public string Narrator { get => _narrator; set => Dispatcher.UIThread.Invoke(() => this.RaiseAndSetIfChanged(ref _narrator, value)); }
		public string Author { get => _author; set => Dispatcher.UIThread.Invoke(() => this.RaiseAndSetIfChanged(ref _author, value)); }
		public string Title { get => _title; set => Dispatcher.UIThread.Invoke(() => this.RaiseAndSetIfChanged(ref _title, value)); }
		public int Progress { get => _progress; private set => Dispatcher.UIThread.Invoke(() => this.RaiseAndSetIfChanged(ref _progress, value)); }
		public string ETA { get => _eta; private set => Dispatcher.UIThread.Invoke(() => this.RaiseAndSetIfChanged(ref _eta, value)); }
		public Bitmap Cover { get => _cover; private set => Dispatcher.UIThread.Invoke(() => this.RaiseAndSetIfChanged(ref _cover, value)); }
		public bool IsFinished => Status is not ProcessBookStatus.Queued and not ProcessBookStatus.Working;
		public bool IsDownloading => Status is ProcessBookStatus.Working;
		public bool Queued => Status is ProcessBookStatus.Queued;

		public IBrush BackgroundColor => Status switch
		{
			ProcessBookStatus.Cancelled => App.ProcessQueueBookCancelledBrush,
			ProcessBookStatus.Completed => App.ProcessQueueBookCompletedBrush,
			ProcessBookStatus.Failed => App.ProcessQueueBookFailedBrush,
			_ => App.ProcessQueueBookDefaultBrush,
		};
		public string StatusText => Result switch
		{
			ProcessBookResult.Success => "Finished",
			ProcessBookResult.Cancelled => "Cancelled",
			ProcessBookResult.ValidationFail => "Validion fail",
			ProcessBookResult.FailedRetry => "Error, will retry later",
			ProcessBookResult.FailedSkip => "Error, Skippping",
			ProcessBookResult.FailedAbort => "Error, Abort",
			ProcessBookResult.LicenseDenied => "License Denied",
			ProcessBookResult.LicenseDeniedPossibleOutage => "Possible Service Interruption",
			_ => Status.ToString(),
		};

		#endregion

		private TimeSpan TimeRemaining { set { ETA = $"ETA: {value:mm\\:ss}"; } }
		private Processable CurrentProcessable => _currentProcessable ??= Processes.Dequeue().Invoke();
		private Processable NextProcessable() => _currentProcessable = null;
		private Processable _currentProcessable;
		private readonly Queue<Func<Processable>> Processes = new();
		private readonly LogMe Logger;

		public ProcessBookViewModel(LibraryBook libraryBook, LogMe logme)
		{
			LibraryBook = libraryBook;
			Logger = logme;

			_title = LibraryBook.Book.TitleWithSubtitle;
			_author = LibraryBook.Book.AuthorNames();
			_narrator = LibraryBook.Book.NarratorNames();

			(bool isDefault, byte[] picture) = PictureStorage.GetPicture(new PictureDefinition(LibraryBook.Book.PictureId, PictureSize._80x80));

			if (isDefault)
				PictureStorage.PictureCached += PictureStorage_PictureCached;

			// Mutable property. Set the field so PropertyChanged isn't fired.
			_cover = AvaloniaUtils.TryLoadImageOrDefault(picture, PictureSize._80x80);
		}

		private void PictureStorage_PictureCached(object sender, PictureCachedEventArgs e)
		{
			if (e.Definition.PictureId == LibraryBook.Book.PictureId)
			{
				Cover = AvaloniaUtils.TryLoadImageOrDefault(e.Picture, PictureSize._80x80);
				PictureStorage.PictureCached -= PictureStorage_PictureCached;
			}
		}

		public async Task<ProcessBookResult> ProcessOneAsync()
		{
			string procName = CurrentProcessable.Name;
			ProcessBookResult result = ProcessBookResult.None;
			try
			{
				LinkProcessable(CurrentProcessable);

				var statusHandler = await CurrentProcessable.ProcessSingleAsync(LibraryBook, validate: true);

				if (statusHandler.IsSuccess)
					result = ProcessBookResult.Success;
				else if (statusHandler.Errors.Contains("Cancelled"))
				{
					Logger.Info($"{procName}:  Process was cancelled - {LibraryBook.Book}");
					result = ProcessBookResult.Cancelled;
				}
				else if (statusHandler.Errors.Contains("Validation failed"))
				{
					Logger.Info($"{procName}:  Validation failed - {LibraryBook.Book}");
					result = ProcessBookResult.ValidationFail;
				}
				else
				{
					foreach (var errorMessage in statusHandler.Errors)
						Logger.Error($"{procName}:  {errorMessage}");
				}
			}
			catch (ContentLicenseDeniedException ldex)
			{
				if (ldex.AYCL?.RejectionReason is null or RejectionReason.GenericError)
				{
					Logger.Info($"{procName}:  Content license was denied, but this error appears to be caused by a temporary interruption of service. - {LibraryBook.Book}");
					result = ProcessBookResult.LicenseDeniedPossibleOutage;
				}
				else
				{
					Logger.Info($"{procName}:  Content license denied. Check your Audible account to see if you have access to this title. - {LibraryBook.Book}");
					result = ProcessBookResult.LicenseDenied;
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex, procName);
			}
			finally
			{
				if (result == ProcessBookResult.None)
					result = await showRetry(LibraryBook);

				var status = result switch
				{
					ProcessBookResult.Success => ProcessBookStatus.Completed,
					ProcessBookResult.Cancelled => ProcessBookStatus.Cancelled,
					_ => ProcessBookStatus.Failed,
				};

				await Dispatcher.UIThread.InvokeAsync(() => Status = status);
			}

			await Dispatcher.UIThread.InvokeAsync(() => Result = result);
			return result;
		}

		public async Task CancelAsync()
		{
			try
			{
				if (CurrentProcessable is AudioDecodable audioDecodable)
					await audioDecodable.CancelAsync();
			}
			catch (Exception ex)
			{
				Logger.Error(ex, $"{CurrentProcessable.Name}:  Error while cancelling");
			}
		}

		public void AddDownloadPdf() => AddProcessable<DownloadPdf>();
		public void AddDownloadDecryptBook() => AddProcessable<DownloadDecryptBook>();
		public void AddConvertToMp3() => AddProcessable<ConvertToMp3>();

		private void AddProcessable<T>() where T : Processable, new()
		{
			Processes.Enqueue(() => new T());
		}

		public override string ToString() => LibraryBook.ToString();

		#region Subscribers and Unsubscribers

		private void LinkProcessable(Processable processable)
		{
			processable.Begin += Processable_Begin;
			processable.Completed += Processable_Completed;
			processable.StreamingProgressChanged += Streamable_StreamingProgressChanged;
			processable.StreamingTimeRemaining += Streamable_StreamingTimeRemaining;

			if (processable is AudioDecodable audioDecodable)
			{
				audioDecodable.RequestCoverArt += AudioDecodable_RequestCoverArt;
				audioDecodable.TitleDiscovered += AudioDecodable_TitleDiscovered;
				audioDecodable.AuthorsDiscovered += AudioDecodable_AuthorsDiscovered;
				audioDecodable.NarratorsDiscovered += AudioDecodable_NarratorsDiscovered;
				audioDecodable.CoverImageDiscovered += AudioDecodable_CoverImageDiscovered;
			}
		}

		private void UnlinkProcessable(Processable processable)
		{
			processable.Begin -= Processable_Begin;
			processable.Completed -= Processable_Completed;
			processable.StreamingProgressChanged -= Streamable_StreamingProgressChanged;
			processable.StreamingTimeRemaining -= Streamable_StreamingTimeRemaining;

			if (processable is AudioDecodable audioDecodable)
			{
				audioDecodable.RequestCoverArt -= AudioDecodable_RequestCoverArt;
				audioDecodable.TitleDiscovered -= AudioDecodable_TitleDiscovered;
				audioDecodable.AuthorsDiscovered -= AudioDecodable_AuthorsDiscovered;
				audioDecodable.NarratorsDiscovered -= AudioDecodable_NarratorsDiscovered;
				audioDecodable.CoverImageDiscovered -= AudioDecodable_CoverImageDiscovered;
			}
		}

		#endregion

		#region AudioDecodable event handlers

		private void AudioDecodable_TitleDiscovered(object sender, string title) => Title = title;

		private void AudioDecodable_AuthorsDiscovered(object sender, string authors) => Author = authors;

		private void AudioDecodable_NarratorsDiscovered(object sender, string narrators) => Narrator = narrators;


		private byte[] AudioDecodable_RequestCoverArt(object sender, EventArgs e)
		{
			var quality
				= Configuration.Instance.FileDownloadQuality == Configuration.DownloadQuality.High && LibraryBook.Book.PictureLarge is not null
				? new PictureDefinition(LibraryBook.Book.PictureLarge, PictureSize.Native)
				: new PictureDefinition(LibraryBook.Book.PictureId, PictureSize._500x500);

			byte[] coverData = PictureStorage.GetPictureSynchronously(quality);

			AudioDecodable_CoverImageDiscovered(this, coverData);
			return coverData;
		}

		private void AudioDecodable_CoverImageDiscovered(object sender, byte[] coverArt)
		{
			using var ms = new System.IO.MemoryStream(coverArt);
			Cover = new Avalonia.Media.Imaging.Bitmap(ms);
		}

		#endregion

		#region Streamable event handlers
		private void Streamable_StreamingTimeRemaining(object sender, TimeSpan timeRemaining) => TimeRemaining = timeRemaining;


		private void Streamable_StreamingProgressChanged(object sender, Dinah.Core.Net.Http.DownloadProgress downloadProgress)
		{
			if (!downloadProgress.ProgressPercentage.HasValue)
				return;

			if (downloadProgress.ProgressPercentage == 0)
				TimeRemaining = TimeSpan.Zero;
			else
				Progress = (int)downloadProgress.ProgressPercentage;
		}

		#endregion

		#region Processable event handlers

		private async void Processable_Begin(object sender, LibraryBook libraryBook)
		{
			await Dispatcher.UIThread.InvokeAsync(() => Status = ProcessBookStatus.Working);

			Logger.Info($"{Environment.NewLine}{((Processable)sender).Name} Step, Begin: {libraryBook.Book}");

			Title = libraryBook.Book.TitleWithSubtitle;
			Author = libraryBook.Book.AuthorNames();
			Narrator = libraryBook.Book.NarratorNames();
		}

		private async void Processable_Completed(object sender, LibraryBook libraryBook)
		{
			Logger.Info($"{((Processable)sender).Name} Step, Completed: {libraryBook.Book}");
			UnlinkProcessable((Processable)sender);

			if (Processes.Count == 0)
			{
				Completed?.Invoke(this, EventArgs.Empty);
				return;
			}

			NextProcessable();
			LinkProcessable(CurrentProcessable);

			StatusHandler result;
			try
			{
				result = await CurrentProcessable.ProcessSingleAsync(libraryBook, validate: true);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"{nameof(Processable_Completed)} error");

				result = new StatusHandler();
				result.AddError($"{nameof(Processable_Completed)} error. See log for details. Error summary: {ex.Message}");
			}

			if (result.HasErrors)
			{
				foreach (var errorMessage in result.Errors.Where(e => e != "Validation failed"))
					Logger.Error(errorMessage);

				Completed?.Invoke(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Failure Handler

		private async Task<ProcessBookResult> showRetry(LibraryBook libraryBook)
		{
			Logger.Error("ERROR. All books have not been processed. Most recent book: processing failed");

			DialogResult? dialogResult = Configuration.Instance.BadBook switch
			{
				Configuration.BadBookAction.Abort => DialogResult.Abort,
				Configuration.BadBookAction.Retry => DialogResult.Retry,
				Configuration.BadBookAction.Ignore => DialogResult.Ignore,
				Configuration.BadBookAction.Ask => null,
				_ => null
			};

			string details;
			try
			{
				static string trunc(string str)
					=> string.IsNullOrWhiteSpace(str) ? "[empty]"
					: (str.Length > 50) ? $"{str.Truncate(47)}..."
					: str;

				details =
$@"  Title: {libraryBook.Book.TitleWithSubtitle}
  ID: {libraryBook.Book.AudibleProductId}
  Author: {trunc(libraryBook.Book.AuthorNames())}
  Narr: {trunc(libraryBook.Book.NarratorNames())}";
			}
			catch
			{
				details = "[Error retrieving details]";
			}

			// if null then ask user
			dialogResult ??= await MessageBox.Show(string.Format(SkipDialogText + "\r\n\r\nSee Settings to avoid this box in the future.", details), "Skip importing this book?", SkipDialogButtons, MessageBoxIcon.Question, SkipDialogDefaultButton);

			if (dialogResult == DialogResult.Abort)
				return ProcessBookResult.FailedAbort;

			if (dialogResult == SkipResult)
			{
				libraryBook.UpdateBookStatus(LiberatedStatus.Error);

				Logger.Info($"Error. Skip: [{libraryBook.Book.AudibleProductId}] {libraryBook.Book.TitleWithSubtitle}");

				return ProcessBookResult.FailedSkip;
			}

			return ProcessBookResult.FailedRetry;
		}

		private static string SkipDialogText => @"
An error occurred while trying to process this book.
{0}

- ABORT: Stop processing books.

- RETRY: retry this book later. Just skip it for now. Continue processing books. (Will try this book again later.)

- IGNORE: Permanently ignore this book. Continue processing books. (Will not try this book again later.)
".Trim();
		private static MessageBoxButtons SkipDialogButtons => MessageBoxButtons.AbortRetryIgnore;
		private static MessageBoxDefaultButton SkipDialogDefaultButton => MessageBoxDefaultButton.Button1;
		private static DialogResult SkipResult => DialogResult.Ignore;
	}

	#endregion
}
