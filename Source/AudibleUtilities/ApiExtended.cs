using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Diagnostics;
using AudibleApi;
using AudibleApi.Common;
using Dinah.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Polly;
using Polly.Retry;

namespace AudibleUtilities
{
	/// <summary>USE THIS from within Libation. It wraps the call with correct JSONPath</summary>
	public class ApiExtended
	{
		public Api Api { get; private set; }

		private const int MaxConcurrency = 10;
		private const int BatchSize = 50;

		private ApiExtended(Api api) => Api = api;

		/// <summary>Get api from existing tokens else login with 'eager' choice. External browser url is provided. Response can be external browser login or continuing with native api callbacks.</summary>
		public static async Task<ApiExtended> CreateAsync(Account account, ILoginChoiceEager loginChoiceEager)
		{
			Serilog.Log.Logger.Information("{@DebugInfo}", new
			{
				LoginType = nameof(ILoginChoiceEager),
				Account = account?.MaskedLogEntry ?? "[null]",
				LocaleName = account?.Locale?.Name
			});

			var api = await EzApiCreator.GetApiAsync(
				loginChoiceEager,
				account.Locale,
				AudibleApiStorage.AccountsSettingsFile,
				account.GetIdentityTokensJsonPath());
			return new ApiExtended(api);
		}

		/// <summary>Get api from existing tokens. Assumes you have valid login tokens. Else exception</summary>
		public static async Task<ApiExtended> CreateAsync(Account account)
		{
			ArgumentValidator.EnsureNotNull(account, nameof(account));
			ArgumentValidator.EnsureNotNull(account.Locale, nameof(account.Locale));

			Serilog.Log.Logger.Information("{@DebugInfo}", new
			{
				AccountMaskedLogEntry = account.MaskedLogEntry
			});

			return await CreateAsync(account.AccountId, account.Locale.Name);
		}

		/// <summary>Get api from existing tokens. Assumes you have valid login tokens. Else exception</summary>
		public static async Task<ApiExtended> CreateAsync(string username, string localeName)
		{
			Serilog.Log.Logger.Information("{@DebugInfo}", new
			{
				Username = username.ToMask(),
				LocaleName = localeName,
			});

			var api = await EzApiCreator.GetApiAsync(
					Localization.Get(localeName),
					AudibleApiStorage.AccountsSettingsFile,
					AudibleApiStorage.GetIdentityTokensJsonPath(username, localeName));
			return new ApiExtended(api);
		}

		private static AsyncRetryPolicy policy { get; }
			= Policy.Handle<Exception>()
			// 2 retries == 3 total
			.RetryAsync(2);

		public Task<List<Item>> GetLibraryValidatedAsync(LibraryOptions libraryOptions, bool importEpisodes = true)
		{
			// bug on audible's side. the 1st time after a long absence, a query to get library will return without titles or authors. a subsequent identical query will be successful. this is true whether or not tokens are refreshed
			// worse, this 1st dummy call doesn't seem to help:
			//    var page = await api.GetLibraryAsync(new AudibleApi.LibraryOptions { NumberOfResultPerPage = 1, PageNumber = 1, PurchasedAfter = DateTime.Now.AddYears(-20), ResponseGroups = AudibleApi.LibraryOptions.ResponseGroupOptions.ALL_OPTIONS });
			// i don't want to incur the cost of making a full dummy call every time because it fails sometimes
			return policy.ExecuteAsync(() => getItemsAsync(libraryOptions, importEpisodes));
		}

