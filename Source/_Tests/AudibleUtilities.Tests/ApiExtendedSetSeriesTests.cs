using AudibleApi.Common;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace ApiExtendedSetSeriesTests;

/// <summary>
/// Podcast series numbers come from Audible's episode_number, then relationship sort/sequence.
/// A missing episode_number is sometimes sent as a huge sentinel integer (issue #2024), which
/// must not be stored as the series order.
/// </summary>
[TestClass]
public class SetSeries
{
	private static Relationship childRel(string asin, long? sort = null, string? sequence = null)
		=> new()
		{
			Asin = asin,
			RelationshipToProduct = RelationshipToProduct.Child,
			RelationshipType = RelationshipType.Episode,
			Sort = sort,
			Sequence = sequence
		};

	private static Relationship parentRel(string asin, long? sort = null, string? sequence = null)
		=> new()
		{
			Asin = asin,
			RelationshipToProduct = RelationshipToProduct.Parent,
			RelationshipType = RelationshipType.Episode,
			Sort = sort,
			Sequence = sequence
		};

	private static Item show(string asin, params Relationship[] childRels)
		=> new()
		{
			Asin = asin,
			Title = "My Show",
			PurchaseDate = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
			Relationships = childRels
		};

	private static Item episode(string asin, string parentAsin, int? episodeNumber, long? sort = null, string? sequence = null, string? catalogSequence = null)
		=> new()
		{
			Asin = asin,
			Title = $"Episode {asin}",
			EpisodeNumber = episodeNumber,
			Relationships = [parentRel(parentAsin, sort, sequence)],
			Series = catalogSequence is null ? null : [new Series { Asin = parentAsin, Sequence = catalogSequence, Title = "My Show" }]
		};

	private static string SequenceOf(Item item) => item.Series!.Single().Sequence!;

	[TestMethod]
	public void a_real_episode_number_is_the_series_order()
	{
		var parent = show("SHOW", childRel("EP", sort: 99));
		var child = episode("EP", "SHOW", episodeNumber: 406, sort: 99);

		ApiExtended.SetSeries(parent, [child]);

		Assert.AreEqual("406", SequenceOf(child));
	}

	[TestMethod]
	public void integer_max_value_episode_number_falls_back_to_parent_relationship_sort()
	{
		var parent = show("SHOW", childRel("EP", sort: 406));
		var child = episode("EP", "SHOW", episodeNumber: int.MaxValue);

		ApiExtended.SetSeries(parent, [child]);

		Assert.AreEqual("406", SequenceOf(child));
	}

	[TestMethod]
	public void integer_max_value_episode_number_falls_back_to_child_relationship_sort()
	{
		var parent = show("SHOW", childRel("EP"));
		var child = episode("EP", "SHOW", episodeNumber: int.MaxValue, sort: 406);

		ApiExtended.SetSeries(parent, [child]);

		Assert.AreEqual("406", SequenceOf(child));
	}

	[TestMethod]
	public void integer_max_value_episode_number_falls_back_to_relationship_sequence()
	{
		var parent = show("SHOW", childRel("EP", sort: int.MaxValue, sequence: "406"));
		var child = episode("EP", "SHOW", episodeNumber: int.MaxValue);

		ApiExtended.SetSeries(parent, [child]);

		Assert.AreEqual("406", SequenceOf(child));
	}

	[TestMethod]
	public void integer_max_value_episode_number_falls_back_to_catalog_series_sequence()
	{
		var parent = show("SHOW", childRel("EP", sort: int.MaxValue));
		var child = episode("EP", "SHOW", episodeNumber: int.MaxValue, sort: int.MaxValue, catalogSequence: "406");

		ApiExtended.SetSeries(parent, [child]);

		Assert.AreEqual("406", SequenceOf(child));
	}

	[TestMethod]
	public void integer_max_value_with_no_fallback_is_zero_not_the_sentinel()
	{
		var parent = show("SHOW", childRel("EP"));
		var child = episode("EP", "SHOW", episodeNumber: int.MaxValue);

		ApiExtended.SetSeries(parent, [child]);

		Assert.AreEqual("0", SequenceOf(child));
	}

	[TestMethod]
	public void null_episode_number_still_uses_parent_sort()
	{
		var parent = show("SHOW", childRel("EP", sort: 7));
		var child = episode("EP", "SHOW", episodeNumber: null);

		ApiExtended.SetSeries(parent, [child]);

		Assert.AreEqual("7", SequenceOf(child));
	}

	[TestMethod]
	public void a_real_episode_number_wins_over_a_different_sort()
	{
		var parent = show("SHOW", childRel("EP", sort: 1));
		var child = episode("EP", "SHOW", episodeNumber: 5, sort: 1);

		ApiExtended.SetSeries(parent, [child]);

		Assert.AreEqual("5", SequenceOf(child));
	}

	[TestMethod]
	public void multipart_episodes_with_the_same_number_keep_an_offset()
	{
		var parent = show("SHOW", childRel("A", sort: 3), childRel("B", sort: 3));
		var a = episode("A", "SHOW", episodeNumber: 3);
		var b = episode("B", "SHOW", episodeNumber: 3);

		ApiExtended.SetSeries(parent, [a, b]);

		CollectionAssert.AreEquivalent(new[] { "3", "4" }, new[] { SequenceOf(a), SequenceOf(b) });
	}

	[TestMethod]
	public void a_yyyymmdd_episode_number_is_kept()
	{
		var parent = show("SHOW", childRel("EP", sort: 1));
		var child = episode("EP", "SHOW", episodeNumber: 20260903, sort: 1);

		ApiExtended.SetSeries(parent, [child]);

		Assert.AreEqual("20260903", SequenceOf(child));
	}

	[TestMethod]
	public void a_unix_timestamp_sort_is_not_used_as_the_series_order()
	{
		var parent = show("SHOW", childRel("EP", sort: 1_725_400_800));
		var child = episode("EP", "SHOW", episodeNumber: null, sequence: "406");

		ApiExtended.SetSeries(parent, [child]);

		Assert.AreEqual("406", SequenceOf(child));
	}

	[TestMethod]
	public void a_nine_digit_episode_number_is_kept()
	{
		var parent = show("SHOW", childRel("EP"));
		var child = episode("EP", "SHOW", episodeNumber: 999_999_999);

		ApiExtended.SetSeries(parent, [child]);

		Assert.AreEqual("999999999", SequenceOf(child));
	}

	[TestMethod]
	public void a_ten_digit_episode_number_falls_back()
	{
		var parent = show("SHOW", childRel("EP", sort: 406));
		var child = episode("EP", "SHOW", episodeNumber: 1_000_000_000);

		ApiExtended.SetSeries(parent, [child]);

		Assert.AreEqual("406", SequenceOf(child));
	}
}
