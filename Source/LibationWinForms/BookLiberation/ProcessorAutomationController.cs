﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataLayer;
using Dinah.Core;
using FileLiberator;
using LibationFileManager;
using LibationWinForms.BookLiberation.BaseForms;

namespace LibationWinForms.BookLiberation
{
	// decouple serilog and form. include convenience factory method
	public class LogMe
	{
		public event EventHandler<string> LogInfo;
		public event EventHandler<string> LogErrorString;
		public event EventHandler<(Exception, string)> LogError;

		private LogMe()
		{
			LogInfo += (_, text) => Serilog.Log.Logger.Information($"Automated backup: {text}");
			LogErrorString += (_, text) => Serilog.Log.Logger.Error(text);
			LogError += (_, tuple) => Serilog.Log.Logger.Error(tuple.Item1, tuple.Item2 ?? "Automated backup: error");
		}

		public static LogMe RegisterForm(AutomatedBackupsForm form = null)
		{
			var logMe = new LogMe();

			if (form is null)
				return logMe;

			logMe.LogInfo += (_, text) => form?.WriteLine(text);

			logMe.LogErrorString += (_, text) => form?.WriteLine(text);

			logMe.LogError += (_, tuple) =>
			{
				form?.WriteLine(tuple.Item2 ?? "Automated backup: error");
				form?.WriteLine("ERROR: " + tuple.Item1.Message);
			};

			return logMe;
		}

		public void Info(string text) => LogInfo?.Invoke(this, text);
		public void Error(string text) => LogErrorString?.Invoke(this, text);
		public void Error(Exception ex, string text = null) => LogError?.Invoke(this, (ex, text));
	}

	public static class ProcessorAutomationController
	{
		public static async Task BackupSingleBookAsync(LibraryBook libraryBook)
		{
			Serilog.Log.Logger.Information($"Begin {nameof(BackupSingleBookAsync)} {{@DebugInfo}}", new { libraryBook?.Book?.AudibleProductId });

			var logMe = LogMe.RegisterForm();
			var backupBook = CreateBackupBook(logMe);

			// continue even if libraryBook is null. we'll display even that in the processing box
			await new BackupSingle(logMe, backupBook, libraryBook).RunBackupAsync();
		}

		public static async Task BackupAllBooksAsync(List<LibraryBook> libraryBooks = null)
		{
			Serilog.Log.Logger.Information("Begin " + nameof(BackupAllBooksAsync));

			var automatedBackupsForm = new AutomatedBackupsForm();
			var logMe = LogMe.RegisterForm(automatedBackupsForm);
			var backupBook = CreateBackupBook(logMe);

			await new BackupLoop(logMe, backupBook, automatedBackupsForm, libraryBooks).RunBackupAsync();
		}

		public static async Task ConvertAllBooksAsync()
		{
			Serilog.Log.Logger.Information("Begin " + nameof(ConvertAllBooksAsync));

			var automatedBackupsForm = new AutomatedBackupsForm();
			var logMe = LogMe.RegisterForm(automatedBackupsForm);

			var convertBook = CreateProcessable<ConvertToMp3, AudioConvertForm>(logMe);

			await new BackupLoop(logMe, convertBook, automatedBackupsForm).RunBackupAsync();
		}

		public static async Task BackupAllPdfsAsync()
		{
			Serilog.Log.Logger.Information("Begin " + nameof(BackupAllPdfsAsync));

			var automatedBackupsForm = new AutomatedBackupsForm();
			var logMe = LogMe.RegisterForm(automatedBackupsForm);

			var downloadPdf = CreateProcessable<DownloadPdf, PdfDownloadForm>(logMe);

			await new BackupLoop(logMe, downloadPdf, automatedBackupsForm).RunBackupAsync();
		}

		private static Processable CreateBackupBook(LogMe logMe)
		{
			var downloadPdf = CreateProcessable<DownloadPdf, PdfDownloadForm>(logMe);

			//Chain pdf download on DownloadDecryptBook.Completed
			async void onDownloadDecryptBookCompleted(object sender, LibraryBook e)
			{
				await downloadPdf.TryProcessAsync(e);
			}

			var downloadDecryptBook = CreateProcessable<DownloadDecryptBook, AudioDecryptForm>(logMe, onDownloadDecryptBookCompleted);
			return downloadDecryptBook;
		}

		public static void DownloadFile(string url, string destination, bool showDownloadCompletedDialog = false)
		{
			Serilog.Log.Logger.Information($"Begin {nameof(DownloadFile)} for {url}");

			void onDownloadFileStreamingCompleted(object sender, string savedFile)
			{
				Serilog.Log.Logger.Information($"Completed {nameof(DownloadFile)} for {url}. Saved to {savedFile}");

				if (showDownloadCompletedDialog)
					MessageBox.Show($"File downloaded to:{Environment.NewLine}{Environment.NewLine}{savedFile}");
			}

			var downloadFile = new DownloadFile();
			var downloadForm = new DownloadForm();
			downloadForm.RegisterFileLiberator(downloadFile);
			downloadFile.StreamingCompleted += onDownloadFileStreamingCompleted;

			async void runDownload() => await downloadFile.PerformDownloadFileAsync(url, destination);
			new Task(runDownload).Start();
		}

