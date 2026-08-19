using AssertionHelper;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace QuickFiltersTests;

[TestClass]
[DoNotParallelize]
public class QuickFiltersTests
{
	private string tempDir = null!;

	[TestInitialize]
	public void Initialize()
	{
		tempDir = Path.Combine(Path.GetTempPath(), "QuickFiltersTests_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(tempDir);

		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, tempDir);
		Configuration.CreateMockInstance();
		QuickFilters.Reset();
	}

	[TestCleanup]
	public void Cleanup()
	{
		QuickFilters.Reset();
		Configuration.RestoreSingletonInstance();
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, null);
		try { Directory.Delete(tempDir, recursive: true); } catch { }
	}

	private string JsonFile => Path.Combine(tempDir, "QuickFilters.json");

	#region loading from file

	[TestMethod]
	public void Loads_current_format_from_file()
	{
		// Exact file contents attached to issue #1979
		File.WriteAllText(JsonFile, """
			{
			  "UseDefault": false,
			  "Filters": [
			    {
			      "Filter": "!IsPodcast&!IsLiberated&!Absent",
			      "Name": "!IsPodcast&!IsLiberated&!Absent"
			    }
			  ]
			}
			""");

		var state = QuickFilters.LoadFromFile(JsonFile);

		state.UseDefault.Should().BeFalse();
		state.Filters.Count.Should().Be(1);
		state.Filters[0].Filter.Should().Be("!IsPodcast&!IsLiberated&!Absent");
		state.Filters[0].Name.Should().Be("!IsPodcast&!IsLiberated&!Absent");
	}

	[TestMethod]
	public void Loads_legacy_pre_11_5_0_format_from_file()
	{
		File.WriteAllText(JsonFile, """
			{
			  "UseDefault": true,
			  "Filters": [ "tag1", "author:Sanderson" ]
			}
			""");

		var state = QuickFilters.LoadFromFile(JsonFile);

		state.UseDefault.Should().BeTrue();
		state.Filters.Select(f => f.Filter).Should().BeEquivalentTo(new[] { "tag1", "author:Sanderson" });
		state.Filters.All(f => f.Name == null).Should().BeTrue();
	}

	[TestMethod]
	public void Missing_file_yields_empty_state()
	{
		var state = QuickFilters.LoadFromFile(JsonFile);

		state.UseDefault.Should().BeFalse();
		state.Filters.Should().HaveCount(0);
	}

	[TestMethod]
	public void Malformed_file_yields_empty_state_without_throwing()
	{
		File.WriteAllText(JsonFile, "this is not json {{{");

		var state = QuickFilters.LoadFromFile(JsonFile);

		state.UseDefault.Should().BeFalse();
		state.Filters.Should().HaveCount(0);
	}

	#endregion

	#region persistence across restarts (issue #1979)

	[TestMethod]
	public void Filters_survive_restart()
	{
		QuickFilters.Add(new QuickFilters.NamedFilter("!IsPodcast&!IsLiberated&!Absent", "My filter"));
		File.Exists(JsonFile).Should().BeTrue();

		// Simulate an app restart: discard all in-memory state
		QuickFilters.Reset();

		var filters = QuickFilters.Filters.ToList();
		filters.Count.Should().Be(1);
		filters[0].Filter.Should().Be("!IsPodcast&!IsLiberated&!Absent");
		filters[0].Name.Should().Be("My filter");
	}

	[TestMethod]
	public void Adding_after_restart_appends_instead_of_overwriting()
	{
		QuickFilters.Add(new QuickFilters.NamedFilter("filter one", null));
		QuickFilters.Reset();

		QuickFilters.Add(new QuickFilters.NamedFilter("filter two", null));

		QuickFilters.Filters.Select(f => f.Filter).Should().BeEquivalentTo(new[] { "filter one", "filter two" });

		// And the file on disk has both, too
		QuickFilters.Reset();
		QuickFilters.Filters.Select(f => f.Filter).Should().BeEquivalentTo(new[] { "filter one", "filter two" });
	}

	[TestMethod]
	public void UseDefault_survives_restart()
	{
		QuickFilters.Add(new QuickFilters.NamedFilter("some filter", null));
		QuickFilters.UseDefault = true;

		QuickFilters.Reset();

		QuickFilters.UseDefault.Should().BeTrue();
	}

	[TestMethod]
	public void UseDefault_can_be_set_before_any_filter_is_added()
	{
		QuickFilters.UseDefault = true;

		QuickFilters.Reset();

		QuickFilters.UseDefault.Should().BeTrue();
		QuickFilters.Filters.Should().HaveCount(0);
	}

	#endregion
}
