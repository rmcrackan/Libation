using ApplicationServices;
using AudibleApi;
using AudibleApi.Common;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using FileLiberator;
using LibationFileManager;
using LibationUiBase.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable
namespace LibationUiBase.ProcessQueue;

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
public abstract class ProcessBookViewModelBase : ReactiveObject
{
	private readonly LogMe Logger;
	public LibraryBook LibraryBook { get; protected set; }

	private ProcessBookResult _result = ProcessBookResult.None;
	private ProcessBookStatus _status = ProcessBookStatus.Queued;
	private string? _narrator;
	private string? _author;
	private string? _title;
	private int _progress;
	private string? _eta;
	private object? _cover;
	private TimeSpan _timeRemaining;

	#region Properties exposed to the view
	public ProcessBookResult Result { get => _result; set { RaiseAndSetIfChanged(ref _result, value); RaisePropertyChanged(nameof(StatusText)); } }
	public ProcessBookStatus Status { get => _status; set { RaiseAndSetIfChanged(ref _status, value); RaisePropertyChanged(nameof(IsFinished)); RaisePropertyChanged(nameof(IsDownloading)); RaisePropertyChanged(nameof(Queued)); } }
	public string? Narrator { get => _narrator; set => RaiseAndSetIfChanged(ref _narrator, value); }
	public string? Author { get => _author; set => RaiseAndSetIfChanged(ref _author, value); }
	public string? Title { get => _title; set => RaiseAndSetIfChanged(ref _title, value); }
	public int Progress { get => _progress; protected set => RaiseAndSetIfChanged(ref _progress, value); }
	public TimeSpan TimeRemaining { get => _timeRemaining; set { RaiseAndSetIfChanged(ref _timeRemaining, value); ETA = $"ETA: {value:mm\\:ss}"; } }
	public string? ETA { get => _eta; private set => RaiseAndSetIfChanged(ref _eta, value); }
	public object? Cover { get => _cover; protected set => RaiseAndSetIfChanged(ref _cover, value); }
	public bool IsFinished => Status is not ProcessBookStatus.Queued and not ProcessBookStatus.Working;
	public bool IsDownloading => Status is ProcessBookStatus.Working;
	public bool Queued => Status is ProcessBookStatus.Queued;

	public string StatusText => Result switch
	{
		ProcessBookResult.Success => "Finished",
		ProcessBookResult.Cancelled => "Cancelled",
		ProcessBookResult.ValidationFail => "Validation fail",
		ProcessBookResult.FailedRetry => "Error, will retry later",
		ProcessBookResult.FailedSkip => "Error, Skipping",
		ProcessBookResult.FailedAbort => "Error, Abort",
		ProcessBookResult.LicenseDenied => "License Denied",
		ProcessBookResult.LicenseDeniedPossibleOutage => "Possible Service Interruption",
		_ => Status.ToString(),
	};

	#endregion

	protected Processable CurrentProcessable => _currentProcessable ??= Processes.Dequeue().Invoke();
	protected void NextProcessable() => _currentProcessable = null;
	private Processable? _currentProcessable;

	/// <summary> A series of Processable actions to perform on this book </summary>
	protected Queue<Func<Processable>> Processes { get; } = new();

