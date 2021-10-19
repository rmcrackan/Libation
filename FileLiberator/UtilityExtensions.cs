using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core;

namespace FileLiberator
{
	public static class UtilityExtensions
	{
		public static (string id, string title, string locale, string account) LogFriendly(this LibraryBook libraryBook)
			=> (
			id: libraryBook.Book.AudibleProductId,
			title: libraryBook.Book.Title,
			locale: libraryBook.Book.Locale,
			account: libraryBook.Account.ToMask()
			);

		public static async Task<AudibleApi.Api> GetApiAsync(this LibraryBook libraryBook)
		{
			var apiExtended = await AudibleUtilities.ApiExtended.CreateAsync(libraryBook.Account, libraryBook.Book.Locale);
			return apiExtended.Api;
		}
	}
}