		private async Task<List<Item>> getItemsAsync(LibraryOptions libraryOptions, bool importEpisodes)
		{
			Serilog.Log.Logger.Debug("Beginning library scan.");

			int count = 0;
			List<Item> items = new();
			List<Item> seriesItems = new();

			var sw = Stopwatch.StartNew();

			//Scan the library for all added books, and add any episode-type items to seriesItems to be scanned for episodes/parents
			await foreach (var item in Api.GetLibraryItemAsyncEnumerable(libraryOptions, BatchSize, MaxConcurrency))
			{
				if ((item.IsEpisodes || item.IsSeriesParent) && importEpisodes)
					seriesItems.Add(item);
				else if (!item.IsEpisodes && !item.IsSeriesParent)
					items.Add(item);

				count++;
			}

			Serilog.Log.Logger.Debug("Library scan complete. Found {count} books and series. Waiting on series episode scans to complete.", count);
			Serilog.Log.Logger.Debug("Beginning episode scan.");

			count = 0;

			//'get' Tasks are activated when they are written to the channel. To avoid more concurrency than is desired, the
			//channel is bounded with a capacity of 1. Channel write operations are blocked until the current item is read
			var episodeChannel = Channel.CreateBounded<Task<List<Item>>>(new BoundedChannelOptions(1) { SingleReader = true });

			//Start scanning for all episodes. Episode batch 'get' Tasks are written to the channel.
			var scanAllSeriesTask = scanAllSeries(seriesItems, episodeChannel.Writer);

			//Read all episodes from the channel and add them to the import items.
			//This method blocks until episodeChannel.Writer is closed by scanAllSeries()
			await foreach (var ep in getAllEpisodesAsync(episodeChannel.Reader))
			{
				items.AddRange(ep);
				count += ep.Count;
			}

			//Be sure to await the scanAllSeries Task so that any exceptions are thrown
			await scanAllSeriesTask;

			sw.Stop();
			Serilog.Log.Logger.Debug("Episode scan complete. Found {count} episodes and series.", count);
			Serilog.Log.Logger.Debug($"Completed library scan in {sw.Elapsed.TotalMilliseconds:F0} ms.");

#if DEBUG
			//// this will not work for multi accounts
			//var library_json = "library.json";
			//library_json = System.IO.Path.GetFullPath(library_json);
			//if (System.IO.File.Exists(library_json))
			//    items = AudibleApi.Common.Converter.FromJson<List<Item>>(System.IO.File.ReadAllText(library_json));
			//System.IO.File.WriteAllText(library_json, AudibleApi.Common.Converter.ToJson(items));
#endif
			var validators = new List<IValidator>();
			validators.AddRange(getValidators());
			foreach (var v in validators)
			{
				var exceptions = v.Validate(items);
				if (exceptions is not null && exceptions.Any())
					throw new AggregateException(exceptions);
			}
			return items;
		}

		private static List<IValidator> getValidators()
		{
			var type = typeof(IValidator);
			var types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

			return types.Select(t => Activator.CreateInstance(t) as IValidator).ToList();
		}

		#region episodes and podcasts

		/// <summary>
		/// Read get tasks from the <paramref name="channel"/> and await results. This method maintains
		/// a list of up to <see cref="MaxConcurrency"/> get tasks. When any of the get tasks completes,
		/// the Items are yielded, that task is removed from the list, and a new get task is read from
		/// the channel.
		/// </summary>
		private async IAsyncEnumerable<List<Item>> getAllEpisodesAsync(ChannelReader<Task<List<Item>>> channel)
		{
			List<Task<List<Item>>> concurentGets = new();

			for (int i = 0; i < MaxConcurrency && await channel.WaitToReadAsync(); i++)
				concurentGets.Add(await channel.ReadAsync());

			while (concurentGets.Count > 0)
			{
				var completed = await Task.WhenAny(concurentGets);
				concurentGets.Remove(completed);

				if (await channel.WaitToReadAsync())
					concurentGets.Add(await channel.ReadAsync());

				yield return completed.Result;
			}
		}

		/// <summary>
		/// Gets all child episodes and episode parents belonging to <paramref name="seriesItems"/> in batches and
		/// writes the get tasks to <paramref name="channel"/>.
		/// </summary>
		private async Task scanAllSeries(IEnumerable<Item> seriesItems, ChannelWriter<Task<List<Item>>> channel)
		{
			try
			{
				List<Task> episodeScanTasks = new();

				foreach (var item in seriesItems)
				{
					if (item.IsEpisodes)
						await channel.WriteAsync(getEpisodeParentAsync(item));
					else if (item.IsSeriesParent)
						episodeScanTasks.Add(getParentEpisodesAsync(item, channel));
				}

				//episodeScanTasks complete only after all episode batch 'gets' have been written to the channel
				await Task.WhenAll(episodeScanTasks);
			}
			finally { channel.Complete(); }
		}

