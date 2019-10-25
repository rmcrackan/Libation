using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authentication;
using AudibleApi.Authorization;
using DTOs;

namespace AudibleApiDomainService
{
	public class AudibleApiLibationClient
	{
		private Api _api;

		#region initialize api
		private AudibleApiLibationClient() { }
		public async static Task<AudibleApiLibationClient> CreateClientAsync(Settings settings, IAudibleApiResponder responder)
		{
			Localization.SetLocale(settings.LocaleCountryCode);

			Api api;
			try
			{
				api = await EzApiCreator.GetApiAsync(settings.IdentityFilePath);
			}
			catch
			{
				var inMemoryIdentity = await loginAsync(responder);
				api = await EzApiCreator.GetApiAsync(settings.IdentityFilePath, inMemoryIdentity);
			}

			return new AudibleApiLibationClient { _api = api };
		}

		// LOGIN PATTERN
		// - Start with Authenticate. Submit email + pw
		// - Each step in the login process will return a LoginResult
		// - Each result which has required user input has a SubmitAsync method
		// - The final LoginComplete result returns "Identity" -- in-memory authorization items
		private static async Task<IIdentity> loginAsync(IAudibleApiResponder responder)
		{
			var login = new Authenticate();

			var (email, password) = responder.GetLogin();

			var loginResult = await login.SubmitCredentialsAsync(email, password);

			while (true)
			{
				switch (loginResult)
				{
					case CredentialsPage credentialsPage:
						var (emailInput, pwInput) = responder.GetLogin();
						loginResult = await credentialsPage.SubmitAsync(emailInput, pwInput);
						break;

					case CaptchaPage captchaResult:
						var imageBytes = await downloadImageAsync(captchaResult.CaptchaImage);
						var guess = responder.GetCaptchaAnswer(imageBytes);
						loginResult = await captchaResult.SubmitAsync(guess);
						break;

					case TwoFactorAuthenticationPage _2fa:
						var _2faCode = responder.Get2faCode();
						loginResult = await _2fa.SubmitAsync(_2faCode);
						break;

					case LoginComplete final:
						return final.Identity;

					default:
						throw new Exception("Unknown LoginResult");
				}
			}
		}

		private static async Task<byte[]> downloadImageAsync(Uri imageUri)
		{
			using var client = new HttpClient();
			using var contentStream = await client.GetStreamAsync(imageUri);
			using var localStream = new MemoryStream();
			await contentStream.CopyToAsync(localStream);
			return localStream.ToArray();
		}
		#endregion

		public async Task ImportLibraryAsync()
		{
			try
			{
				var items = await GetLibraryItemsAsync();
//var (total, newEntries) = await ScrapingDomainServices.Indexer.IndexLibraryAsync(items);
			}
			catch (Exception ex)
			{
				// catch here for easier debugging
				throw;
			}
		}

		private async Task<List<Item>> GetLibraryItemsAsync()
		{
			var allItems = new List<Item>();

			for (var i = 1; ; i++)
			{
				var page = await _api.GetLibraryAsync(new LibraryOptions
				{
					NumberOfResultPerPage = 1000,
					PageNumber = i,
					PurchasedAfter = new DateTime(2000, 1, 1),
					ResponseGroups = LibraryOptions.ResponseGroupOptions.ALL_OPTIONS
				});

				// important! use this convert method
				var libResult = LibraryApiV10.FromJson(page.ToString());

				if (!libResult.Items.Any())
					break;

				allItems.AddRange(libResult.Items);
			}

			return allItems;
		}

		//public async Task DownloadBookAsync(string asinToDownload)
		//{
		//	// console example
		//	using var progressBar = new Dinah.Core.ConsoleLib.ProgressBar();
		//	var progress = new Progress<Dinah.Core.Net.Http.DownloadProgress>();
		//	progress.ProgressChanged += (_, e) => progressBar.Report(Math.Round((double)(100 * e.BytesReceived) / e.TotalFileSize.Value) / 100);

		//	logger.WriteLine("Download book");
		//	var finalFile = await _api.DownloadAaxWorkaroundAsync(asinToDownload, "downloadExample.xyz", progress);

		//	logger.WriteLine(" Done!");
		//	logger.WriteLine("final file: " + Path.GetFullPath(finalFile));

		//	// benefit of this small delay:
		//	// - if you try to delete a file too soon after it's created, the OS isn't done with the creation and you can get an unexpected error
		//	// - give progressBar's internal timer time to finish. if timer is disposed before the final message is processed, "100%" will never get a chance to be displayed
		//	await Task.Delay(100);

		//	File.Delete(finalFile);
		//}
	}
}
