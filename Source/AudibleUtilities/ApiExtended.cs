using AudibleApi;
using AudibleApi.Common;
using Dinah.Core;
using LibationFileManager;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AudibleUtilities;

/// <summary>USE THIS from within Libation. It wraps the call with correct JSONPath</summary>
public class ApiExtended
{
	public static Func<Account, ILoginChoiceEager>? LoginChoiceFactory { get; set; }
	public Api Api { get; private set; }

	private const int MaxConcurrency = 10;
	private const int BatchSize = 50;

	private ApiExtended(Api api) => Api = api;

	/// <summary>Get api from existing tokens else login with 'eager' choice. External browser url is provided. Response can be external browser login or continuing with native api callbacks.</summary>
	public static async Task<ApiExtended> CreateAsync(Account account)
	{
		ArgumentValidator.EnsureNotNull(account, nameof(account));
		ArgumentValidator.EnsureNotNull(account.AccountId, nameof(account.AccountId));
		var locale = ArgumentValidator.EnsureNotNull(account.Locale, nameof(account.Locale));

		try
		{
			Serilog.Log.Logger.Information("{@DebugInfo}", new
			{
				AccountMaskedLogEntry = account.MaskedLogEntry
			});

			var api = await EzApiCreator.GetApiAsync(
					locale,
					AudibleApiStorage.AccountsSettingsFile,
					account.GetIdentityTokensJsonPath());
			return new ApiExtended(api);
		}
		catch
		{
			if (LoginChoiceFactory is null)
				throw new InvalidOperationException($"The UI module must first set {nameof(LoginChoiceFactory)} before attempting to create the api");

			Serilog.Log.Logger.Information("{@DebugInfo}", new
			{
				LoginType = nameof(ILoginChoiceEager),
				Account = account.MaskedLogEntry ?? "[null]",
				LocaleName = locale.Name
			});

			var api = await EzApiCreator.GetApiAsync(
				LoginChoiceFactory(account),
				locale,
				AudibleApiStorage.AccountsSettingsFile,
				account.GetIdentityTokensJsonPath());

			return new ApiExtended(api);
		}
	}	

	private static AsyncRetryPolicy policy { get; }
		= Policy.Handle<Exception>()
		// 2 retries == 3 total
		.RetryAsync(2);

	public Task<List<Item>> GetLibraryValidatedAsync(LibraryOptions libraryOptions)
	{
		// bug on audible's side. the 1st time after a long absence, a query to get library will return without titles or authors. a subsequent identical query will be successful. this is true whether or not tokens are refreshed
		// worse, this 1st dummy call doesn't seem to help:
		//    var page = await api.GetLibraryAsync(new AudibleApi.LibraryOptions { NumberOfResultPerPage = 1, PageNumber = 1, PurchasedAfter = DateTime.Now.AddYears(-20), ResponseGroups = AudibleApi.LibraryOptions.ResponseGroupOptions.ALL_OPTIONS });
		// i don't want to incur the cost of making a full dummy call every time because it fails sometimes
		return policy.ExecuteAsync(() => getItemsAsync(libraryOptions));
	}

	/// <summary>
	/// A debugging method used to simulate a library scan from a LibraryScans.zip json file.
	/// Simply replace the Api call to GetLibraryItemsPagesAsync() with a call to this method.
	/// </summary>
	private static async IAsyncEnumerable<Item[]> GetItemsFromJsonFile()
	{
		var libraryScanJsonPath = @"Path/to/libraryscan.json";
		using var jsonFile = System.IO.File.OpenText(libraryScanJsonPath);

		var json = await JToken.ReadFromAsync(new Newtonsoft.Json.JsonTextReader(jsonFile));
		if (json?["Items"] is not JArray items)
			yield break;

		foreach (var batch in items.OfType<JObject>().Select(Item.FromJson).OfType<Item>().Chunk(BatchSize))
			yield return batch;
	}

