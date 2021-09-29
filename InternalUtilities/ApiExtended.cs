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

		public Task<List<Item>> GetLibraryValidatedAsync(LibraryOptions.ResponseGroupOptions responseGroups = LibraryOptions.ResponseGroupOptions.ALL_OPTIONS, bool importEpisodes = true)
		{
			// bug on audible's side. the 1st time after a long absence, a query to get library will return without titles or authors. a subsequent identical query will be successful. this is true whether or not tokens are refreshed
			// worse, this 1st dummy call doesn't seem to help:
			//    var page = await api.GetLibraryAsync(new AudibleApi.LibraryOptions { NumberOfResultPerPage = 1, PageNumber = 1, PurchasedAfter = DateTime.Now.AddYears(-20), ResponseGroups = AudibleApi.LibraryOptions.ResponseGroupOptions.ALL_OPTIONS });
			// i don't want to incur the cost of making a full dummy call every time because it fails sometimes
			return policy.ExecuteAsync(() => getItemsAsync(responseGroups, importEpisodes));
		}

		private async Task<List<Item>> getItemsAsync(LibraryOptions.ResponseGroupOptions responseGroups, bool importEpisodes)
		{
			var items = new List<Item>();
#if DEBUG
//// this will not work for multi accounts
//var library_json = "library.json";
//library_json = System.IO.Path.GetFullPath(library_json);
//if (System.IO.File.Exists(library_json))
//{
//    items = AudibleApi.Common.Converter.FromJson<List<Item>>(System.IO.File.ReadAllText(library_json));
//}
#endif
			if (!items.Any())
				items = await Api.GetAllLibraryItemsAsync(responseGroups);

			await manageEpisodesAsync(items, importEpisodes);

#if DEBUG
//System.IO.File.WriteAllText(library_json, AudibleApi.Common.Converter.ToJson(items));
#endif

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
		private async Task manageEpisodesAsync(List<Item> items, bool importEpisodes)
		{
			// add podcasts and episodes to list. If fail, don't let it de-rail the rest of the import
			try
			{
				// get parents
				var parents = items.Where(i => i.IsEpisodes).ToList();
#if DEBUG
//var parentsDebug = parents.Select(i => i.ToJson()).Aggregate((a, b) => $"{a}\r\n\r\n{b}");
//System.IO.File.WriteAllText("parents.json", parentsDebug);
#endif

				if (!parents.Any())
					return;

				Serilog.Log.Logger.Information($"{parents.Count} series of shows/podcasts found");

				// remove episode parents. even if the following stuff fails, these will still be removed from the collection.
				// also must happen before processing children because children abuses this flag
				items.RemoveAll(i => i.IsEpisodes);

				if (importEpisodes)
				{
					// add children
					var children = await getEpisodesAsync(parents);
					Serilog.Log.Logger.Information($"{children.Count} episodes of shows/podcasts found");
					items.AddRange(children);
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error adding podcasts and episodes");
			}
		}

		private async Task<List<Item>> getEpisodesAsync(List<Item> parents)
		{
			var results = new List<Item>();

			foreach (var parent in parents)
			{
				var children = await getEpisodeChildrenAsync(parent);

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
							Sequence = parent.Relationships.Single(r => r.Asin == child.Asin).Sort.ToString(),
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

				results.AddRange(children);
			}

			return results;
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
