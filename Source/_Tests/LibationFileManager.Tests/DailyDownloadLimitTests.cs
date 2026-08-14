using AssertionHelper;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DailyDownloadLimitTests;

[TestClass]
[DoNotParallelize]
public class DailyDownloadLimitTests
{
	private static readonly DateTimeOffset Now = new(2026, 3, 14, 21, 0, 0, TimeSpan.FromHours(-4));

	private const long EstimatedBookBytes = DiskSpaceHelper.EstimatedBytesPerAudiobookBackup;

	[TestCleanup]
	public void Cleanup() => Configuration.RestoreSingletonInstance();

	private static Configuration Config(
		Configuration.DailyLimitScope scope,
		int quantity = 50,
		Configuration.DailyLimitUnit unit = Configuration.DailyLimitUnit.Books)
	{
		var config = Configuration.CreateMockInstance();
		config.DailyDownloadLimit = scope;
		config.DailyDownloadLimitQuantity = quantity;
		config.DailyDownloadLimitUnit = unit;
		return config;
	}

	private static List<DownloadHistoryEntry> Downloads(int count, DateTimeOffset at, bool isPlus = true, long bytes = 300_000_000)
		=> Enumerable.Range(0, count)
		.Select(i => new DownloadHistoryEntry(at.AddSeconds(i), $"ASIN{i}", isPlus, bytes))
		.ToList();

	#region defaults

	[TestMethod]
	public void Defaults_are_off_and_write_nothing()
	{
		var config = Configuration.CreateMockInstance();

		config.Exists(nameof(Configuration.DailyDownloadLimit)).Should().BeFalse();
		config.Exists(nameof(Configuration.DailyDownloadLimitQuantity)).Should().BeFalse();
		config.Exists(nameof(Configuration.DailyDownloadLimitUnit)).Should().BeFalse();

		Assert.AreEqual(Configuration.DailyLimitScope.NoLimit, config.DailyDownloadLimit);
		Assert.AreEqual(50, config.DailyDownloadLimitQuantity);
		Assert.AreEqual(Configuration.DailyLimitUnit.Books, config.DailyDownloadLimitUnit);
	}

	[TestMethod]
	public void Quantity_of_zero_or_negative_is_clamped_to_one()
	{
		var config = Configuration.CreateMockInstance();

		config.DailyDownloadLimitQuantity = 0;
		Assert.AreEqual(1, config.DailyDownloadLimitQuantity);

		config.DailyDownloadLimitQuantity = -7;
		Assert.AreEqual(1, config.DailyDownloadLimitQuantity);
	}

	[TestMethod]
	public void NoLimit_never_blocks_anything()
	{
		var config = Config(Configuration.DailyLimitScope.NoLimit);
		var history = Downloads(500, Now.AddHours(-1));

		var allowance = DailyDownloadLimit.Evaluate(config, history, Now);

		allowance.IsLimited.Should().BeFalse();
		allowance.AllowsAnother.Should().BeTrue();
		allowance.Blocks(isPlus: true).Should().BeFalse();
		allowance.Blocks(isPlus: false).Should().BeFalse();
		Assert.IsNull(allowance.RemainingBooks);
		Assert.IsNull(allowance.NextCapacityAt);
	}

	#endregion

	#region rolling window

	[TestMethod]
	public void Fifty_downloads_at_eleven_pm_still_blocks_two_hours_later()
	{
		var config = Config(Configuration.DailyLimitScope.AllBooks, quantity: 50);
		var elevenPm = Now.AddHours(2);
		var history = Downloads(50, elevenPm);

		var oneAmNextDay = elevenPm.AddHours(2);
		var allowance = DailyDownloadLimit.Evaluate(config, history, oneAmNextDay);

		allowance.Blocks(isPlus: false).Should().BeTrue();
		allowance.UsedBooks.Should().Be(50);
		Assert.AreEqual(0, allowance.RemainingBooks!.Value);
	}

