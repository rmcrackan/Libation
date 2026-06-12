using LibationUiBase.Forms;
using System.Threading.Tasks;

namespace LibationUiBase;

public record BadBookDialogResult(
	DialogResult Action,
	bool ApplyToAll,
	bool RememberInSettings);

public delegate Task<BadBookDialogResult> ShowBadBookDialogAsyncDelegate(
	object? owner, string message, string caption);

public static class BadBookActionDialogBase
{
	private static ShowBadBookDialogAsyncDelegate? s_ShowAsyncImpl;

	public static ShowBadBookDialogAsyncDelegate ShowAsyncImpl
	{
		get => s_ShowAsyncImpl ?? DefaultShowAsyncImpl;
		set => s_ShowAsyncImpl = value;
	}

	private static Task<BadBookDialogResult> DefaultShowAsyncImpl(object? owner, string message, string caption)
	{
		Serilog.Log.Logger.Error("BadBookActionDialogBase implementation not set. {@DebugInfo}", new { owner, message, caption });
		return Task.FromResult(new BadBookDialogResult(DialogResult.Retry, false, false));
	}

	public static Task<BadBookDialogResult> Show(string message, string caption)
		=> ShowAsyncImpl(null, message, caption);

	public static Task<BadBookDialogResult> Show(object? owner, string message, string caption)
		=> ShowAsyncImpl(owner, message, caption);
}
