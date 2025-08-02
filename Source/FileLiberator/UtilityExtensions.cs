using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudibleUtilities;
using DataLayer;
using Dinah.Core;
using LibationFileManager;
using LibationFileManager.Templates;

#nullable enable
namespace FileLiberator
{
	public static class UtilityExtensions
	{
		public static (string id, string title, string locale, string account) LogFriendly(this LibraryBook libraryBook)
			=> (
			id: libraryBook.Book.AudibleProductId,
			title: libraryBook.Book.TitleWithSubtitle,
			locale: libraryBook.Book.Locale,
			account: libraryBook.Account.ToMask()
			);

		public static Func<Account, Task<ApiExtended>>? ApiExtendedFunc { get; set; }

		public static async Task<AudibleApi.Api> GetApiAsync(this LibraryBook libraryBook)
		{
			Account account;
			using (var accounts = AudibleApiStorage.GetAccountsSettingsPersister())
				account = accounts.AccountsSettings.GetAccount(libraryBook.Account, libraryBook.Book.Locale);

			var apiExtended = await ApiExtended.CreateAsync(account);
			return apiExtended.Api;
		}

		public static LibraryBookDto ToDto(this LibraryBook libraryBook)
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var nickname
				= persister.AccountsSettings.Accounts
				.FirstOrDefault(a => a.AccountId == libraryBook.Account && a.Locale.Name == libraryBook.Book.Locale)
				?.AccountName;

			return new()
			{
				Account = libraryBook.Account,
				AccountNickname = nickname,
				DateAdded = libraryBook.DateAdded,

				AudibleProductId = libraryBook.Book.AudibleProductId,
				Title = libraryBook.Book.Title,
				Subtitle = libraryBook.Book.Subtitle,
				TitleWithSubtitle = libraryBook.Book.TitleWithSubtitle,
				Locale = libraryBook.Book.Locale,
				YearPublished = libraryBook.Book.DatePublished?.Year,
				DatePublished = libraryBook.Book.DatePublished,

				Authors = libraryBook.Book.Authors.Select(c => new ContributorDto(c.Name, c.AudibleContributorId)).ToList(),
				Narrators = libraryBook.Book.Narrators.Select(c => new ContributorDto(c.Name, c.AudibleContributorId)).ToList(),

				Series = getSeries(libraryBook.Book.SeriesLink),
				IsPodcastParent = libraryBook.Book.IsEpisodeParent(),
				IsPodcast = libraryBook.Book.IsEpisodeChild() || libraryBook.Book.IsEpisodeParent(),

				Language = libraryBook.Book.Language,
				Codec = libraryBook.Book.UserDefinedItem.LastDownloadedFormat?.CodecString,
				BitRate = libraryBook.Book.UserDefinedItem.LastDownloadedFormat?.BitRate,
				SampleRate = libraryBook.Book.UserDefinedItem.LastDownloadedFormat?.SampleRate,
				Channels = libraryBook.Book.UserDefinedItem.LastDownloadedFormat?.ChannelCount,
				LibationVersion = libraryBook.Book.UserDefinedItem.LastDownloadedVersion?.ToVersionString(),
				FileVersion = libraryBook.Book.UserDefinedItem.LastDownloadedFileVersion
			};
		}

		private static List<SeriesDto>? getSeries(IEnumerable<SeriesBook> seriesBooks)
		{
			if (!seriesBooks.Any())
				return null;

			//I don't remember why or if there was a good reason not to have series numbers for
			//podcast parents, but preserving the behavior for backwards compatibility.
			return seriesBooks
				.Select(sb
					=> new SeriesDto(
						sb.Series.Name,
						sb.Book.IsEpisodeParent() ? null : sb.Index,
						sb.Series.AudibleSeriesId)
				).ToList();
		}
	}
}
