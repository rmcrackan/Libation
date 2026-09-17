using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace FileLiberator.Tests;

/// <summary>
/// Recognising the placeholder product Audible returns for a title its storefront no longer lists, so a
/// re-download of that title does not replace a real metadata file with the placeholder. Reported in
/// issue #1947.
/// </summary>
[TestClass]
public class CatalogMetadataTests
{
	[TestMethod]
	public void Podcast_content_reference_with_missing_required_fields_can_be_exported()
	{
		var reference = new AudibleApi.Common.ContentReference
		{
			Asin = "B0981FC27J", Version = "1", Acr = "CR!ACR", Codec = null!,
			ContentFormat = null!, Marketplace = "AF2M0KC94RCEA", Sku = "BK_TEST_000001", Tempo = null!
		};

		// Reproduce the exception from the reported podcast download before exercising the export.
		Assert.ThrowsExactly<Newtonsoft.Json.JsonSerializationException>(() => JObject.FromObject(reference));
		var exported = DownloadDecryptBook.SerializeContentReference(reference);

		Assert.AreEqual("B0981FC27J", exported.Value<string>("asin"));
		Assert.AreEqual("1", exported.Value<string>("version"));
		Assert.IsNull(exported.Property("codec"));
	}

	[TestMethod]
	public void Complete_content_reference_keeps_its_existing_json_representation()
	{
		var reference = new AudibleApi.Common.ContentReference
		{
			Acr = "CR!ACR", Asin = "B0981FC27J", Codec = "mp4a", ContentFormat = "MPEG4_44_128",
			Marketplace = "AF2M0KC94RCEA", Sku = "BK_TEST_000001", Tempo = "1.0", Version = "1"
		};

		Assert.IsTrue(JToken.DeepEquals(JObject.FromObject(reference), DownloadDecryptBook.SerializeContentReference(reference)));
	}

	/// <summary>
	/// Verbatim from <c>api.audible.com/1.0/catalog/products/?asins=B089T8FSK6&amp;response_groups=&lt;all&gt;</c>,
	/// the response behind the empty metadata file in the report. The title itself is real and downloadable;
	/// it is only listed in the Canadian storefront, so every other one answers with this.
	/// </summary>
	private const string DelistedProduct = """
		{
			"asin": "B089T8FSK6",
			"asset_details": [],
			"is_preview_enabled": false,
			"is_vvab": false,
			"rating": {
				"num_reviews": 0,
				"overall_distribution": { "average_rating": 0.0, "display_stars": 0.0, "num_ratings": 0 },
				"performance_distribution": { "average_rating": 0.0, "display_stars": 0.0, "num_ratings": 0 },
				"story_distribution": { "average_rating": 0.0, "display_stars": 0.0, "num_ratings": 0 }
			}
		}
		""";

	/// <summary>The same asin from the one storefront that still lists it, trimmed to the fields that matter here.</summary>
	private const string ListedProduct = """
		{
			"asin": "B089T8FSK6",
			"asset_details": [],
			"authors": [ { "asin": "B002BMEJ9I", "name": "David D. Friedman" } ],
			"content_type": "Product",
			"is_vvab": false,
			"publisher_name": "David Friedman",
			"release_date": "2020-06-10",
			"runtime_length_min": 610,
			"subtitle": "Technology and Freedom in an Uncertain World",
			"title": "Future Imperfect"
		}
		""";

	[TestMethod]
	public void A_delisted_titles_placeholder_product_is_empty()
		=> Assert.IsTrue(DownloadDecryptBook.CatalogProductIsEmpty(JObject.Parse(DelistedProduct)));

	[TestMethod]
	public void A_product_the_storefront_still_lists_is_not_empty()
		=> Assert.IsFalse(DownloadDecryptBook.CatalogProductIsEmpty(JObject.Parse(ListedProduct)));

	[TestMethod]
	public void A_product_with_nothing_at_all_is_empty()
		=> Assert.IsTrue(DownloadDecryptBook.CatalogProductIsEmpty(JObject.Parse("""{ "asin": "B089T8FSK6" }""")));

	[TestMethod]
	public void A_title_of_whitespace_counts_as_no_title()
		=> Assert.IsTrue(DownloadDecryptBook.CatalogProductIsEmpty(JObject.Parse("""{ "asin": "B089T8FSK6", "title": "   " }""")));

	/// <summary>
	/// A product carrying nothing but its title is thin, but it is data Audible sent and the user asked for.
	/// The guard only rejects the placeholder.
	/// </summary>
	[TestMethod]
	public void A_product_with_only_a_title_is_kept()
		=> Assert.IsFalse(DownloadDecryptBook.CatalogProductIsEmpty(JObject.Parse("""{ "asin": "B089T8FSK6", "title": "Future Imperfect" }""")));
}
