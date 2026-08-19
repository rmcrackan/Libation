using AAXClean;
using AssertionHelper;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MaxSampleRateTests;

[TestClass]
[DoNotParallelize]
public class MaxSampleRateTests
{
	[TestCleanup]
	public void Cleanup() => Configuration.RestoreSingletonInstance();

	[TestMethod]
	public void Default_when_missing_is_Hz_44100()
	{
		var config = Configuration.CreateMockInstance();

		config.Exists(nameof(Configuration.MaxSampleRate)).Should().BeFalse();
		Assert.AreEqual(SampleRate.Hz_44100, config.MaxSampleRate);
	}

	[TestMethod]
	public void In_range_values_round_trip_unchanged()
	{
		var config = Configuration.CreateMockInstance();

		config.MaxSampleRate = SampleRate.Hz_8000;
		Assert.AreEqual(SampleRate.Hz_8000, config.MaxSampleRate);

		config.MaxSampleRate = SampleRate.Hz_22050;
		Assert.AreEqual(SampleRate.Hz_22050, config.MaxSampleRate);

		config.MaxSampleRate = SampleRate.Hz_48000;
		Assert.AreEqual(SampleRate.Hz_48000, config.MaxSampleRate);
	}

	[TestMethod]
	public void Setter_clamps_out_of_range_values()
	{
		var config = Configuration.CreateMockInstance();

		config.MaxSampleRate = SampleRate.Hz_7350;
		Assert.AreEqual(SampleRate.Hz_8000, config.MaxSampleRate);

		config.MaxSampleRate = SampleRate.Hz_96000;
		Assert.AreEqual(SampleRate.Hz_48000, config.MaxSampleRate);
	}

	[TestMethod]
	public void Getter_clamps_a_hand_edited_low_rate()
	{
		var config = Configuration.CreateMockInstance();

		// Bypass the property setter, as a hand-edited Settings.json or a pre-v11.6.5 install would
		config.SetNonString(nameof(SampleRate.Hz_7350), nameof(Configuration.MaxSampleRate));

		Assert.AreEqual(SampleRate.Hz_8000, config.MaxSampleRate);
	}

	[TestMethod]
	public void Getter_clamps_a_hand_edited_high_rate()
	{
		var config = Configuration.CreateMockInstance();

		config.SetNonString(nameof(SampleRate.Hz_96000), nameof(Configuration.MaxSampleRate));

		Assert.AreEqual(SampleRate.Hz_48000, config.MaxSampleRate);
	}
}
