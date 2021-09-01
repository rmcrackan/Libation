using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Common;
using Dinah.Core;
using Polly;
using Polly.Retry;

namespace InternalUtilities
{
	public static class AudibleApiActions
	{
		/// <summary>USE THIS from within Libation. It wraps the call with correct JSONPath</summary>
		public static Task<Api> GetApiAsync(string username, string localeName, ILoginCallback loginCallback = null)
		{
			Serilog.Log.Logger.Information("GetApiAsync. {@DebugInfo}", new
			{
				Username = username.ToMask(),
				LocaleName = localeName,
			});
			return EzApiCreator.GetApiAsync(
				Localization.Get(localeName),
				AudibleApiStorage.AccountsSettingsFile,
				AudibleApiStorage.GetIdentityTokensJsonPath(username, localeName),
				loginCallback);
		}

		/// <summary>USE THIS from within Libation. It wraps the call with correct JSONPath</summary>
		public static Task<Api> GetApiAsync(ILoginCallback loginCallback, Account account)
		{
			Serilog.Log.Logger.Information("GetApiAsync. {@DebugInfo}", new
			{
				Account = account?.MaskedLogEntry ?? "[null]",
				LocaleName = account?.Locale?.Name
			});
			return EzApiCreator.GetApiAsync(
				account.Locale,
				AudibleApiStorage.AccountsSettingsFile,
				account.GetIdentityTokensJsonPath(),
				loginCallback);
		}

		private static AsyncRetryPolicy policy { get; }
			= Policy.Handle<Exception>()
			// 2 retries == 3 total
			.RetryAsync(2);

		public static Task<List<Item>> GetLibraryValidatedAsync(Api api, LibraryOptions.ResponseGroupOptions responseGroups = LibraryOptions.ResponseGroupOptions.ALL_OPTIONS)
		{
			// bug on audible's side. the 1st time after a long absence, a query to get library will return without titles or authors. a subsequent identical query will be successful. this is true whether or tokens are refreshed
			// worse, this 1st dummy call doesn't seem to help:
			//    var page = await api.GetLibraryAsync(new AudibleApi.LibraryOptions { NumberOfResultPerPage = 1, PageNumber = 1, PurchasedAfter = DateTime.Now.AddYears(-20), ResponseGroups = AudibleApi.LibraryOptions.ResponseGroupOptions.ALL_OPTIONS });
			// i don't want to incur the cost of making a full dummy call every time because it fails sometimes
			return policy.ExecuteAsync(() => getItemsAsync(api, responseGroups));
		}

		private static async Task<List<Item>> getItemsAsync(Api api, LibraryOptions.ResponseGroupOptions responseGroups)
		{
			var items = await api.GetAllLibraryItemsAsync(responseGroups);

			await manageEpisodesAsync(api, items);

			var validators = new List<IValidator>();
			validators.AddRange(getValidators());
			foreach (var v in validators)
			{
				var exceptions = v.Validate(items);
				if (exceptions != null && exceptions.Any())
					throw new AggregateException(exceptions);
			}

			return items;
		}

		#region episodes and podcasts
		private static async Task manageEpisodesAsync(Api api, List<Item> items)
		{
			// add podcasts and episodes to list. If fail, don't let it de-rail the rest of the import
			try
			{
				// get parents
				var parents = items.Where(i => i.IsEpisodes).ToList();

				if (!parents.Any())
					return;

				// remove episode parents. even if the following stuff fails, these will still be removed from the collection
				items.RemoveAll(i => i.IsEpisodes);

				// add children
				var children = await getEpisodesAsync(api, parents);
				items.AddRange(children);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error adding podcasts and episodes");
			}
		}

		private static async Task<List<Item>> getEpisodesAsync(Api api, List<Item> parents)
		{
			Serilog.Log.Logger.Information($"{parents.Count} series of episodes/podcasts found");

			var results = new List<Item>();

			foreach (var parent in parents)
			{
				var children = await getEpisodeChildrenAsync(api, parent);

				// use parent's 'DateAdded'. DateAdded is just a convenience prop for: PurchaseDate.UtcDateTime
				foreach (var child in children)
					child.PurchaseDate = parent.PurchaseDate;

				results.AddRange(children);
			}

			return results;
		}

		private static async Task<List<Item>> getEpisodeChildrenAsync(Api api, Item parent)
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
					childrenBatch = await api.GetCatalogProductsAsync(idBatch, CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);
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

				Serilog.Log.Logger.Debug($"Batch {i}: {childrenBatch.Count} results");
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
				Serilog.Log.Logger.Error(ex, "Quantity of series episodes defined by parent does not match quantity returned by batch fetching.");
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
