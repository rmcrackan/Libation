namespace LibationUiBase.Tests;

[TestClass]
public class CrashLogMessageTests
{
	[TestMethod]
	public void Normal_logger_without_fallback_path_does_not_claim_failure()
	{
		var text = CrashLogMessage.Describe(null, true, "current.log");
		Assert.IsTrue(text.Contains("submitted"));
		Assert.IsTrue(text.Contains("current.log"));
		Assert.IsFalse(text.Contains("could not"));
		Assert.IsFalse(text.Contains("was written"));
	}

	[TestMethod]
	public void Normal_logger_without_known_path_still_requests_log()
	{
		var text = CrashLogMessage.Describe(null, true, null);
		Assert.IsTrue(text.Contains("attach the current Libation log"));
		Assert.IsFalse(text.Contains("could not"));
	}

	[TestMethod]
	public void Successful_fallback_names_the_file_actually_written()
	{
		var text = CrashLogMessage.Describe("fallback.log", false, "old.log");
		Assert.IsTrue(text.Contains("was written to:"));
		Assert.IsTrue(text.Contains("fallback.log"));
		Assert.IsFalse(text.Contains("old.log"));
	}

	[TestMethod]
	public void Neither_logging_path_succeeded_requests_exception_text()
	{
		var text = CrashLogMessage.Describe(null, false, "old.log");
		Assert.IsTrue(text.Contains("could not confirm"));
		Assert.IsTrue(text.Contains("include the text below"));
		Assert.IsFalse(text.Contains("old.log"));
	}
}
