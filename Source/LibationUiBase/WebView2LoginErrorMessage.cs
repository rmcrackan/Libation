using System;
using System.Runtime.InteropServices;

namespace LibationUiBase;

/// <summary>
/// User-facing copy and exception matching when embedded sign-in browser fails (often mistaken for a "library scan" bug).
/// Shared by WinForms and Avalonia; stack markers include WebView2 and Avalonia's NativeWebDialog.
/// </summary>
public static class WebView2LoginErrorMessage
{
	public const string Caption = "Sign-in browser could not start";

	public static string ExplainerBody =>
		"Libation could not start the in-app sign-in browser. On Windows this uses Microsoft WebView2. "
		+ "This is a local sign-in or system setup issue — not a failure of the library scan itself.\r\n\r\n"
		+ "Things to try:\r\n"
		+ "• On Windows: install or repair the Microsoft Edge WebView2 Runtime from https://developer.microsoft.com/microsoft-edge/webview2/\r\n"
		+ "• In Libation Settings, try turning off the option to use the embedded browser and use external browser sign-in instead.\r\n"
		+ "• Check that security software is not blocking Libation or the embedded browser.\r\n"
		+ "• Ensure your account can write to local app data folders (permissions).\r\n"
		+ "• If you run as Administrator, try running Libation as a normal user (or the reverse).\r\n\r\n"
		+ "After sign-in works, use Import again to scan your library.";

	public static bool IsWebView2SignInInfrastructureFailure(Exception ex)
	{
		for (var e = ex; e is not null; e = e.InnerException)
		{
			if (!StackMentionsEmbeddedSignInBrowser(e))
				continue;

			if (e is UnauthorizedAccessException)
				return true;

			if (e is COMException com
				&& (com.HResult == unchecked((int)0x8000FFFF) || com.HResult == unchecked((int)0x80070005)))
				return true;
		}

		return false;
	}

	public static bool TryFindInTree(Exception ex, out Exception? match)
	{
		Exception? found = null;
		walk(ex);
		match = found;
		return found is not null;

		void walk(Exception? e)
		{
			if (e is null || found is not null)
				return;
			if (IsWebView2SignInInfrastructureFailure(e))
				found = e;
			if (found is not null)
				return;
			if (e is AggregateException agg)
			{
				foreach (var inner in agg.InnerExceptions)
					walk(inner);
			}
			walk(e.InnerException);
		}
	}

	private static bool StackMentionsEmbeddedSignInBrowser(Exception e)
	{
		var stack = e.StackTrace;
		return stack is not null
			&& (stack.Contains("WebView2", StringComparison.Ordinal)
				|| stack.Contains("CoreWebView2", StringComparison.Ordinal)
				|| stack.Contains("NativeWebDialog", StringComparison.Ordinal));
	}
}
