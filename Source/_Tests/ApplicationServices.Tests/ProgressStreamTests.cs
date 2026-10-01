using AssertionHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ApplicationServices.Tests;

[TestClass]
public class ProgressStreamTests
{
	[TestMethod]
	public void SynchronousProgress_reports_in_call_order()
	{
		var calls = new List<int>();
		IProgress<int> progress = new SynchronousProgress<int>(calls.Add);
		progress.Report(1);
		progress.Report(2);
		calls.Should().BeEquivalentTo(new[] { 1, 2 });
	}

	[TestMethod]
	public void SynchronousProgress_propagates_handler_exception()
	{
		var exception = new InvalidOperationException("progress failed");
		IProgress<int> progress = new SynchronousProgress<int>(_ => throw exception);
		Assert.ThrowsExactly<InvalidOperationException>(() => progress.Report(1)).Should().BeSameAs(exception);
	}

	[TestMethod]
	public void SynchronousProgress_has_no_post_completion_callbacks()
	{
		var completed = false;
		var calls = 0;
		IProgress<int> progress = new SynchronousProgress<int>(_ =>
		{
			if (completed) throw new InvalidOperationException("callback after completion");
			calls++;
		});
		progress.Report(1);
		completed = true;
		calls.Should().Be(1);
	}
}
