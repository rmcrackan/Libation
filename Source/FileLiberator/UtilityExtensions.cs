using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core;
using LibationFileManager;

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

		public static LibraryBookDto ToDto(this LibraryBook libraryBook) => new()
		{
			Account = libraryBook.Account,
			DateAdded = libraryBook.DateAdded,

			AudibleProductId = libraryBook.Book.AudibleProductId,
			Title = libraryBook.Book.Title ?? "",
			Locale = libraryBook.Book.Locale,
			YearPublished = libraryBook.Book.DatePublished?.Year,
			DatePublished = libraryBook.Book.DatePublished,

			Authors = libraryBook.Book.Authors.Select(c => c.Name).ToList(),

			Narrators = libraryBook.Book.Narrators.Select(c => c.Name).ToList(),

			SeriesName = libraryBook.Book.SeriesLink.FirstOrDefault()?.Series.Name,
			SeriesNumber = (int?)libraryBook.Book.SeriesLink.FirstOrDefault()?.Index,
			IsPodcast = libraryBook.Book.IsEpisodeChild(),

			BitRate = libraryBook.Book.AudioFormat.Bitrate,
			SampleRate = libraryBook.Book.AudioFormat.SampleRate,
			Channels = libraryBook.Book.AudioFormat.Channels,
			Language = libraryBook.Book.Language
		};
	}
}