		/// <summary>
		/// Create a new <see cref="Processable"/> and links it to a new <see cref="LiberationBaseForm"/>.
		/// </summary>
		/// <typeparam name="TProcessable">The <see cref="Processable"/> derived type to create.</typeparam>
		/// <typeparam name="TForm">The <see cref="LiberationBaseForm"/> derived Form to create on <see cref="Processable.Begin"/>, Show on <see cref="Streamable.StreamingBegin"/>, Close on <see cref="Streamable.StreamingCompleted"/>, and Dispose on <see cref="Processable.Completed"/> </typeparam>
		/// <param name="logMe">The logger</param>
		/// <param name="completedAction">An additional event handler to handle <see cref="Processable.Completed"/></param>
		/// <returns>A new <see cref="Processable"/> of type <typeparamref name="TProcessable"/></returns>
		private static TProcessable CreateProcessable<TProcessable, TForm>(LogMe logMe, EventHandler<LibraryBook> completedAction = null)
			where TForm : LiberationBaseForm, new()
			where TProcessable : Processable, new()
		{
			var strProc = new TProcessable();

			strProc.Begin += (sender, libraryBook) =>
			{
				var processForm = new TForm();
				processForm.RegisterFileLiberator(strProc, logMe);
				processForm.Processable_Begin(sender, libraryBook);
			};

			strProc.Completed += completedAction;

			return strProc;
		}
	}

	internal abstract class BackupRunner
	{
		protected LogMe LogMe { get; }
		protected Processable Processable { get; }
		protected AutomatedBackupsForm AutomatedBackupsForm { get; }

		protected BackupRunner(LogMe logMe, Processable processable, AutomatedBackupsForm automatedBackupsForm = null)
		{
			LogMe = logMe;
			Processable = processable;
			AutomatedBackupsForm = automatedBackupsForm;
		}

		protected abstract Task RunAsync();
		protected abstract string SkipDialogText { get; }
		protected abstract MessageBoxButtons SkipDialogButtons { get; }
		protected abstract MessageBoxDefaultButton SkipDialogDefaultButton { get; }
		protected abstract DialogResult SkipResult { get; }

		public async Task RunBackupAsync()
		{
			AutomatedBackupsForm?.Show();

			try
			{
				await RunAsync();
			}
			catch (Exception ex)
			{
				LogMe.Error(ex);
			}

			AutomatedBackupsForm?.FinalizeUI();
			LogMe.Info("DONE");
		}

		protected async Task<bool> ProcessOneAsync(LibraryBook libraryBook, bool validate)
		{
			try
			{
				var statusHandler = await Processable.ProcessSingleAsync(libraryBook, validate);

				if (statusHandler.IsSuccess)
					return true;

				foreach (var errorMessage in statusHandler.Errors)
					LogMe.Error(errorMessage);
			}
			catch (Exception ex)
			{
				LogMe.Error(ex);
			}

			return showRetry(libraryBook);
		}

		private bool showRetry(LibraryBook libraryBook)
		{
			LogMe.Error("ERROR. All books have not been processed. Most recent book: processing failed");

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
				return false;

			if (dialogResult == SkipResult)
			{
				libraryBook.Book.UserDefinedItem.BookStatus = LiberatedStatus.Error;
                ApplicationServices.LibraryCommands.UpdateUserDefinedItem(libraryBook.Book);

				LogMe.Info($"Error. Skip: [{libraryBook.Book.AudibleProductId}] {libraryBook.Book.Title}");
			}

			return true;
		}
	}

	internal class BackupSingle : BackupRunner
	{
		private LibraryBook _libraryBook { get; }

		protected override string SkipDialogText => @"
An error occurred while trying to process this book. Skip this book permanently?
{0}

- Click YES to skip this book permanently.

- Click NO to skip the book this time only. We'll try again later.
".Trim();
		protected override MessageBoxButtons SkipDialogButtons => MessageBoxButtons.YesNo;
		protected override MessageBoxDefaultButton SkipDialogDefaultButton => MessageBoxDefaultButton.Button2;
		protected override DialogResult SkipResult => DialogResult.Yes;

		public BackupSingle(LogMe logMe, Processable processable, LibraryBook libraryBook)
			: base(logMe, processable)
		{
			_libraryBook = libraryBook;
		}

		protected override async Task RunAsync()
		{
			if (_libraryBook is not null)
				await ProcessOneAsync(_libraryBook, validate: true);
		}
	}

	internal class BackupLoop : BackupRunner
	{
		protected override string SkipDialogText => @"
An error occurred while trying to process this book.
{0}

- ABORT: Stop processing books.

- RETRY: retry this book later. Just skip it for now. Continue processing books. (Will try this book again later.)

- IGNORE: Permanently ignore this book. Continue processing books. (Will not try this book again later.)
".Trim();
		protected override MessageBoxButtons SkipDialogButtons => MessageBoxButtons.AbortRetryIgnore;
		protected override MessageBoxDefaultButton SkipDialogDefaultButton => MessageBoxDefaultButton.Button1;
		protected override DialogResult SkipResult => DialogResult.Ignore;

		private List<LibraryBook> libraryBooks { get; }

		public BackupLoop(LogMe logMe, Processable processable, AutomatedBackupsForm automatedBackupsForm, List<LibraryBook> libraryBooks = null)
			: base(logMe, processable, automatedBackupsForm)
			=> this.libraryBooks = libraryBooks ?? ApplicationServices.DbContexts.GetLibrary_Flat_NoTracking();

		protected override async Task RunAsync()
		{
			// support for 'skip this time only' requires state. iterators provide this state for free. therefore: use foreach/iterator here
			foreach (var libraryBook in Processable.GetValidLibraryBooks(libraryBooks))
			{
				var keepGoing = await ProcessOneAsync(libraryBook, validate: false);
				if (!keepGoing)
					return;

				if (AutomatedBackupsForm.IsDisposed)
					break;

				if (!AutomatedBackupsForm.KeepGoing)
				{
					if (!AutomatedBackupsForm.KeepGoingChecked)
						LogMe.Info("'Keep going' is unchecked");
					return;
				}
			}

			LogMe.Info("Done. All books have been processed");
		}
	}
}
