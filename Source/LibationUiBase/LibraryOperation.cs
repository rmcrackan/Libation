using System;
using System.Threading.Tasks;

namespace LibationUiBase;

/// <summary>Await UI operations, report their failures, and always restore disabled controls.</summary>
public static class LibraryOperation
{
	public static async Task RunAsync(Func<Task> operation, Func<Exception, Task> reportError, Action<bool>? setControlsEnabled = null)
	{
		try
		{
			setControlsEnabled?.Invoke(false);
			await operation();
		}
		catch (Exception ex)
		{
			await reportError(ex);
		}
		finally
		{
			setControlsEnabled?.Invoke(true);
		}
	}
}
