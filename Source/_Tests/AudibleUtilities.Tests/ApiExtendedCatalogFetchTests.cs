using AudibleApi.Common;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiExtendedCatalogFetchTests;

/// <summary>
/// Audible's catalog endpoint can answer 200 while quietly omitting products. When that happened
/// the affected podcast episodes vanished from the library with nothing in the log (issue #1925).
/// </summary>
[TestClass]
public class GetMissingAsins
{
	private static Item item(string asin) => new() { Asin = asin };

	[TestMethod]
	public void nothing_missing_when_all_returned()
	{
		var missing = ApiExtended.GetMissingAsins(["A1", "A2", "A3"], [item("A1"), item("A2"), item("A3")]);

		Assert.AreEqual(0, missing.Count);
	}

	[TestMethod]
	public void reports_asins_the_response_omitted()
	{
		var missing = ApiExtended.GetMissingAsins(["A1", "A2", "A3"], [item("A1"), item("A3")]);

		CollectionAssert.AreEqual(new[] { "A2" }, missing);
	}

	[TestMethod]
	public void reports_every_asin_when_response_is_empty()
	{
		var missing = ApiExtended.GetMissingAsins(["A1", "A2"], []);

		CollectionAssert.AreEqual(new[] { "A1", "A2" }, missing);
	}

	[TestMethod]
	public void asin_comparison_ignores_case()
	{
		var missing = ApiExtended.GetMissingAsins(["a1"], [item("A1")]);

		Assert.AreEqual(0, missing.Count);
	}

	[TestMethod]
	public void items_without_an_asin_do_not_satisfy_a_request()
	{
		var missing = ApiExtended.GetMissingAsins(["A1"], [new Item()]);

		CollectionAssert.AreEqual(new[] { "A1" }, missing);
	}

	[TestMethod]
	public void extra_unrequested_items_are_ignored()
	{
		var missing = ApiExtended.GetMissingAsins(["A1"], [item("A1"), item("A9")]);

		Assert.AreEqual(0, missing.Count);
	}
}

[TestClass]
public class FetchRetryingMissingAsync
{
	private static Item item(string asin) => new() { Asin = asin };

	/// <summary>Returns the requested asins minus <paramref name="omit"/>, recording each request.</summary>
	private static Func<List<string>, Task<List<Item>>> fetcher(List<List<string>> requests, params string[] omit)
		=> asins =>
		{
			requests.Add([.. asins]);
			return Task.FromResult(asins.Where(a => !omit.Contains(a)).Select(item).ToList());
		};

	[TestMethod]
	public async Task complete_response_is_not_retried()
	{
		var requests = new List<List<string>>();

		var (items, missing) = await ApiExtended.FetchRetryingMissingAsync(["A1", "A2"], fetcher(requests), maxRetries: 2);

		Assert.AreEqual(1, requests.Count);
		Assert.AreEqual(2, items.Count);
		Assert.AreEqual(0, missing.Count);
	}

	[TestMethod]
	public async Task omitted_asin_is_re_requested_on_its_own()
	{
		var requests = new List<List<string>>();
		var call = 0;

		// Audible drops A2 from the first response, then returns it when asked again.
		Task<List<Item>> fetch(List<string> asins)
		{
			requests.Add([.. asins]);
			var omit = call++ == 0 ? "A2" : null;
			return Task.FromResult(asins.Where(a => a != omit).Select(item).ToList());
		}

		var (items, missing) = await ApiExtended.FetchRetryingMissingAsync(["A1", "A2", "A3"], fetch, maxRetries: 2);

		Assert.AreEqual(2, requests.Count);
		CollectionAssert.AreEqual(new[] { "A2" }, requests[1]);
		CollectionAssert.AreEquivalent(new[] { "A1", "A2", "A3" }, items.Select(i => i.Asin).ToList());
		Assert.AreEqual(0, missing.Count);
	}

	[TestMethod]
	public async Task persistently_omitted_asin_is_reported_after_the_retries_run_out()
	{
		var requests = new List<List<string>>();

		var (items, missing) = await ApiExtended.FetchRetryingMissingAsync(["A1", "A2"], fetcher(requests, "A2"), maxRetries: 2);

		Assert.AreEqual(3, requests.Count);
		CollectionAssert.AreEqual(new[] { "A1" }, items.Select(i => i.Asin).ToList());
		CollectionAssert.AreEqual(new[] { "A2" }, missing);
	}

	[TestMethod]
	public async Task retries_can_be_disabled()
	{
		var requests = new List<List<string>>();

		var (_, missing) = await ApiExtended.FetchRetryingMissingAsync(["A1", "A2"], fetcher(requests, "A2"), maxRetries: 0);

		Assert.AreEqual(1, requests.Count);
		CollectionAssert.AreEqual(new[] { "A2" }, missing);
	}

	[TestMethod]
	public async Task onRetry_reports_each_attempt_number()
	{
		var attempts = new List<int>();

		await ApiExtended.FetchRetryingMissingAsync(["A1"], fetcher([], "A1"), maxRetries: 2, attempts.Add);

		CollectionAssert.AreEqual(new[] { 1, 2 }, attempts);
	}

	[TestMethod]
	public async Task onRetry_is_not_called_for_a_complete_response()
	{
		var attempts = new List<int>();

		await ApiExtended.FetchRetryingMissingAsync(["A1"], fetcher([]), maxRetries: 2, attempts.Add);

		Assert.AreEqual(0, attempts.Count);
	}
}
