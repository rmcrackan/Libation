using ApplicationServices;
using CommandLine;
using DataLayer;
using Dinah.Core;
using FileLiberator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli
{
	public abstract class ProcessableOptionsBase : OptionsBase
	{

		[Value(0, MetaName = "[asins]", HelpText = "Optional product IDs of books to process.")]
		public IEnumerable<string> Asins { get; set; }

		protected static TProcessable CreateProcessable<TProcessable>(EventHandler<LibraryBook> completedAction = null)
			where TProcessable : Processable, new()
		{
			var progressBar = new ConsoleProgressBar(Console.Out);
			var strProc = new TProcessable();

			strProc.Begin += (o, e) => Console.WriteLine($"{typeof(TProcessable).Name} Begin: {e}");

			strProc.Completed += (o, e) =>
			{
				progressBar.Clear();
				Console.WriteLine($"{typeof(TProcessable).Name} Completed: {e}");
			};

			strProc.Completed += (s, e) =>
			{
				try
				{
					completedAction?.Invoke(s, e);
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine("CLI error. See log for more details.");
					Serilog.Log.Logger.Error(ex, "CLI error");
				}
			};

			strProc.StreamingTimeRemaining += (_, e) => progressBar.RemainingTime = e;
			strProc.StreamingProgressChanged += (_, e) => progressBar.Progress = e.ProgressPercentage;

			return strProc;
		}

		protected async Task RunAsync(Processable Processable)
		{
			var libraryBooks = DbContexts.GetLibrary_Flat_NoTracking();

			if (Asins.Any())
			{
				var asinsLower = Asins.Select(a => a.TrimStart('[').TrimEnd(']').ToLower()).ToArray();

				foreach (var lb in libraryBooks.Where(lb => lb.Book.AudibleProductId.ToLower().In(asinsLower)))
					await ProcessOneAsync(Processable, lb, true);
			}
			else
			{
				foreach (var lb in Processable.GetValidLibraryBooks(libraryBooks))
					await ProcessOneAsync(Processable, lb, false);
			}

			var done = "Done. All books have been processed";
			Console.WriteLine(done);
			Serilog.Log.Logger.Information(done);
		}

		private static async Task ProcessOneAsync(Processable Processable, LibraryBook libraryBook, bool validate)
		{
			try
			{
				var statusHandler = await Processable.ProcessSingleAsync(libraryBook, validate);

				if (statusHandler.IsSuccess)
					return;

				foreach (var errorMessage in statusHandler.Errors)
				{
					Console.Error.WriteLine(errorMessage);
					Serilog.Log.Logger.Error(errorMessage);
				}
			}
			catch (Exception ex)
			{
				var msg = "Error processing book. Skipping. This book will be tried again on next attempt. For options of skipping or marking as error, retry with main Libation app.";
				Console.Error.WriteLine(msg + ". See log for more details.");
				Serilog.Log.Logger.Error(ex, $"{msg} {{@DebugInfo}}", new { Book = libraryBook.LogFriendly() });
			}
		}
	}
}
