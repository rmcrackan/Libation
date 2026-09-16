namespace LibationUiBase.Tests;

[TestClass]
public class LibraryOperationTests
{
	[TestMethod]
	public async Task Delayed_failure_is_observed_and_controls_recover_after_alert_completes()
	{
		var operation = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var alertShown = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
		var dismissAlert = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var controls = new List<bool>();
		var failure = new InvalidOperationException("database failed");
		var pending = LibraryOperation.RunAsync(() => operation.Task, ex =>
		{
			alertShown.SetResult(ex);
			return dismissAlert.Task;
		}, controls.Add);

		CollectionAssert.AreEqual(new[] { false }, controls);
		Assert.IsFalse(pending.IsCompleted);
		operation.SetException(failure);
		Assert.AreSame(failure, await alertShown.Task.WaitAsync(TimeSpan.FromSeconds(5)));
		Assert.IsFalse(pending.IsCompleted, "Keep the operation active until the user dismisses the alert.");
		dismissAlert.SetResult();
		await pending.WaitAsync(TimeSpan.FromSeconds(5));
		CollectionAssert.AreEqual(new[] { false, true }, controls);
	}

	[TestMethod]
	public async Task Status_operation_failure_is_reported_without_faulting_the_returned_task()
	{
		var failure = new InvalidOperationException("status failed");
		Exception? reported = null;
		await LibraryOperation.RunAsync(() => Task.FromException(failure), ex =>
		{
			reported = ex;
			return Task.CompletedTask;
		});
		Assert.AreSame(failure, reported);
	}

	[TestMethod]
	public async Task Synchronous_failure_and_failed_alert_still_restore_controls()
	{
		var controls = new List<bool>();
		var alertFailure = new InvalidOperationException("alert failed");
		var result = LibraryOperation.RunAsync(() => throw new Exception("selection failed"),
			_ => Task.FromException(alertFailure), controls.Add);
		try
		{
			await result;
			Assert.Fail("The alert failure must propagate.");
		}
		catch (InvalidOperationException ex)
		{
			Assert.AreSame(alertFailure, ex);
		}
		CollectionAssert.AreEqual(new[] { false, true }, controls);
	}

	[TestMethod]
	public async Task Successful_operation_restores_controls_without_alert()
	{
		var controls = new List<bool>();
		await LibraryOperation.RunAsync(() => Task.CompletedTask, _ =>
		{
			Assert.Fail("No alert should be shown.");
			return Task.CompletedTask;
		}, controls.Add);
		CollectionAssert.AreEqual(new[] { false, true }, controls);
	}
}
