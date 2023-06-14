using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationServices;
using CommandLine;
using DataLayer;
using FileLiberator;

namespace LibationCli
{
	public abstract class ProcessableOptionsBase : OptionsBase
	{
		protected static TProcessable CreateProcessable<TProcessable>(EventHandler<LibraryBook> completedAction = null)
			where TProcessable : Processable, new()
		{
			var strProc = new TProcessable();

			strProc.Begin += (o, e) => Console.WriteLine($"{typeof(TProcessable).Name} Begin: {e}");
			strProc.Completed += (o, e) => Console.WriteLine($"{typeof(TProcessable).Name} Completed: {e}");

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

			return strProc;
		}

		protected static async Task RunAsync(Processable Processable)
		{
			foreach (var libraryBook in Processable.GetValidLibraryBooks(DbContexts.GetLibrary_Flat_NoTracking()))
				await ProcessOneAsync(Processable, libraryBook, false);

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