	[TestMethod]
	public void Capacity_returns_twenty_four_hours_after_the_download_not_at_midnight()
	{
		var config = Config(Configuration.DailyLimitScope.AllBooks, quantity: 50);
		var elevenPm = Now.AddHours(2);
		var history = Downloads(50, elevenPm);

		// One second before the oldest download ages out.
		DailyDownloadLimit
			.Evaluate(config, history, elevenPm.AddHours(24).AddSeconds(-1))
			.Blocks(isPlus: false)
			.Should().BeTrue();

		// And just after.
		DailyDownloadLimit
			.Evaluate(config, history, elevenPm.AddHours(24).AddSeconds(1))
			.Blocks(isPlus: false)
			.Should().BeFalse();
	}

	[TestMethod]
	public void NextCapacityAt_is_twenty_four_hours_after_the_oldest_counted_download()
	{
		var config = Config(Configuration.DailyLimitScope.AllBooks, quantity: 2);
		var oldest = Now.AddHours(-5);
		var history = new List<DownloadHistoryEntry>
		{
			new(Now.AddHours(-1), "NEWER", false, 1),
			new(oldest, "OLDEST", false, 1),
		};

		var allowance = DailyDownloadLimit.Evaluate(config, history, Now);

		allowance.Blocks(isPlus: false).Should().BeTrue();
		Assert.AreEqual(oldest.AddHours(24), allowance.NextCapacityAt);
	}

	[TestMethod]
	public void Partial_expiry_frees_capacity_a_few_at_a_time()
	{
		// What a multi-day queue relies on: an evening of downloads frees up over the following evening.
		var config = Config(Configuration.DailyLimitScope.AllBooks, quantity: 10);
		var evening = Now;
		var history = new List<DownloadHistoryEntry>();
		for (var i = 0; i < 10; i++)
			history.Add(new DownloadHistoryEntry(evening.AddMinutes(i * 30), $"ASIN{i}", false, 1));

		// A day later, the first three have aged out, so three slots are free and no more.
		var nextEvening = evening.AddHours(24).AddMinutes(75);
		var allowance = DailyDownloadLimit.Evaluate(config, history, nextEvening);

		allowance.UsedBooks.Should().Be(7);
		Assert.AreEqual(3, allowance.RemainingBooks!.Value);
		allowance.AllowsAnother.Should().BeTrue();

		// Filling those three blocks again until the next one ages out.
		history.AddRange(Downloads(3, nextEvening, isPlus: false, bytes: 1));
		var refilled = DailyDownloadLimit.Evaluate(config, history, nextEvening.AddMinutes(1));

		refilled.UsedBooks.Should().Be(10);
		refilled.Blocks(isPlus: false).Should().BeTrue();
		Assert.AreEqual(evening.AddMinutes(90).AddHours(24), refilled.NextCapacityAt);
	}

	[TestMethod]
	public void Downloads_older_than_the_window_are_ignored()
	{
		var config = Config(Configuration.DailyLimitScope.AllBooks, quantity: 5);
		var history = Downloads(50, Now.AddHours(-30), isPlus: false);

		var allowance = DailyDownloadLimit.Evaluate(config, history, Now);

		allowance.UsedBooks.Should().Be(0);
		allowance.Blocks(isPlus: false).Should().BeFalse();
	}

	#endregion

	#region scope

	[TestMethod]
	public void PlusOnly_counts_and_blocks_only_plus_titles()
	{
		var config = Config(Configuration.DailyLimitScope.PlusOnly, quantity: 3);
		var history = Downloads(3, Now.AddHours(-2), isPlus: true)
			.Concat(Downloads(9, Now.AddHours(-1), isPlus: false))
			.ToList();

		var allowance = DailyDownloadLimit.Evaluate(config, history, Now);

		allowance.UsedBooks.Should().Be(3);
		allowance.Blocks(isPlus: true).Should().BeTrue();
		allowance.Blocks(isPlus: false).Should().BeFalse();

		DailyDownloadLimit.AppliesTo(isPlus: true, config).Should().BeTrue();
		DailyDownloadLimit.AppliesTo(isPlus: false, config).Should().BeFalse();
	}

	[TestMethod]
	public void AllBooks_counts_owned_and_plus_together()
	{
		var config = Config(Configuration.DailyLimitScope.AllBooks, quantity: 5);
		var history = Downloads(3, Now.AddHours(-2), isPlus: true)
			.Concat(Downloads(2, Now.AddHours(-1), isPlus: false))
			.ToList();

		var allowance = DailyDownloadLimit.Evaluate(config, history, Now);

		allowance.UsedBooks.Should().Be(5);
		allowance.Blocks(isPlus: true).Should().BeTrue();
		allowance.Blocks(isPlus: false).Should().BeTrue();
	}

