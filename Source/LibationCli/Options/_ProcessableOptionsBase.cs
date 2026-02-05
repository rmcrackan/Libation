using ApplicationServices;
using CommandLine;
using DataLayer;
using FileLiberator;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli
{
	public abstract class ProcessableOptionsBase : OptionsBase
	{

		[Value(0, MetaName = "[asins]", HelpText = "Optional product IDs of books to process.")]
		public IEnumerable<string>? Asins { get; set; }

		protected static TProcessable CreateProcessable<TProcessable>(EventHandler<LibraryBook>? completedAction = null)
			where TProcessable : Processable, IProcessable<TProcessable>
		{
			var strProc = TProcessable.Create(Configuration.Instance);
			LibraryBook? currentLibraryBook = null;

			if (Environment.UserInteractive && !Console.IsOutputRedirected && !Console.IsErrorRedirected) {
				var progressBar = new ConsoleProgressBar(Console.Out);
				strProc.Completed += (_, e) => progressBar.Clear();
				strProc.StreamingTimeRemaining += (_, e) => progressBar.RemainingTime = e;
				strProc.StreamingProgressChanged += (_, e) => progressBar.Progress = e.ProgressPercentage;
			}

			strProc.Begin += (o, e) =>
			{
				currentLibraryBook = e;
				Console.WriteLine($"{typeof(TProcessable).Name} Begin: {e}");
			};

			strProc.Completed += (o, e) =>
			{
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

			if (strProc is AudioDecodable audDec)
			{
				audDec.RequestCoverArt += (_,_) =>
				{
					if (currentLibraryBook is null)
						return null;

					var pictureId = Configuration.Instance.FileDownloadQuality == Configuration.DownloadQuality.High
					? currentLibraryBook.Book.PictureLarge ?? currentLibraryBook.Book.PictureId
					: currentLibraryBook.Book.PictureId;

					return pictureId is null ? null : PictureStorage.GetPictureSynchronously(new PictureDefinition(pictureId, PictureSize.Native));
				};
			}

			return strProc;
		}

		protected async Task RunAsync(Processable Processable, Action<LibraryBook>? config = null)
		{
			if (Asins?.Any() is true)
			{
				foreach (var asin in Asins.Select(a => a.TrimStart('[').TrimEnd(']')))
				{
					if (DbContexts.GetLibraryBook_Flat_NoTracking(asin, caseSensative: false) is LibraryBook lb)
					{
						config?.Invoke(lb);
						await ProcessOneAsync(Processable, lb, true);
					}
					else
					{
						var msg = $"Book with ASIN '{asin}' not found in library. Skipping.";
						Console.Error.WriteLine(msg);
						Serilog.Log.Logger.Error(msg);
					}
				}
			}
			else
			{
				var libraryBooks = DbContexts.GetLibrary_Flat_NoTracking();
				foreach (var lb in Processable.GetValidLibraryBooks(libraryBooks))
				{
					config?.Invoke(lb);
					await ProcessOneAsync(Processable, lb, false);
				}
			}

			var done = "Done. All books have been processed";
			Console.WriteLine(done);
			Serilog.Log.Logger.Information(done);
		}

		protected async Task ProcessOneAsync(Processable Processable, LibraryBook libraryBook, bool validate)
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
