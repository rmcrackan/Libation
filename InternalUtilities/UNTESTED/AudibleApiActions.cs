using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApiDTOs;
using FileManager;
using Polly;
using Polly.Retry;

namespace InternalUtilities
{
	public static class AudibleApiActions
	{
		/// <summary>USE THIS from within Libation. It wraps the call with correct JSONPath</summary>
		public static async Task<Api> GetApiAsync(ILoginCallback loginCallback = null)
		{
			Localization.SetLocale(Configuration.Instance.LocaleCountryCode);

			return await EzApiCreator.GetApiAsync(AudibleApiStorage.AccountsSettingsFile, AudibleApiStorage.GetJsonPath(), loginCallback);
		}

		private static AsyncRetryPolicy policy { get; }
			= Policy.Handle<Exception>()
			// 2 retries == 3 total
			.RetryAsync(2);

		public static Task<List<Item>> GetAllLibraryItemsAsync(ILoginCallback callback)
		{
			// bug on audible's side. the 1st time after a long absence, a query to get library will return without titles or authors. a subsequent identical query will be successful. this is true whether or tokens are refreshed
			// worse, this 1st dummy call doesn't seem to help:
			//    var page = await api.GetLibraryAsync(new AudibleApi.LibraryOptions { NumberOfResultPerPage = 1, PageNumber = 1, PurchasedAfter = DateTime.Now.AddYears(-20), ResponseGroups = AudibleApi.LibraryOptions.ResponseGroupOptions.ALL_OPTIONS });
			// i don't want to incur the cost of making a full dummy call every time because it fails sometimes
			return policy.ExecuteAsync(() => getItemsAsync(callback));
		}
		private static async Task<List<Item>> getItemsAsync(ILoginCallback callback)
		{
			var api = await GetApiAsync(callback);
			var items = await api.GetAllLibraryItemsAsync();

			// remove episode parents
			items.RemoveAll(i => i.IsEpisodes);

			#region // episode handling. doesn't quite work
			//				// add individual/children episodes
			//				var childIds = items
			//					.Where(i => i.Episodes)
			//					.SelectMany(ep => ep.Relationships)
			//					.Where(r => r.RelationshipToProduct == AudibleApiDTOs.RelationshipToProduct.Child && r.RelationshipType == AudibleApiDTOs.RelationshipType.Episode)
			//					.Select(c => c.Asin)
			//					.ToList();
			//				foreach (var childId in childIds)
			//				{
			//					var bookResult = await api.GetLibraryBookAsync(childId, AudibleApi.LibraryOptions.ResponseGroupOptions.ALL_OPTIONS);
			//					var bookItem = AudibleApiDTOs.LibraryDtoV10.FromJson(bookResult.ToString()).Item;
			//					items.Add(bookItem);
			//				}
			#endregion

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
