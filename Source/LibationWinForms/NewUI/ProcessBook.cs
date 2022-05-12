using DataLayer;
using Dinah.Core;
using FileLiberator;
using LibationFileManager;
using LibationWinForms.BookLiberation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.NewUI
{
	public enum ProcessBookResult
	{
		None,
		Success,
		Cancelled,
		FailedRetry,
		FailedSkip,
		FailedAbort
	}

	internal enum QueuePosition
	{
		Absent,
		Current,
		Fisrt,
		OneUp,
		OneDown,
		Last
	}

	internal delegate QueuePosition ProcessControlReorderHandler(ProcessBook sender, QueuePosition arg);
	internal delegate void ProcessControlEventArgs<T>(ProcessBook sender, T arg);
	internal delegate void ProcessControlEventArgs(ProcessBook sender, EventArgs arg);

	internal class ProcessBook
	{
		public event EventHandler Completed;
		public event ProcessControlEventArgs Cancelled;
		public event ProcessControlReorderHandler RequestMove;
		public GridEntry Entry { get; }
		public ILiberationBaseForm BookControl { get; }

		private Func<Processable> _makeFirstProc;
		private Processable _firstProcessable;
		private bool cancelled = false;
		private bool running = false;
		public Processable FirstProcessable => _firstProcessable ??= _makeFirstProc?.Invoke();
		private readonly Queue<Func<Processable>> Processes = new();

		LogMe Logger;

		public ProcessBook(GridEntry entry, LogMe logme)
		{
			Entry = entry;
			BookControl = new ProcessBookControl(Entry.Title, Entry.Cover);
			BookControl.CancelAction = Cancel;
			BookControl.MoveUpAction = MoveUp;
			BookControl.MoveDownAction = MoveDown;
			Logger = logme;
		}

		public QueuePosition? MoveUp()
		{
			return RequestMove?.Invoke(this, QueuePosition.OneUp);
		}
		public QueuePosition? MoveDown()
		{
			return RequestMove?.Invoke(this, QueuePosition.OneDown);
		}

		public void Cancel()
		{
			cancelled = true;
			try
			{
				if (FirstProcessable is AudioDecodable audioDecodable)
					audioDecodable.Cancel();
			}
			catch(Exception ex)
			{
				Logger.Error(ex, "Error while cancelling");
			}

			if (!running)
				Cancelled?.Invoke(this, EventArgs.Empty);
		}

		public async Task<ProcessBookResult> ProcessOneAsync()
		{
			running = true;
			ProcessBookResult result = ProcessBookResult.None;
			try
			{
				var firstProc = FirstProcessable;

				LinkProcessable(firstProc);

				var statusHandler = await firstProc.ProcessSingleAsync(Entry.LibraryBook, validate: true);


				if (statusHandler.IsSuccess)
					return result = ProcessBookResult.Success;
				else if (cancelled)
				{
					Logger.Info($"Process was cancelled {Entry.LibraryBook.Book}");
					return result = ProcessBookResult.Cancelled;
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
				if (result == ProcessBookResult.None)
					result = showRetry(Entry.LibraryBook);

				BookControl.SetResult(result);
			}

			return result;
		}

		public void AddPdfProcessable() => AddProcessable<DownloadPdf>();
		public void AddDownloadDecryptProcessable() => AddProcessable<DownloadDecryptBook>();
		public void AddConvertMp3Processable() => AddProcessable<ConvertToMp3>();

		private void AddProcessable<T>() where T : Processable, new()
		{
			if (FirstProcessable == null)
			{
				_makeFirstProc = () => new T();
			}
			else
				Processes.Enqueue(() => new T());
		}

		private void LinkProcessable(Processable strProc)
		{
			strProc.Begin += Processable_Begin;
			strProc.Completed += Processable_Completed;
		}

		private void Processable_Begin(object sender, LibraryBook libraryBook)
		{
			BookControl.RegisterFileLiberator((Processable)sender, Logger);
			BookControl.Processable_Begin(sender, libraryBook);
		}

		private async void Processable_Completed(object sender, LibraryBook e)
		{
			((Processable)sender).Begin -= Processable_Begin;

			if (Processes.Count > 0)
			{
				var nextProcessFunc = Processes.Dequeue();
				var nextProcess = nextProcessFunc();
				LinkProcessable(nextProcess);
				var result = await nextProcess.ProcessSingleAsync(e, true);

				if (result.HasErrors)
				{
					foreach (var errorMessage in result.Errors.Where(e => e != "Validation failed"))
						Logger.Error(errorMessage);

					Completed?.Invoke(this, EventArgs.Empty);
					running = false;
				}
			}
			else
			{
				Completed?.Invoke(this, EventArgs.Empty);
				running = false;
			}
		}

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


		protected string SkipDialogText => @"
An error occurred while trying to process this book.
{0}

- ABORT: Stop processing books.

- RETRY: retry this book later. Just skip it for now. Continue processing books. (Will try this book again later.)

- IGNORE: Permanently ignore this book. Continue processing books. (Will not try this book again later.)
".Trim();
		protected MessageBoxButtons SkipDialogButtons => MessageBoxButtons.AbortRetryIgnore;
		protected MessageBoxDefaultButton SkipDialogDefaultButton => MessageBoxDefaultButton.Button1;
		protected DialogResult SkipResult => DialogResult.Ignore;
	}
}
