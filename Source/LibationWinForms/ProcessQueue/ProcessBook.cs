using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationServices;
using DataLayer;
using Dinah.Core;
using FileLiberator;
using LibationFileManager;

namespace LibationWinForms.ProcessQueue
{
	public enum ProcessBookResult
	{
		None,
		Success,
		Cancelled,
		ValidationFail,
		FailedRetry,
		FailedSkip,
		FailedAbort
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
	public class ProcessBook : INotifyPropertyChanged
	{
		public event EventHandler Completed;
		public event PropertyChangedEventHandler PropertyChanged;

		private ProcessBookResult _result = ProcessBookResult.None;
		private ProcessBookStatus _status = ProcessBookStatus.Queued;
		private string _bookText;
		private int _progress;
		private TimeSpan _timeRemaining;
		private Image _cover;

		public ProcessBookResult Result { get => _result; private set { _result = value; NotifyPropertyChanged(); } }
		public ProcessBookStatus Status { get => _status; private set { _status = value; NotifyPropertyChanged(); } }
		public string BookText { get => _bookText; private set { _bookText = value; NotifyPropertyChanged(); } }
		public int Progress { get => _progress; private set { _progress = value; NotifyPropertyChanged(); } }
		public TimeSpan TimeRemaining { get => _timeRemaining; private set { _timeRemaining = value; NotifyPropertyChanged(); } }
		public Image Cover { get => _cover; private set { _cover = value; NotifyPropertyChanged(); } }

		public LibraryBook LibraryBook { get; private set; }
		private Processable CurrentProcessable => _currentProcessable ??= Processes.Dequeue().Invoke();
		private Processable NextProcessable() => _currentProcessable = null;
		private Processable _currentProcessable;
		private readonly Queue<Func<Processable>> Processes = new();
		private readonly LogMe Logger;

		public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public ProcessBook(LibraryBook libraryBook, LogMe logme)
		{
			LibraryBook = libraryBook;
			Logger = logme;

			title = LibraryBook.Book.Title;
			authorNames = LibraryBook.Book.AuthorNames();
			narratorNames = LibraryBook.Book.NarratorNames();
			_bookText = $"{title}\r\nBy {authorNames}\r\nNarrated by {narratorNames}";

			(bool isDefault, byte[] picture) = PictureStorage.GetPicture(new PictureDefinition(LibraryBook.Book.PictureId, PictureSize._80x80));

			if (isDefault)
				PictureStorage.PictureCached += PictureStorage_PictureCached;
			_cover = Dinah.Core.Drawing.ImageReader.ToImage(picture);

		}

		private void PictureStorage_PictureCached(object sender, PictureCachedEventArgs e)
		{
			if (e.Definition.PictureId == LibraryBook.Book.PictureId)
			{
				Cover = Dinah.Core.Drawing.ImageReader.ToImage(e.Picture);
				PictureStorage.PictureCached -= PictureStorage_PictureCached;
			}
		}

		public async Task<ProcessBookResult> ProcessOneAsync()
		{
			string procName = CurrentProcessable.Name;
			try
			{
				LinkProcessable(CurrentProcessable);

				var statusHandler = await CurrentProcessable.ProcessSingleAsync(LibraryBook, validate: true);

				if (statusHandler.IsSuccess)
					return Result = ProcessBookResult.Success;
				else if (statusHandler.Errors.Contains("Cancelled"))
				{
					Logger.Info($"{procName}:  Process was cancelled {LibraryBook.Book}");
					return Result = ProcessBookResult.Cancelled;
				}
				else if (statusHandler.Errors.Contains("Validation failed"))
				{
					Logger.Info($"{procName}:  Validation failed {LibraryBook.Book}");
					return Result = ProcessBookResult.ValidationFail;
				}

				foreach (var errorMessage in statusHandler.Errors)
					Logger.Error($"{procName}:  {errorMessage}");
			}
			catch (Exception ex)
			{
				Logger.Error(ex, procName);
			}
			finally
			{
				if (Result == ProcessBookResult.None)
					Result = showRetry(LibraryBook);

				Status = Result switch
				{
					ProcessBookResult.Success => ProcessBookStatus.Completed,
					ProcessBookResult.Cancelled => ProcessBookStatus.Cancelled,
					_ => ProcessBookStatus.Failed,
				};
			}

			return Result;
		}