	#endregion

	#region MB and GB

	[TestMethod]
	public void Byte_limit_blocks_when_another_estimated_book_would_not_fit()
	{
		// 1 GB limit, 800 MB already used: a further ~400 MB book would exceed it.
		var config = Config(Configuration.DailyLimitScope.AllBooks, quantity: 1, unit: Configuration.DailyLimitUnit.GB);
		var history = Downloads(2, Now.AddHours(-1), isPlus: false, bytes: 400_000_000);

		var allowance = DailyDownloadLimit.Evaluate(config, history, Now);

		allowance.UsedBytes.Should().Be(800_000_000);
		allowance.Blocks(isPlus: false).Should().BeTrue();
		Assert.AreEqual(1024L * 1024 * 1024, allowance.LimitBytes);
	}

	[TestMethod]
	public void Byte_limit_allows_when_another_estimated_book_fits()
	{
		var config = Config(Configuration.DailyLimitScope.AllBooks, quantity: 2, unit: Configuration.DailyLimitUnit.GB);
		var history = Downloads(2, Now.AddHours(-1), isPlus: false, bytes: 400_000_000);

		var allowance = DailyDownloadLimit.Evaluate(config, history, Now);

		allowance.Blocks(isPlus: false).Should().BeFalse();
		Assert.AreEqual((int)((2L * 1024 * 1024 * 1024 - 800_000_000) / EstimatedBookBytes), allowance.RemainingBooks!.Value);
	}

	[TestMethod]
	public void MB_and_GB_use_the_same_scale_as_the_rest_of_Libation()
	{
		var mb = Config(Configuration.DailyLimitScope.AllBooks, quantity: 1024, unit: Configuration.DailyLimitUnit.MB);
		var gb = Config(Configuration.DailyLimitScope.AllBooks, quantity: 1, unit: Configuration.DailyLimitUnit.GB);

		Assert.AreEqual(
			DailyDownloadLimit.Evaluate(gb, [], Now).LimitBytes,
			DailyDownloadLimit.Evaluate(mb, [], Now).LimitBytes);
	}

	[TestMethod]
	public void An_empty_window_always_allows_one_download_even_below_the_estimate()
	{
		// Without this rule a 1 MB limit would block downloading forever and report "limit reached"
		// to a user who had downloaded nothing.
		var config = Config(Configuration.DailyLimitScope.AllBooks, quantity: 1, unit: Configuration.DailyLimitUnit.MB);

		var allowance = DailyDownloadLimit.Evaluate(config, [], Now);

		allowance.AllowsAnother.Should().BeTrue();
		allowance.Blocks(isPlus: false).Should().BeFalse();
		Assert.AreEqual(1, allowance.RemainingBooks!.Value);

		// But only one: after that download the limit holds.
		var afterOne = DailyDownloadLimit.Evaluate(config, Downloads(1, Now, isPlus: false, bytes: 300_000_000), Now.AddMinutes(1));
		afterOne.Blocks(isPlus: false).Should().BeTrue();
	}

	[TestMethod]
	public void Byte_limit_next_capacity_waits_for_enough_bytes_to_age_out()
	{
		// 1 GB limit; three 400 MB downloads. Losing only the oldest still leaves 800 MB used,
		// which cannot fit another estimated book, so capacity returns with the second.
		var config = Config(Configuration.DailyLimitScope.AllBooks, quantity: 1, unit: Configuration.DailyLimitUnit.GB);
		var first = Now.AddHours(-6);
		var second = Now.AddHours(-5);
		var history = new List<DownloadHistoryEntry>
		{
			new(first, "A", false, 400_000_000),
			new(second, "B", false, 400_000_000),
			new(Now.AddHours(-4), "C", false, 400_000_000),
		};

		var allowance = DailyDownloadLimit.Evaluate(config, history, Now);

		allowance.Blocks(isPlus: false).Should().BeTrue();
		Assert.AreEqual(second.AddHours(24), allowance.NextCapacityAt);
	}

	#endregion

	#region throttling suggestion

