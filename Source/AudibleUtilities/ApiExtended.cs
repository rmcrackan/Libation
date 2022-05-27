using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Common;
using Dinah.Core;
using Polly;
using Polly.Retry;

namespace AudibleUtilities
{
	/// <summary>USE THIS from within Libation. It wraps the call with correct JSONPath</summary>
	public class ApiExtended
	{
		public Api Api { get; private set; }

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

		/// <summary>Get api from existing tokens else login with native api callbacks.</summary>
		public static async Task<ApiExtended> CreateAsync(Account account, ILoginCallback loginCallback)
		{
			Serilog.Log.Logger.Information("{@DebugInfo}", new
			{
				LoginType = nameof(ILoginCallback),
				Account = account?.MaskedLogEntry ?? "[null]",
				LocaleName = account?.Locale?.Name
			});

			var api = await EzApiCreator.GetApiAsync(
				loginCallback,
				account.Locale,
				AudibleApiStorage.AccountsSettingsFile,
				account.GetIdentityTokensJsonPath());
			return new ApiExtended(api);
		}

		/// <summary>Get api from existing tokens else login with external browser</summary>
		public static async Task<ApiExtended> CreateAsync(Account account, ILoginExternal loginExternal)
		{
			Serilog.Log.Logger.Information("{@DebugInfo}", new
			{
				LoginType = nameof(ILoginExternal),
				Account = account?.MaskedLogEntry ?? "[null]",
				LocaleName = account?.Locale?.Name
			});

			var api = await EzApiCreator.GetApiAsync(
				loginExternal,
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
			var items = new List<Item>();

			Serilog.Log.Logger.Debug("Beginning library scan.");

			List<Task<List<Item>>> getChildEpisodesTasks = new();

			int count = 0, maxConcurrentEpisodeScans = 5;
			using SemaphoreSlim concurrencySemaphore = new(maxConcurrentEpisodeScans);

			await foreach (var item in Api.GetLibraryItemAsyncEnumerable(libraryOptions))
			{
				if (item.IsEpisodes && importEpisodes)
				{
					//Helps to distinguish product parrents which have no content
					//from children which do have content.
					item.Asin = $"SERIES_{item.Asin}";
					//Add the parent to the library because it contains the series
					//description, series rating, and series cover art which differ
					//from the individual episodes' values.
					item.Series = new Series[] { new Series { Asin = item.Asin, Sequence = RelationshipToProduct.Parent, Title = item.TitleWithSubtitle } };

					//Get child episodes asynchronously and await all at the end
					getChildEpisodesTasks.Add(getChildEpisodesAsync(concurrencySemaphore, item));

					items.Add(item);
				}
				else if (!item.IsEpisodes)
					items.Add(item);

				count++;
			}

			Serilog.Log.Logger.Debug("Library scan complete. Found {count} books and series. Waiting on {getChildEpisodesTasksCount} series episode scans to complete.", count, getChildEpisodesTasks.Count);

			//await and add all episides from all parents
			foreach (var epList in await Task.WhenAll(getChildEpisodesTasks))
				items.AddRange(epList);

			Serilog.Log.Logger.Debug("Completed library scan.");

#if DEBUG
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

		#region episodes and podcasts
		
		private async Task<List<Item>> getChildEpisodesAsync(SemaphoreSlim concurrencySemaphore, Item parent)
		{
			await concurrencySemaphore.WaitAsync();

			try
			{
				Serilog.Log.Logger.Debug("Beginning episode scan for {parent}", parent);

				var children = await getEpisodeChildrenAsync(parent);

				if (!children.Any())
				{
					//The parent is the only episode in the podcase series,
					//so the parent is its own child.
					var parentJson = parent.ToJson(parent).ToString();
					var child = Item.FromJson(parentJson);
					child.Asin = child.Asin.Replace("SERIES_", "");
					children.Add(child);
				}

				foreach (var child in children)
				{
					// use parent's 'DateAdded'. DateAdded is just a convenience prop for: PurchaseDate.UtcDateTime
					child.PurchaseDate = parent.PurchaseDate;
					// parent is essentially a series
					child.Series = new Series[]
					{
						new Series
						{
							Asin = parent.Asin,
							// This should properly be Single() not FirstOrDefault(), but FirstOrDefault is defensive for malformed data from audible
							Sequence = parent.Relationships.FirstOrDefault(r => r.Asin == child.Asin)?.Sort?.ToString() ?? "0",
							Title = parent.TitleWithSubtitle
						}
					};
					// overload (read: abuse) IsEpisodes flag
					child.Relationships = new Relationship[]
					{
						new Relationship
						{
							RelationshipToProduct = RelationshipToProduct.Child,
							RelationshipType = RelationshipType.Episode
						}
					};
				}

				Serilog.Log.Logger.Debug("Completed episode scan for {parent}", parent);

				return children;
			}
			finally
			{
				concurrencySemaphore.Release();
			}
		}

		private async Task<List<Item>> getEpisodeChildrenAsync(Item parent)
		{
			var childrenIds = parent.Relationships
				.Where(r => r.RelationshipToProduct == RelationshipToProduct.Child && r.RelationshipType == RelationshipType.Episode)
				.Select(r => r.Asin)
				.ToList();

			// fetch children in batches
			const int batchSize = 20;

			var results = new List<Item>();

			for (var i = 1; ; i++)
			{
				var idBatch = childrenIds.Skip((i - 1) * batchSize).Take(batchSize).ToList();
				if (!idBatch.Any())
					break;

				List<Item> childrenBatch;
				try
				{
					childrenBatch = await Api.GetCatalogProductsAsync(idBatch, CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);
#if DEBUG
//var childrenBatchDebug = childrenBatch.Select(i => i.ToJson()).Aggregate((a, b) => $"{a}\r\n\r\n{b}");
//System.IO.File.WriteAllText($"children of {parent.Asin}.json", childrenBatchDebug);
#endif
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Error fetching batch of episodes. {@DebugInfo}", new
					{
						ParentId = parent.Asin,
						ParentTitle = parent.Title,
						BatchNumber = i,
						ChildIdBatch = idBatch
					});
					throw;
				}

				Serilog.Log.Logger.Debug($"Batch {i}: {childrenBatch.Count} results\t({{parent}})", parent);
				// the service returned no results. probably indicates an error. stop running batches
				if (!childrenBatch.Any())
					break;

				results.AddRange(childrenBatch);
			}

			Serilog.Log.Logger.Debug("Parent episodes/podcasts series. Children found. {@DebugInfo}", new
			{
				ParentId = parent.Asin,
				ParentTitle = parent.Title,
				ChildCount = childrenIds.Count
			});

			if (childrenIds.Count != results.Count)
			{
				var ex = new ApplicationException($"Mis-match: Children defined by parent={childrenIds.Count}. Children returned by batches={results.Count}");
				Serilog.Log.Logger.Error(ex, "{parent} - Quantity of series episodes defined by parent does not match quantity returned by batch fetching.", parent);
				throw ex;
			}

			return results;
		}
		#endregion

		private static List<IValidator> getValidators()
		{
			var type = typeof(IValidator);
			var types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

			return types.Select(t => Activator.CreateInstance(t) as IValidator).ToList();
		}
	}
}