		public async Task Cancel()
		{
			try
			{
				if (CurrentProcessable is AudioDecodable audioDecodable)
				{
					//There's some threadding bug that causes this to hang if executed synchronously.
					await Task.Run(audioDecodable.Cancel);
				}
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

		private string title;
		private string authorNames;
		private string narratorNames;
		private void AudioDecodable_TitleDiscovered(object sender, string title)
		{
			this.title = title;
			updateBookInfo();
		}

		private void AudioDecodable_AuthorsDiscovered(object sender, string authors)
		{
			authorNames = authors;
			updateBookInfo();
		}

		private void AudioDecodable_NarratorsDiscovered(object sender, string narrators)
		{
			narratorNames = narrators;
			updateBookInfo();
		}

		private void updateBookInfo()
		{
			BookText = $"{title}\r\nBy {authorNames}\r\nNarrated by {narratorNames}";
		}

		private byte[] AudioDecodable_RequestCoverArt(object sender, EventArgs e)
		{
			byte[] coverData = PictureStorage
				.GetPictureSynchronously(
				new PictureDefinition(LibraryBook.Book.PictureId, PictureSize._500x500));

			AudioDecodable_CoverImageDiscovered(this, coverData);
			return coverData;
		}

		private void AudioDecodable_CoverImageDiscovered(object sender, byte[] coverArt)
		{
			Cover = Dinah.Core.Drawing.ImageReader.ToImage(coverArt);
		}

		#endregion

		#region Streamable event handlers
		private void Streamable_StreamingTimeRemaining(object sender, TimeSpan timeRemaining)
		{
			TimeRemaining = timeRemaining;
		}

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

		private void Processable_Begin(object sender, LibraryBook libraryBook)
		{
			Status = ProcessBookStatus.Working;

			Logger.Info($"{Environment.NewLine}{((Processable)sender).Name} Step, Begin: {libraryBook.Book}");

			title = libraryBook.Book.Title;
			authorNames = libraryBook.Book.AuthorNames();
			narratorNames = libraryBook.Book.NarratorNames();
			updateBookInfo();
		}

		private async void Processable_Completed(object sender, LibraryBook libraryBook)
		{
			Logger.Info($"{((Processable)sender).Name} Step, Completed: {libraryBook.Book}");
			UnlinkProcessable((Processable)sender);

			if (Processes.Count > 0)
			{
				NextProcessable();
				LinkProcessable(CurrentProcessable);
				var result = await CurrentProcessable.ProcessSingleAsync(libraryBook, validate: true);

				if (result.HasErrors)
				{
					foreach (var errorMessage in result.Errors.Where(e => e != "Validation failed"))
						Logger.Error(errorMessage);

					Completed?.Invoke(this, EventArgs.Empty);
				}
			}
			else
			{
				Completed?.Invoke(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Failure Handler

		private ProcessBookResult showRetry(LibraryBook libraryBook)
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
$@"  Title: {libraryBook.Book.Title}
  ID: {libraryBook.Book.AudibleProductId}
  Author: {trunc(libraryBook.Book.AuthorNames())}
  Narr: {trunc(libraryBook.Book.NarratorNames())}";
			}
			catch
			{
				details = "[Error retrieving details]";
			}

			// if null then ask user
			dialogResult ??= MessageBox.Show(string.Format(SkipDialogText + "\r\n\r\nSee Settings to avoid this box in the future.", details), "Skip importing this book?", SkipDialogButtons, MessageBoxIcon.Question, SkipDialogDefaultButton);

			if (dialogResult == DialogResult.Abort)
				return ProcessBookResult.FailedAbort;

			if (dialogResult == SkipResult)
			{
				libraryBook.Book.UpdateBookStatus(LiberatedStatus.Error);

				Logger.Info($"Error. Skip: [{libraryBook.Book.AudibleProductId}] {libraryBook.Book.Title}");

				return ProcessBookResult.FailedSkip;
			}

			return ProcessBookResult.FailedRetry;
		}


		private string SkipDialogText => @"
An error occurred while trying to process this book.
{0}

- ABORT: Stop processing books.

- RETRY: retry this book later. Just skip it for now. Continue processing books. (Will try this book again later.)

- IGNORE: Permanently ignore this book. Continue processing books. (Will not try this book again later.)
".Trim();
		private MessageBoxButtons SkipDialogButtons => MessageBoxButtons.AbortRetryIgnore;
		private MessageBoxDefaultButton SkipDialogDefaultButton => MessageBoxDefaultButton.Button1;
		private DialogResult SkipResult => DialogResult.Ignore;
	}

	#endregion
}
