//using System;
//using System.IO;
//using System.Net.Http;
//using System.Threading.Tasks;
//using AudibleApi;
//using AudibleApi.Authentication;
//using AudibleApi.Authorization;
//using Dinah.Core.Net.Http;
//using Newtonsoft.Json;

//namespace AudibleApiDomainService
//{
//	public class AudibleApiClient
//    {
//		#region initialize api
//		public const string APP_SETTINGS = "appsettings.json";
//        private Api _api;

//		private static ClientSettings settings;

//		private AudibleApiClient() { }
//        public async static Task<AudibleApiClient> CreateClientAsync()
//		{
//			settings = ClientSettings.FromFile(APP_SETTINGS);

//			restoreLocale();

//			Api api;
//			try
//			{
//				api = await EzApiCreator.GetApiAsync(settings.IdentityFilePath);
//			}
//			catch
//			{
//				var inMemoryIdentity = await loginAsync(email, password);
//				api = await EzApiCreator.GetApiAsync(settings.IdentityFilePath, inMemoryIdentity);
//			}

//			return new AudibleApiClient { _api = api };
//        }

//		private static void restoreLocale()
//		{
//			if (settings.LocaleCountryCode != null)
//				Localization.SetLocale(settings.LocaleCountryCode);
//		}

//		// LOGIN PATTERN
//		// - Start with Authenticate. Submit email + pw
//		// - Each step in the login process will return a LoginResult
//		// - Each result which has required user input has a SubmitAsync method
//		// - The final LoginComplete result returns "Identity" -- in-memory authorization items
//		private static async Task<IIdentity> loginAsync(string email, string pw)
//        {
//			var login = new Authenticate();
//            var loginResult = await login.SubmitCredentialsAsync(email, pw);

//            while (true)
//			{
//				switch (loginResult)
//				{
//					case CredentialsPage credentialsPage:
//						Console.WriteLine("Email:");
//						var emailInput = Console.ReadLine();
//						Console.WriteLine("Password:");
//						var pwInput = Dinah.Core.ConsoleLib.ConsoleExt.ReadPassword();
//						loginResult = await credentialsPage.SubmitAsync(emailInput, pwInput);
//						break;

//					case CaptchaPage captchaResult:
//						var imageBytes = await downloadImageAsync(captchaResult.CaptchaImage);
//						var guess = getUserCaptchaGuess(imageBytes);
//						loginResult = await captchaResult.SubmitAsync(guess);
//						break;

//					case TwoFactorAuthenticationPage _2fa:
//						Console.WriteLine("Two-Step Verification code:");
//						var _2faCode = Console.ReadLine();
//						loginResult = await _2fa.SubmitAsync(_2faCode);
//						break;

//					case LoginComplete final:
//						return final.Identity;

//					default:
//						throw new Exception("Unknown LoginResult");
//				}
//            }
//        }

//		private static async Task<byte[]> downloadImageAsync(Uri imageUri)
//		{
//			using var client = new HttpClient();
//			using var contentStream = await client.GetStreamAsync(imageUri);
//			using var localStream = new MemoryStream();
//			await contentStream.CopyToAsync(localStream);
//			return localStream.ToArray();
//		}

//		private static string getUserCaptchaGuess(byte[] captchaImage)
//        {
//            var tempFileName = Path.Combine(Path.GetTempPath(), "audible_api_captcha_" + Guid.NewGuid() + ".jpg");

//            try
//            {
//                File.WriteAllBytes(tempFileName, captchaImage);

//                var processStartInfo = new System.Diagnostics.ProcessStartInfo
//                {
//                    Verb = string.Empty,
//                    UseShellExecute = true,
//                    CreateNoWindow = true,
//                    FileName = tempFileName
//                };
//                System.Diagnostics.Process.Start(processStartInfo);

//                Console.WriteLine("CAPTCHA answer: ");
//                var guess = Console.ReadLine();
//                return guess;
//            }
//            finally
//            {
//                if (File.Exists(tempFileName))
//                    File.Delete(tempFileName);
//            }
//		}
//		#endregion

//		#region api call examples
//		// Mimi's Adventure (3m)
//		public const string TINY_BOOK_ASIN = "B079DZ8YMP";

//		// Harry Potter 1 (8h 33m)
//		public const string MEDIUM_BOOK_ASIN = "B017V4IM1G";

//		// Sherlock Holmes (62h 52m)
//		public const string HUGE_BOOK_ASIN = "B06WLMWF2S";

//		public Task PrintLibraryAsync() => wrapCallAsync(printLibraryAsync);
//		private async Task printLibraryAsync()
//		{
//			// test ad hoc api calls

//			string url;
//			string allGroups;

//			url
//				= "/1.0/library"
//				+ "?purchaseAfterDate=01/01/1970&page=23";
//			url = "/1.0/library/" +
//				//TINY_BOOK_ASIN
//				MEDIUM_BOOK_ASIN
//				//HUGE_BOOK_ASIN
//				;

//			url += url.Contains("?") ? "&" : "?";

//			//allGroups = "response_groups=badge_types,category_ladders,claim_code_url,contributors,is_downloaded,is_returnable,media,origin_asin,pdf_url,percent_complete,price,product_attrs,product_desc,product_extended_attrs,product_plan_details,product_plans,provided_review,rating,relationships,review_attrs,reviews,sample,series,sku";
//			allGroups = "response_groups=series,category_ladders,contributors";

//			url += allGroups;
//			var responseMsg = await _api.AdHocAuthenticatedGetAsync(url);
//			var jObj = await responseMsg.Content.ReadAsJObjectAsync();
//			var str = jObj.ToString(Formatting.Indented);
//			Console.WriteLine(str);
//		}

//		public Task DownloadBookAsync() => wrapCallAsync(downloadBookAsync);
//		private async Task downloadBookAsync()
//		{
//			using var progressBar = new Dinah.Core.ConsoleLib.ProgressBar();
//			var progress = new Progress<DownloadProgress>();
//			progress.ProgressChanged += (_, e) => progressBar.Report(Math.Round((double)(100 * e.BytesReceived) / e.TotalFileSize.Value) / 100);

//			Console.Write("Download book");
//			var finalFile = await _api.DownloadAaxWorkaroundAsync(TINY_BOOK_ASIN, "downloadExample.xyz", progress);

//			Console.WriteLine(" Done!");
//			Console.WriteLine("final file: " + Path.GetFullPath(finalFile));

//			// benefit of this small delay:
//			// - if you try to delete a file too soon after it's created, the OS isn't done with the creation and you can get an unexpected error
//			// - give progressBar's internal timer time to finish. if timer is disposed before the final message is processed, "100%" will never get a chance to be displayed
//			await Task.Delay(100);

//			File.Delete(finalFile);
//		}

//		private async Task wrapCallAsync(Func<Task> fn)
//		{
//			try
//			{
//				await fn();
//			}
//			catch (AudibleApiException aex)
//			{
//				Console.WriteLine("ERROR:");
//				Console.WriteLine(aex.Message);
//				Console.WriteLine(aex.JsonMessage.ToString());
//				Console.WriteLine(aex.RequestUri.ToString());
//			}
//		}
//		#endregion
//    }
//}