using AudibleApi;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace FileLiberator.Tests;

[TestClass]
public class WidevineRecommendationTests
{
	private const string B08XDZCH78_JSON = """
		{"error_code":"000307","message":"Unable to retrieve asset details from Sable(AssetInfos), for marketplaceId:AF2M0KC94RCEA, asin:B08XDZCH78, acr:null, skuLite:BK_ACX0_239890, version:LATEST, aaaClientId:urn:cdo:AudibleApiExternalRouterService:Prod:Default"}
		""";

	[TestCleanup]
	public void RestoreConfiguration() => Configuration.RestoreSingletonInstance();

	[TestMethod]
	public void IsAdrmLicenseUnavailableError_matches_known_Sable_acr_null_licenserequest()
	{
		var ex = new ApiErrorException(
			"https://api.audible.com/1.0/content/B08XDZCH78/licenserequest",
			JObject.Parse(B08XDZCH78_JSON),
			WidevineRecommendation.SableAssetErrorCode);

		Assert.IsTrue(WidevineRecommendation.IsAdrmLicenseUnavailableError(ex));
	}

	[TestMethod]
	public void IsAdrmLicenseUnavailableError_rejects_000307_without_acr_null()
	{
		var ex = new ApiErrorException(
			"https://api.audible.com/1.0/content/B00/licenserequest",
			new JObject
			{
				{ "error_code", WidevineRecommendation.SableAssetErrorCode },
				{ "message", "No response groups populated." }
			},
			WidevineRecommendation.SableAssetErrorCode);

		Assert.IsFalse(WidevineRecommendation.IsAdrmLicenseUnavailableError(ex));
	}

	[TestMethod]
	public void IsAdrmLicenseUnavailableError_requires_licenserequest_uri()
	{
		var ex = new ApiErrorException(
			"https://api.audible.com/1.0/content/B08XDZCH78/metadata",
			JObject.Parse(B08XDZCH78_JSON),
			WidevineRecommendation.SableAssetErrorCode);

		Assert.IsFalse(WidevineRecommendation.IsAdrmLicenseUnavailableError(ex));
	}

	[TestMethod]
	public void ShouldRecommendWidevine_when_pattern_matches_and_widevine_disabled()
	{
		var ex = new ApiErrorException(
			"https://api.audible.com/1.0/content/B08XDZCH78/licenserequest",
			JObject.Parse(B08XDZCH78_JSON),
			WidevineRecommendation.SableAssetErrorCode);

		var config = Configuration.CreateMockInstance();
		config.UseWidevine = false;

		Assert.IsTrue(WidevineRecommendation.ShouldRecommendWidevine(ex, config));
	}

	[TestMethod]
	public void ShouldRecommendWidevine_false_when_widevine_already_enabled()
	{
		var ex = new ApiErrorException(
			"https://api.audible.com/1.0/content/B08XDZCH78/licenserequest",
			JObject.Parse(B08XDZCH78_JSON),
			WidevineRecommendation.SableAssetErrorCode);

		var config = Configuration.CreateMockInstance();
		config.UseWidevine = true;

		Assert.IsFalse(WidevineRecommendation.ShouldRecommendWidevine(ex, config));
	}
}
