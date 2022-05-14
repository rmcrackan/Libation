using DataLayer;
using Dinah.Core;
using FileLiberator;
using LibationFileManager;
using LibationWinForms.BookLiberation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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

	public class ProcessBook
	{
		public event EventHandler Completed;
		public event EventHandler DataAvailable;

		public Processable CurrentProcessable => _currentProcessable ??= Processes.Dequeue().Invoke();
		public ProcessBookResult Result { get; private set; } = ProcessBookResult.None;
		public ProcessBookStatus Status { get; private set; } = ProcessBookStatus.Queued;
		public string BookText { get; private set; }
		public Image Cover { get; private set; }
		public int Progress { get; private set; }
		public TimeSpan TimeRemaining { get; private set; }
		public LibraryBook LibraryBook { get; }

		private Processable _currentProcessable;
		private Func<byte[]> GetCoverArtDelegate;
		private readonly Queue<Func<Processable>> Processes = new();
		private readonly LogMe Logger;

		public ProcessBook(LibraryBook libraryBook, Image coverImage, LogMe logme)
		{
			LibraryBook = libraryBook;
			Cover = coverImage;
			Logger = logme;

			title = LibraryBook.Book.Title;
			authorNames = LibraryBook.Book.AuthorNames();
			narratorNames = LibraryBook.Book.NarratorNames();
			BookText = $"{title}\r\nBy {authorNames}\r\nNarrated by {narratorNames}";
		}

		public async Task<ProcessBookResult> ProcessOneAsync()
		{
			try
			{
				LinkProcessable(CurrentProcessable);

				var statusHandler = await CurrentProcessable.ProcessSingleAsync(LibraryBook, validate: true);

				if (statusHandler.IsSuccess)
					return Result = ProcessBookResult.Success;
				else if (statusHandler.Errors.Contains("Cancelled"))
				{
					Logger.Info($"Process was cancelled {LibraryBook.Book}");
					return Result = ProcessBookResult.Cancelled;
				}
				else if (statusHandler.Errors.Contains("Validation failed"))
				{
					Logger.Info($"Validation failed {LibraryBook.Book}");
					return Result = ProcessBookResult.ValidationFail;
				}

				foreach (var errorMessage in statusHandler.Errors)
					Logger.Error(errorMessage);
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}
			finally
			{
				if (Result == ProcessBookResult.None)
					Result = showRetry(LibraryBook);

				Status = Result switch
				{
					ProcessBookResult.Success => ProcessBookStatus.Completed,
					ProcessBookResult.Cancelled => ProcessBookStatus.Cancelled,
					ProcessBookResult.FailedRetry => ProcessBookStatus.Queued,
					_ => ProcessBookStatus.Failed,
				};

				DataAvailable?.Invoke(this, EventArgs.Empty);
			}

			return Result;
		}

		public void Cancel()
		{
			try
			{
				if (CurrentProcessable is AudioDecodable audioDecodable)
				{
					//There's some threadding bug that causes this to hang if executed synchronously.
					Task.Run(audioDecodable.Cancel);
					DataAvailable?.Invoke(this, EventArgs.Empty);
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Error while cancelling");
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

		private void LinkProcessable(Processable strProc)
		{
			strProc.Begin += Processable_Begin;
			strProc.Completed += Processable_Completed;
			strProc.StreamingProgressChanged += Streamable_StreamingProgressChanged;
			strProc.StreamingTimeRemaining += Streamable_StreamingTimeRemaining;

			if (strProc is AudioDecodable audioDecodable)
			{
				audioDecodable.RequestCoverArt += AudioDecodable_RequestCoverArt;
				audioDecodable.TitleDiscovered += AudioDecodable_TitleDiscovered;
				audioDecodable.AuthorsDiscovered += AudioDecodable_AuthorsDiscovered;
				audioDecodable.NarratorsDiscovered += AudioDecodable_NarratorsDiscovered;
				audioDecodable.CoverImageDiscovered += AudioDecodable_CoverImageDiscovered;
			}
		}

		private void UnlinkProcessable(Processable strProc)
		{
			strProc.Begin -= Processable_Begin;
			strProc.Completed -= Processable_Completed;
			strProc.StreamingProgressChanged -= Streamable_StreamingProgressChanged;
			strProc.StreamingTimeRemaining -= Streamable_StreamingTimeRemaining;

			if (strProc is AudioDecodable audioDecodable)
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
			DataAvailable?.Invoke(this, EventArgs.Empty);
		}

		public void AudioDecodable_RequestCoverArt(object sender, Action<byte[]> setCoverArtDelegate)
		{
			byte[] coverData = GetCoverArtDelegate();
			setCoverArtDelegate(coverData);
			AudioDecodable_CoverImageDiscovered(this, coverData);
		}

		private void AudioDecodable_CoverImageDiscovered(object sender, byte[] coverArt)
		{
			Cover = Dinah.Core.Drawing.ImageReader.ToImage(coverArt);
			DataAvailable?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region Streamable event handlers
		private void Streamable_StreamingTimeRemaining(object sender, TimeSpan timeRemaining)
		{
			TimeRemaining = timeRemaining;
			DataAvailable?.Invoke(this, EventArgs.Empty);
		}

		private void Streamable_StreamingProgressChanged(object sender, Dinah.Core.Net.Http.DownloadProgress downloadProgress)
		{
			if (!downloadProgress.ProgressPercentage.HasValue)
				return;

			if (downloadProgress.ProgressPercentage == 0)
				TimeRemaining = TimeSpan.Zero;
			else
				Progress = (int)downloadProgress.ProgressPercentage;

			DataAvailable?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region Processable event handlers

		private void Processable_Begin(object sender, LibraryBook libraryBook)
		{
			Status = ProcessBookStatus.Working;

			Logger.Info($"{Environment.NewLine}{((Processable)sender).Name} Step, Begin: {libraryBook.Book}");

			GetCoverArtDelegate = () => PictureStorage.GetPictureSynchronously(
						new PictureDefinition(
							libraryBook.Book.PictureId,
							PictureSize._500x500));

			title = libraryBook.Book.Title;
			authorNames = libraryBook.Book.AuthorNames();
			narratorNames = libraryBook.Book.NarratorNames();
			Cover = Dinah.Core.Drawing.ImageReader.ToImage(PictureStorage.GetPicture(
						new PictureDefinition(
							libraryBook.Book.PictureId,
							PictureSize._80x80)).bytes);

			updateBookInfo();
		}

		private async void Processable_Completed(object sender, LibraryBook libraryBook)
		{

			Logger.Info($"{((Processable)sender).Name} Step, Completed: {libraryBook.Book}");
			UnlinkProcessable((Processable)sender);

			if (Processes.Count > 0)
			{
				_currentProcessable = null;
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
				libraryBook.Book.UserDefinedItem.BookStatus = LiberatedStatus.Error;
				ApplicationServices.LibraryCommands.UpdateUserDefinedItem(libraryBook.Book);

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