	private async Task<List<Item>> getItemsAsync(LibraryOptions libraryOptions)
	{
		Serilog.Log.Logger.Debug("Beginning library scan.");

		List<Item> items = new();
		var sw = Stopwatch.StartNew();
		var totalTime = TimeSpan.Zero;
		using var semaphore = new SemaphoreSlim(MaxConcurrency);

		var episodeChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });
		var batchReaderTask = readAllAsinsAsync(episodeChannel.Reader, semaphore);

		//Scan the library for all added books.
		//Get relationship asins from episode-type items and write them to episodeChannel where they will be batched and queried.
		await foreach (var itemsBatch in Api.GetLibraryItemsPagesAsync(libraryOptions, BatchSize, semaphore))
		{
			if (Configuration.Instance.ImportEpisodes)
			{
				var episodes = itemsBatch.Where(i => i.IsEpisodes).ToList();
				var series = itemsBatch.Where(i => i.IsSeriesParent).ToList();

				var parentAsins = episodes
					.SelectMany(i => i.Relationships ?? [])
					.Where(r => r.RelationshipToProduct == RelationshipToProduct.Parent)
					.Select(r => r.Asin)
					.OfType<string>();

				var episodeAsins = series
					.SelectMany(i => i.Relationships ?? [])
					.Where(r => r.RelationshipToProduct == RelationshipToProduct.Child && r.RelationshipType == RelationshipType.Episode)
					.Select(r => r.Asin)
					.OfType<string>();

				foreach (var asin in parentAsins.Concat(episodeAsins))
					episodeChannel.Writer.TryWrite(asin);

				items.AddRange(episodes);
				items.AddRange(series);
			}

			var booksInBatch
				= itemsBatch
				.Where(i => !i.IsSeriesParent && !i.IsEpisodes)
				.Where(i => i.IsAyce is not true || Configuration.Instance.ImportPlusTitles);
			items.AddRange(booksInBatch);
		}

		sw.Stop();
		totalTime += sw.Elapsed;
		Serilog.Log.Logger.Debug("Library scan complete after {elappsed_ms} ms. Found {count} books and series. Waiting on series episode scans to complete.", sw.ElapsedMilliseconds, items.Count);
		sw.Restart();

		//Signal that we're done adding asins
		episodeChannel.Writer.Complete();

		//Wait for all episodes/parents to be retrived
		var allEps = await batchReaderTask;

		sw.Stop();
		totalTime += sw.Elapsed;
		Serilog.Log.Logger.Debug("Episode scan complete after {elappsed_ms} ms. Found {count} episodes and series .", sw.ElapsedMilliseconds, allEps.Count);
		sw.Restart();

		Serilog.Log.Logger.Debug("Begin indexing series episodes");
		items.AddRange(allEps);

		//Set the Item.Series info for episodes and parents.
		foreach (var parent in items.Where(i => i.IsSeriesParent))
		{
			var children = items.Where(i => i.IsEpisodes && i.Relationships?.Any(r => r.Asin == parent.Asin) is true);
			SetSeries(parent, children);
		}

		int orphansRemoved = items.RemoveAll(i => (i.IsEpisodes || i.IsSeriesParent) && i.Series is null);
		if (orphansRemoved > 0)
			Serilog.Log.Debug("{orphansRemoved} podcast orphans not imported", orphansRemoved);

		sw.Stop();
		totalTime += sw.Elapsed;
		Serilog.Log.Logger.Information("Completed indexing series episodes after {elappsed_ms} ms.", sw.ElapsedMilliseconds);
		Serilog.Log.Logger.Information($"Completed library scan in {totalTime.TotalMilliseconds:F0} ms.");

		Array.ForEach(ISanitizer.GetAllSanitizers(), s => s.Sanitize(items));
		var allExceptions = IValidator.GetAllValidators().SelectMany(v => v.Validate(items)).ToList();
		if (allExceptions?.Count > 0)
			throw new ImportValidationException(items, allExceptions);

		return items;
	}

	#region episodes and podcasts

	/// <summary>
	/// Read asins from the channel and request catalog item info in batches of <see cref="BatchSize"/>. Blocks until <paramref name="channelReader"/> is closed.
	/// </summary>
	/// <param name="channelReader">Input asins to batch</param>
	/// <param name="semaphore">Shared semaphore to limit concurrency</param>
	/// <returns>All <see cref="Item"/>s of asins written to the channel.</returns>
	private async Task<List<Item>> readAllAsinsAsync(ChannelReader<string> channelReader, SemaphoreSlim semaphore)
	{
		int batchNum = 1;
		List<Task<List<Item>>> getTasks = new();

		while (await channelReader.WaitToReadAsync())
		{
			List<string> asins = new();

			while (asins.Count < BatchSize && await channelReader.WaitToReadAsync())
			{
				var asin = await channelReader.ReadAsync();

				if (!asins.Contains(asin))
					asins.Add(asin);
			}
			await semaphore.WaitAsync();
			getTasks.Add(getProductsAsync(batchNum++, asins, semaphore));
		}

		var completed = await Task.WhenAll(getTasks);
		//We only want Series parents and Series episodes. Exclude other relationship types (e.g. 'season')
		return completed.SelectMany(l => l).Where(i => i.IsSeriesParent || i.IsEpisodes).ToList();
	}

	private async Task<List<Item>> getProductsAsync(int batchNum, List<string> asins, SemaphoreSlim semaphore)
	{
		Serilog.Log.Logger.Debug($"Batch {batchNum} Begin: Fetching {asins.Count} asins");
		try
		{
			var sw = Stopwatch.StartNew();
			var items = await Api.GetCatalogProductsAsync(asins, CatalogOptions.ResponseGroupOptions.Rating | CatalogOptions.ResponseGroupOptions.Media
				| CatalogOptions.ResponseGroupOptions.Relationships | CatalogOptions.ResponseGroupOptions.ProductDesc
				| CatalogOptions.ResponseGroupOptions.Contributors | CatalogOptions.ResponseGroupOptions.ProvidedReview
				| CatalogOptions.ResponseGroupOptions.ProductPlans | CatalogOptions.ResponseGroupOptions.Series
				| CatalogOptions.ResponseGroupOptions.CategoryLadders | CatalogOptions.ResponseGroupOptions.ProductExtendedAttrs);
			sw.Stop();

			Serilog.Log.Logger.Debug($"Batch {batchNum} End: Retrieved {items.Count} items in {sw.ElapsedMilliseconds} ms");

			return items;
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Error fetching batch of episodes. {@DebugInfo}", new { asins });
			throw;
		}
		finally { semaphore.Release(); }
	}

	public static void SetSeries(Item parent, IEnumerable<Item> children)
	{
		ArgumentValidator.EnsureNotNull(parent, nameof(parent));
		ArgumentValidator.EnsureNotNull(children, nameof(children));

		//A series parent will always have exactly 1 Series
		parent.Series = new[]
		{
			new Series
			{
				Asin = parent.Asin,
				Sequence = "-1",
				Title = parent.TitleWithSubtitle
			}
		};

		if (parent.PurchaseDate == default)
		{
			parent.PurchaseDate = children.Select(c => c.PurchaseDate).Order().FirstOrDefault(d => d != default);

			if (parent.PurchaseDate == default)
			{
				Serilog.Log.Logger.Warning("{series} doesn't have a purchase date. Using UtcNow", parent);
				parent.PurchaseDate = DateTimeOffset.UtcNow;
			}
		}

		int lastEpNum = -1, dupeCount = 0;
		foreach (var child in children.OrderBy(i => i.EpisodeNumber).ThenBy(i => i.PublicationDateTime))
		{
			string sequence;
			if (child.EpisodeNumber is null)
			{
				// This should properly be Single() not FirstOrDefault(), but FirstOrDefault is defensive for malformed data from audible
				sequence = parent.Relationships?.FirstOrDefault(r => r.Asin == child.Asin)?.Sort?.ToString() ?? "0";
			}
			else
			{
				//multipart episodes may have the same episode number
				if (child.EpisodeNumber == lastEpNum)
					dupeCount++;
				else
					lastEpNum = child.EpisodeNumber.Value;

				sequence = (lastEpNum + dupeCount).ToString();
			}

			// use parent's 'DateAdded'. DateAdded is just a convenience prop for: PurchaseDate.UtcDateTime
			child.PurchaseDate = parent.PurchaseDate;
			// parent is essentially a series
			child.Series = new[]
			{
				new Series
				{
					Asin = parent.Asin,
					Sequence = sequence,
					Title = parent.TitleWithSubtitle
				}
			};
		}
	}
	#endregion
}
