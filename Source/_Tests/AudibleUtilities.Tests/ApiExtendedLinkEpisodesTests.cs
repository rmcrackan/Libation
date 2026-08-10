using AudibleApi.Common;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiExtendedLinkEpisodesTests;

/// <summary>
/// Episodes that can't be linked to a series parent are dropped from the scan. That is what makes a
/// podcast episode disappear from the library with nothing in the log (issue #1925), so the drop has
/// to be both correct and reported.
/// </summary>
[TestClass]
public class LinkEpisodesToSeries
{
	private static Relationship rel(string asin, string toProduct, string type)
		=> new() { Asin = asin, RelationshipToProduct = toProduct, RelationshipType = type };

	/// <summary>A podcast show: has episode children, no season parent.</summary>
	private static Item show(string asin, string title, params string[] episodeAsins)
		=> new()
		{
			Asin = asin,
			Title = title,
			PurchaseDate = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
			Relationships = episodeAsins.Select(e => rel(e, RelationshipToProduct.Child, RelationshipType.Episode)).ToArray()
		};

	/// <summary>A podcast episode pointing at its parent.</summary>
	private static Item episode(string asin, string title, string parentAsin, long sort)
		=> new()
		{
			Asin = asin,
			Title = title,
			Relationships = [new Relationship
			{
				Asin = parentAsin,
				RelationshipToProduct = RelationshipToProduct.Parent,
				RelationshipType = RelationshipType.Episode,
				Sort = sort
			}]
		};

	/// <summary>A season container: episode children, but a season parent of its own.</summary>
	private static Item season(string asin, string showAsin, params string[] episodeAsins)
		=> new()
		{
			Asin = asin,
			Title = $"Season {asin}",
			Relationships =
			[
				.. episodeAsins.Select(e => rel(e, RelationshipToProduct.Child, RelationshipType.Episode)),
				rel(showAsin, RelationshipToProduct.Parent, RelationshipType.Season)
			]
		};

	private static Item book(string asin) => new() { Asin = asin, Title = "A regular audiobook" };

	[TestMethod]
	public void episodes_are_linked_to_their_show_and_nothing_is_dropped()
	{
		var items = new List<Item>
		{
			show("SHOW", "My Show", "EP1", "EP2"),
			episode("EP1", "Episode one", "SHOW", 1),
			episode("EP2", "Episode two", "SHOW", 2)
		};

		var unlinked = ApiExtended.LinkEpisodesToSeries(items);

		Assert.AreEqual(0, unlinked.Count);
		Assert.AreEqual(3, items.Count);
		Assert.IsTrue(items.All(i => i.Series is not null));
		CollectionAssert.AreEquivalent(
			new[] { "SHOW", "SHOW", "SHOW" },
			items.Select(i => i.Series!.Single().Asin).ToList());
	}

	[TestMethod]
	public void episode_whose_show_is_absent_is_dropped_and_returned()
	{
		// Audible omitted SHOW from the catalog response, so EP2's parent never made it into the scan.
		var items = new List<Item>
		{
			episode("EP2", "Episode two", "SHOW", 2),
			book("BOOK")
		};

		var unlinked = ApiExtended.LinkEpisodesToSeries(items);

		CollectionAssert.AreEqual(new[] { "EP2" }, unlinked.Select(i => i.Asin).ToList());
		CollectionAssert.AreEqual(new[] { "BOOK" }, items.Select(i => i.Asin).ToList());
	}

	[TestMethod]
	public void only_the_episode_with_the_missing_parent_is_dropped()
	{
		var items = new List<Item>
		{
			show("SHOW", "My Show", "EP1", "EP3"),
			episode("EP1", "Episode one", "SHOW", 1),
			episode("EP2", "Episode two", "OTHER_SHOW", 2),
			episode("EP3", "Episode three", "SHOW", 3)
		};

		var unlinked = ApiExtended.LinkEpisodesToSeries(items);

		CollectionAssert.AreEqual(new[] { "EP2" }, unlinked.Select(i => i.Asin).ToList());
		CollectionAssert.AreEquivalent(new[] { "SHOW", "EP1", "EP3" }, items.Select(i => i.Asin).ToList());
	}

	[TestMethod]
	public void episode_under_a_season_container_is_dropped_because_a_season_is_not_a_series_parent()
	{
		var seasonItem = season("SEASON", "SHOW", "EP1");
		Assert.IsFalse(seasonItem.IsSeriesParent, "a season has a season parent of its own, so it is not a series parent");

		var items = new List<Item> { seasonItem, episode("EP1", "Episode one", "SEASON", 1) };

		var unlinked = ApiExtended.LinkEpisodesToSeries(items);

		// The episode has nothing to link to, so it is dropped. The season itself is left alone here;
		// it is filtered out of the scan earlier for being neither a series parent nor an episode.
		CollectionAssert.AreEqual(new[] { "EP1" }, unlinked.Select(i => i.Asin).ToList());
		CollectionAssert.AreEqual(new[] { "SEASON" }, items.Select(i => i.Asin).ToList());
		Assert.IsNull(seasonItem.Series);
	}

	[TestMethod]
	public void regular_audiobooks_are_never_dropped()
	{
		var items = new List<Item> { book("BOOK1"), book("BOOK2") };

		var unlinked = ApiExtended.LinkEpisodesToSeries(items);

		Assert.AreEqual(0, unlinked.Count);
		Assert.AreEqual(2, items.Count);
	}

	[TestMethod]
	public void a_show_with_no_episodes_in_the_scan_is_still_linked_to_itself()
	{
		var items = new List<Item> { show("SHOW", "My Show", "EP1") };

		var unlinked = ApiExtended.LinkEpisodesToSeries(items);

		Assert.AreEqual(0, unlinked.Count);
		Assert.AreEqual("SHOW", items.Single().Series!.Single().Asin);
	}

	[TestMethod]
	public void duplicate_copies_of_an_episode_are_both_linked()
	{
		// An episode can arrive both from the library pages and from the catalog batch.
		var items = new List<Item>
		{
			show("SHOW", "My Show", "EP1"),
			episode("EP1", "Episode one", "SHOW", 1),
			episode("EP1", "Episode one", "SHOW", 1)
		};

		var unlinked = ApiExtended.LinkEpisodesToSeries(items);

		Assert.AreEqual(0, unlinked.Count);
		Assert.AreEqual(3, items.Count);
	}

	[TestMethod]
	public void empty_input_is_a_no_op()
	{
		var items = new List<Item>();

		var unlinked = ApiExtended.LinkEpisodesToSeries(items);

		Assert.AreEqual(0, unlinked.Count);
		Assert.AreEqual(0, items.Count);
	}
}