	protected ProcessBookViewModelBase(LibraryBook libraryBook, LogMe logme)
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
		_cover = LoadImageFromBytes(picture, PictureSize._80x80);
	}

	protected abstract object? LoadImageFromBytes(byte[] bytes, PictureSize pictureSize);
	private void PictureStorage_PictureCached(object? sender, PictureCachedEventArgs e)
	{
		if (e.Definition.PictureId == LibraryBook.Book.PictureId)
		{
			Cover = LoadImageFromBytes(e.Picture, PictureSize._80x80);
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
				result = await GetFailureActionAsync(LibraryBook);

			var status = result switch
			{
				ProcessBookResult.Success => ProcessBookStatus.Completed,
				ProcessBookResult.Cancelled => ProcessBookStatus.Cancelled,
				_ => ProcessBookStatus.Failed,
			};

			Status = status;
		}

		Result = result;
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

	public ProcessBookViewModelBase AddDownloadPdf() => AddProcessable<DownloadPdf>();
	public ProcessBookViewModelBase AddDownloadDecryptBook() => AddProcessable<DownloadDecryptBook>();
	public ProcessBookViewModelBase AddConvertToMp3() => AddProcessable<ConvertToMp3>();

	private ProcessBookViewModelBase AddProcessable<T>() where T : Processable, new()
	{
		Processes.Enqueue(() => new T());
		return this;
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

	private void AudioDecodable_TitleDiscovered(object? sender, string title) => Title = title;
	private void AudioDecodable_AuthorsDiscovered(object? sender, string authors) => Author = authors;
	private void AudioDecodable_NarratorsDiscovered(object? sender, string narrators) => Narrator = narrators;
	private void AudioDecodable_CoverImageDiscovered(object? sender, byte[] coverArt)
		=> Cover = LoadImageFromBytes(coverArt, PictureSize._80x80);

	private byte[] AudioDecodable_RequestCoverArt(object? sender, EventArgs e)
	{
		var quality
			= Configuration.Instance.FileDownloadQuality == Configuration.DownloadQuality.High && LibraryBook.Book.PictureLarge is not null
			? new PictureDefinition(LibraryBook.Book.PictureLarge, PictureSize.Native)
			: new PictureDefinition(LibraryBook.Book.PictureId, PictureSize._500x500);

		byte[] coverData = PictureStorage.GetPictureSynchronously(quality);

		AudioDecodable_CoverImageDiscovered(this, coverData);
		return coverData;
	}

	#endregion

	#region Streamable event handlers

	private void Streamable_StreamingTimeRemaining(object? sender, TimeSpan timeRemaining) => TimeRemaining = timeRemaining;
	private void Streamable_StreamingProgressChanged(object? sender, Dinah.Core.Net.Http.DownloadProgress downloadProgress)
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

	private void Processable_Begin(object? sender, LibraryBook libraryBook)
	{
		Status = ProcessBookStatus.Working;

		if (sender is Processable processable)
			Logger.Info($"{Environment.NewLine}{processable.Name} Step, Begin: {libraryBook.Book}");

		Title = libraryBook.Book.TitleWithSubtitle;
		Author = libraryBook.Book.AuthorNames();
		Narrator = libraryBook.Book.NarratorNames();
	}

	private async void Processable_Completed(object? sender, LibraryBook libraryBook)
	{
		if (sender is Processable processable)
		{
			Logger.Info($"{processable.Name} Step, Completed: {libraryBook.Book}");
			UnlinkProcessable(processable);
		}

		if (Processes.Count == 0)
			return;

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
		}
	}

	#endregion

	#region Failure Handler	

	protected async Task<ProcessBookResult> GetFailureActionAsync(LibraryBook libraryBook)
	{
		const DialogResult SkipResult = DialogResult.Ignore;
		Logger.Error($"ERROR. All books have not been processed. Book failed: {libraryBook.Book}");

		DialogResult? dialogResult = Configuration.Instance.BadBook switch
		{
			Configuration.BadBookAction.Abort => DialogResult.Abort,
			Configuration.BadBookAction.Retry => DialogResult.Retry,
			Configuration.BadBookAction.Ignore => DialogResult.Ignore,
			Configuration.BadBookAction.Ask or _ => await ShowRetryDialogAsync(libraryBook)
		};

		if (dialogResult == SkipResult)
		{
			libraryBook.UpdateBookStatus(LiberatedStatus.Error);
			Logger.Info($"Error. Skip: [{libraryBook.Book.AudibleProductId}] {libraryBook.Book.TitleWithSubtitle}");
		}

		return dialogResult is SkipResult ? ProcessBookResult.FailedSkip
			 : dialogResult is DialogResult.Abort ? ProcessBookResult.FailedAbort
			 : ProcessBookResult.FailedRetry;
	}

	protected async Task<DialogResult> ShowRetryDialogAsync(LibraryBook libraryBook)
	{
		string details;
		try
		{
			static string trunc(string str)
				=> string.IsNullOrWhiteSpace(str) ? "[empty]"
				: (str.Length > 50) ? $"{str.Truncate(47)}..."
				: str;

			details = $"""
				  Title: {libraryBook.Book.TitleWithSubtitle}
				  ID: {libraryBook.Book.AudibleProductId}
				  Author: {trunc(libraryBook.Book.AuthorNames())}
				  Narr: {trunc(libraryBook.Book.NarratorNames())}
				""";
		}
		catch
		{
			details = "[Error retrieving details]";
		}

		var skipDialogText = $"""
			An error occurred while trying to process this book.
			{details}
			
			- ABORT: Stop processing books.
			
			- RETRY: Skip this book for now, but retry if it is requeued. Continue processing the queued books.
			
			- IGNORE: Permanently ignore this book. Continue processing the queued books. (Will not try this book again later.)

			See Settings in the Download/Decrypt tab to avoid this box in the future.
			""";

		const MessageBoxButtons SkipDialogButtons = MessageBoxButtons.AbortRetryIgnore;
		const MessageBoxDefaultButton SkipDialogDefaultButton = MessageBoxDefaultButton.Button1;

		try
		{
			return await MessageBoxBase.Show(skipDialogText, "Skip this book?", SkipDialogButtons, MessageBoxIcon.Question, SkipDialogDefaultButton);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Error showing retry dialog. Defaulting to 'Retry'; action.");
			return DialogResult.Retry;
		}
	}

	#endregion
}
