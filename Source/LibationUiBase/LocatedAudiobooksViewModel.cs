using ApplicationServices;
using DataLayer;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibationUiBase;

public class FoundAudiobook
{
	public string ID => Entry.Id;
	public string FileName { get; }
	public FilePathCache.CacheEntry Entry { get; }
	public FoundAudiobook(FilePathCache.CacheEntry entry)
	{
		Entry = entry;
		FileName = Path.GetFileName(entry.Path);
	}
}
public class LocatedAudiobooksViewModel : ReactiveObject
{
	public IList<FoundAudiobook> FoundFiles { get; }
	public int FoundAsinCount { get => field; private set => RaiseAndSetIfChanged(ref field, value); }
	private readonly HashSet<string> foundAsinsSet = [];
	public LocatedAudiobooksViewModel(IList<FoundAudiobook> fileList)
	{
		FoundFiles = fileList;
	}

	public void AddFoundFile(FilePathCache.CacheEntry entry)
	{
		FoundAudiobook foundFile = new(entry);
		Invoke(() => FoundFiles?.Add(foundFile));
		if (!foundAsinsSet.Contains(entry.Id))
		{
			foundAsinsSet.Add(entry.Id);
			FoundAsinCount = foundAsinsSet.Count;
		}
	}

	public async Task FindAndAddBooksAsync(string searchdir, CancellationToken cancellation)
	{
		await Task.Run(() => FindAndAddBooksInternal(searchdir, cancellation), cancellation).ConfigureAwait(false);
	}

	private async Task FindAndAddBooksInternal(string searchdir, CancellationToken cancellation)
	{
		await foreach (var book in AudioFileStorage.FindAudiobooksAsync(searchdir, cancellation))
		{
			try
			{
				FilePathCache.Insert(book);

				var lb = DbContexts.GetLibraryBook_Flat_NoTracking(book.Id);
				if (lb is not null && lb.Book?.UserDefinedItem.BookStatus is not LiberatedStatus.Liberated)
					await lb.UpdateBookStatusAsync(LiberatedStatus.Liberated);

				cancellation.ThrowIfCancellationRequested();
				AddFoundFile(book);
			}
			catch (OperationCanceledException) { }
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Error adding found audiobook file to Libation. {@audioFile}", book);
			}
		}
	}
}