	[TestMethod]
	public void Suggestion_is_silent_below_the_threshold()
	{
		var config = Config(Configuration.DailyLimitScope.NoLimit);
		var history = Downloads(DailyDownloadLimit.SuggestionMinimumRecentDownloads - 1, Now.AddHours(-1));

		DailyDownloadLimitUserMessage.BuildSuggestionParagraph(config, history, Now).Should().BeNull();
	}

	[TestMethod]
	public void Suggestion_is_silent_when_a_limit_is_already_configured()
	{
		var config = Config(Configuration.DailyLimitScope.PlusOnly);
		var history = Downloads(60, Now.AddHours(-1));

		DailyDownloadLimitUserMessage.BuildSuggestionParagraph(config, history, Now).Should().BeNull();
	}

	[TestMethod]
	public void Suggestion_quotes_the_real_recent_counts()
	{
		var config = Config(Configuration.DailyLimitScope.NoLimit);
		var history = Downloads(58, Now.AddHours(-3), isPlus: true)
			.Concat(Downloads(5, Now.AddHours(-2), isPlus: false))
			.Concat(Downloads(99, Now.AddHours(-40), isPlus: true))
			.ToList();

		var suggestion = DailyDownloadLimitUserMessage.BuildSuggestionParagraph(config, history, Now);

		Assert.IsNotNull(suggestion);
		StringAssert.Contains(suggestion, "63 titles");
		StringAssert.Contains(suggestion, "58 of them from the Plus catalog");
		StringAssert.Contains(suggestion, "Daily download limit");
	}

	[TestMethod]
	public void Recent_summary_ignores_downloads_outside_the_window()
	{
		var history = Downloads(4, Now.AddHours(-2), isPlus: true, bytes: 10)
			.Concat(Downloads(3, Now.AddHours(-1), isPlus: false, bytes: 10))
			.Concat(Downloads(99, Now.AddHours(-25), isPlus: true, bytes: 10))
			.ToList();

		var recent = DailyDownloadLimit.SummarizeRecent(history, Now);

		recent.TotalDownloads.Should().Be(7);
		recent.PlusDownloads.Should().Be(4);
		recent.TotalBytes.Should().Be(70);
	}

	#endregion

	#region user-facing copy

	[TestMethod]
	public void Paused_message_says_how_to_change_the_limit_and_when_it_resumes()
	{
		var config = Config(Configuration.DailyLimitScope.PlusOnly, quantity: 50);
		var allowance = DailyDownloadLimit.Evaluate(config, Downloads(50, Now.AddHours(-3)), Now);

		var body = DailyDownloadLimitUserMessage.BuildQueuePausedBody(allowance, "A Wrinkle in Time");

		StringAssert.Contains(body, "A Wrinkle in Time");
		StringAssert.Contains(body, "Settings > Download/Decrypt > Daily download limit");
		StringAssert.Contains(body, "still queued");
		StringAssert.Contains(body, "Cancel All");
	}

	[TestMethod]
	public void Waiting_status_names_the_resume_time_and_stays_short()
	{
		var config = Config(Configuration.DailyLimitScope.AllBooks, quantity: 1);
		var allowance = DailyDownloadLimit.Evaluate(config, Downloads(1, Now.AddHours(-3), isPlus: false), Now);

		var status = DailyDownloadLimitUserMessage.BuildWaitingStatus(allowance);

		StringAssert.Contains(status, "Daily limit");
		StringAssert.Contains(status, allowance.NextCapacityAt!.Value.ToLocalTime().ToString("t"));
		// The process queue column clips instead of wrapping, so this has to stay short enough to read.
		Assert.IsTrue(status.Length <= 34, $"Waiting status is too long for the queue column: '{status}'");
	}

	[TestMethod]
	public void Cli_lines_point_at_Settings_json()
	{
		var config = Config(Configuration.DailyLimitScope.AllBooks, quantity: 10);
		var allowance = DailyDownloadLimit.Evaluate(config, Downloads(10, Now.AddHours(-1), isPlus: false), Now);

		var lines = string.Join("\n", DailyDownloadLimitUserMessage.BuildCliSkippedLines(allowance));

		StringAssert.Contains(lines, "Daily download limit reached");
		StringAssert.Contains(lines, "Settings.json");
		StringAssert.Contains(lines, nameof(Configuration.DailyDownloadLimit));
	}

	#endregion
}
