using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApiDTOs;

//
// probably not the best place for this
// but good enough for now
//
namespace InternalUtilities
{
	public static class AudibleApiExtensions
	{
		public static async Task<List<Item>> GetAllLibraryItemsAsync(this Api api)
		{
			var allItems = new List<Item>();

			for (var i = 1; ; i++)
			{
				var page = await api.GetLibraryAsync(new LibraryOptions
				{
					NumberOfResultPerPage = 1000,
					PageNumber = i,
					PurchasedAfter = new DateTime(2000, 1, 1),
					ResponseGroups = LibraryOptions.ResponseGroupOptions.ALL_OPTIONS
				});

				string pageStr = null;
				LibraryDtoV10 libResult;
				try
				{
					pageStr = page.ToString();
					// important! use this convert method
					libResult = LibraryDtoV10.FromJson(pageStr);
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Error converting library for importing use. Full library:\r\n" + pageStr);
					throw;
				}

				if (!libResult.Items.Any())
					break;

				allItems.AddRange(libResult.Items);
			}

			return allItems;
		}
	}
}