		private async Task<List<Item>> getEpisodeParentAsync(Item episode)
		{
			//Item is a single episode that was added to the library.
			//Get the episode's parent and add it to the database.

			Serilog.Log.Logger.Debug("Supplied Parent is an episode. Beginning parent scan for {parent}", episode);

			List<Item> children = new() { episode };

			var parentAsins = episode.Relationships
				.Where(r => r.RelationshipToProduct == RelationshipToProduct.Parent)
				.Select(p => p.Asin);

			var seriesParents = await Api.GetCatalogProductsAsync(parentAsins, CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);

			int numSeriesParents = seriesParents.Count(p => p.IsSeriesParent);
			if (numSeriesParents != 1)
			{
				//There should only ever be 1 top-level parent per episode. If not, log
				//so we can figure out what to do about those special cases, and don't
				//import the episode.
				JsonSerializerSettings Settings = new()
				{
					MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
					DateParseHandling = DateParseHandling.None,
					Converters = {
						new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
					}
				};
				Serilog.Log.Logger.Error($"Found {numSeriesParents} parents for {episode.Asin}\r\nEpisode Product:\r\n{JsonConvert.SerializeObject(episode, Formatting.None, Settings)}");
				return new();
			}

			var parent = seriesParents.Single(p => p.IsSeriesParent);
			parent.PurchaseDate = episode.PurchaseDate;

			setSeries(parent, children);
			children.Add(parent);

			Serilog.Log.Logger.Debug("Completed parent scan for {episode}", episode);

			return children;
		}

		/// <summary>
		/// Gets all episodes belonging to <paramref name="parent"/> in batches of <see cref="BatchSize"/> and writes the batch get tasks to <paramref name="channel"/>
		/// This method only completes after all episode batch 'gets' have been written to the channel
		/// </summary>
		private async Task getParentEpisodesAsync(Item parent, ChannelWriter<Task<List<Item>>> channel)
		{
			Serilog.Log.Logger.Debug("Beginning episode scan for {parent}", parent);

			var episodeIds = parent.Relationships
				.Where(r => r.RelationshipToProduct == RelationshipToProduct.Child && r.RelationshipType == RelationshipType.Episode)
				.Select(r => r.Asin);

			for (int batchNum = 0; episodeIds.Any(); batchNum++)
			{
				var batch = episodeIds.Take(BatchSize);

				await channel.WriteAsync(getEpisodeBatchAsync(batchNum, parent, batch));

				episodeIds = episodeIds.Skip(BatchSize);
			}
		}

		private async Task<List<Item>> getEpisodeBatchAsync(int batchNum, Item parent, IEnumerable<string> childrenIds)
		{
			try
			{
				List<Item> episodeBatch = await Api.GetCatalogProductsAsync(childrenIds, CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);

				setSeries(parent, episodeBatch);

				if (batchNum == 0)
					episodeBatch.Add(parent);

				Serilog.Log.Logger.Debug($"Batch {batchNum}: {episodeBatch.Count} results\t({{parent}})", parent);

				return episodeBatch;
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error fetching batch of episodes. {@DebugInfo}", new
				{
					ParentId = parent.Asin,
					ParentTitle = parent.Title,
					BatchNumber = batchNum,
					ChildIdBatch = childrenIds
				});
				throw;
			}
		}

		private static void setSeries(Item parent, IEnumerable<Item> children)
		{
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

			foreach (var child in children)
			{
				// use parent's 'DateAdded'. DateAdded is just a convenience prop for: PurchaseDate.UtcDateTime
				child.PurchaseDate = parent.PurchaseDate;
				// parent is essentially a series
				child.Series = new[]
				{
					new Series
					{
						Asin = parent.Asin,
						// This should properly be Single() not FirstOrDefault(), but FirstOrDefault is defensive for malformed data from audible
						Sequence = parent.Relationships.FirstOrDefault(r => r.Asin == child.Asin)?.Sort?.ToString() ?? "0",
						Title = parent.TitleWithSubtitle
					}
				};
			}
		}
		#endregion
	}
}
