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

	public static string ExplainerBody
	{
		get
		{
			const string localIssue =
				"This is a local sign-in or system setup issue — not a failure of the library scan itself.\r\n\r\n";
			const string thingsToTry = "Things to try:\r\n";
			const string settingsBullet =
				"• In Libation Settings, try turning off the option to use the embedded browser and use external browser sign-in instead.\r\n";
			const string securityBullet =
				"• Check that security software is not blocking Libation or the embedded browser.\r\n";
			const string permissionsBullet =
				"• Ensure your account can write to local app data folders (permissions).\r\n";
			const string adminBullet =
				"• If you run as Administrator, try running Libation as a normal user (or the reverse).\r\n\r\n";
			const string footer = "After sign-in works, use Import again to scan your library.";

			if (OperatingSystem.IsLinux())
			{
				return "Libation could not start the in-app sign-in browser. On Linux this uses WebKit2GTK (native WebKit). "
					+ localIssue
					+ thingsToTry
					+ "• Install WebKit2GTK for your distro (e.g. on Arch: webkit2gtk or webkit2gtk-4.1).\r\n"
					+ settingsBullet
					+ securityBullet
					+ permissionsBullet
					+ adminBullet
					+ footer;
			}

			return "Libation could not start the in-app sign-in browser. On Windows this uses Microsoft WebView2. "
				+ localIssue
				+ thingsToTry
				+ "• On Windows: install or repair the Microsoft Edge WebView2 Runtime from https://developer.microsoft.com/microsoft-edge/webview2/\r\n"
				+ settingsBullet
				+ securityBullet
				+ permissionsBullet
				+ adminBullet
				+ footer;
		}
	}

	public static bool IsWebView2SignInInfrastructureFailure(Exception ex)
	{
		for (var e = ex; e is not null; e = e.InnerException)
		{
			// Gtk / WebView2: missing native WebKit or WebView2 runtime often surfaces as DllNotFoundException (sometimes without a useful stack).
			if (e is DllNotFoundException dll && IsEmbeddedWebViewNativeDllFailure(dll))
				return true;

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

	/// <summary>
	/// Gtk WebView (e.g. Avalonia NativeWebDialog on Linux) requires WebKit2GTK; minimal installs omit <c>libwebkit2gtk</c>.
	/// </summary>
	private static bool IsEmbeddedWebViewNativeDllFailure(DllNotFoundException e)
	{
		if (e.Message.Contains("libwebkit2gtk", StringComparison.OrdinalIgnoreCase)
			|| e.Message.Contains("webkit2gtk", StringComparison.OrdinalIgnoreCase))
			return true;

		return StackMentionsEmbeddedSignInBrowser(e);
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
				|| stack.Contains("NativeWebDialog", StringComparison.Ordinal)
				|| stack.Contains("GtkWebView", StringComparison.Ordinal)
				|| stack.Contains("WebViewControlAvalonia", StringComparison.Ordinal));
	}
}
