using System;
using System.Runtime.InteropServices;
using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>
/// User-facing copy and exception matching when embedded sign-in browser fails (often mistaken for a "library scan" bug).
/// Shared by WinForms and Avalonia; stack markers include WebView2 and Avalonia's NativeWebDialog.
/// </summary>
public static class WebView2LoginErrorMessage
{
	public static string Caption => Strings.WebView2LoginErrorMessage_SignInBrowserCouldNotStart;

	public static string ExplainerBody
	{
		get
		{
			string localIssue =
				Strings.WebView2LoginErrorMessage_ThisIsALocalSignIn;
			string thingsToTry = Strings.WebView2LoginErrorMessage_ThingsToTry;
			string settingsBullet =
				Strings.WebView2LoginErrorMessage_InLibationSettingsTryTurningOff;
			string securityBullet =
				Strings.WebView2LoginErrorMessage_CheckThatSecuritySoftwareIsNot;
			string permissionsBullet =
				Strings.WebView2LoginErrorMessage_EnsureYourAccountCanWriteTo;
			string adminBullet =
				Strings.WebView2LoginErrorMessage_IfYouRunAsAdministratorTry;
			string footer = Strings.WebView2LoginErrorMessage_AfterSignInWorksUseImport;

			if (OperatingSystem.IsLinux())
			{
				return string.Format(Strings.WebView2LoginErrorMessage_LibationCouldNotStartTheIn, localIssue, thingsToTry, settingsBullet, securityBullet, permissionsBullet, adminBullet, footer);
			}

			return string.Format(Strings.WebView2LoginErrorMessage_LibationCouldNotStartTheIn2, localIssue, thingsToTry, settingsBullet, securityBullet, permissionsBullet, adminBullet, footer);
		}
	}

	public static bool IsWebView2SignInInfrastructureFailure(Exception ex)
	{
		for (var e = ex; e is not null; e = e.InnerException)
		{
			// Gtk / WebView2: missing native WebKit or WebView2 runtime often surfaces as DllNotFoundException (sometimes without a useful stack).
			if (e is DllNotFoundException dll && IsEmbeddedWebViewNativeDllFailure(dll))
				return true;

			// AvaloniaGtk.RunTask throws InvalidOperationException("Unable to initialize GTK") when GTK cannot start
			// (for example on Wayland without X11 backend or when no GTK display is available).
			if (e is InvalidOperationException && e.Message.Contains("Unable to initialize GTK", StringComparison.OrdinalIgnoreCase))
				return true;

			if (!StackMentionsEmbeddedSignInBrowser(e))
				continue;

			if (e is UnauthorizedAccessException or InvalidOperationException or EntryPointNotFoundException or TypeInitializationException)
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
			|| e.Message.Contains("webkit2gtk", StringComparison.OrdinalIgnoreCase)
			|| e.Message.Contains("libgtk-3", StringComparison.OrdinalIgnoreCase)
			|| e.Message.Contains("libgdk-3", StringComparison.OrdinalIgnoreCase))
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
				|| stack.Contains("GtkNativeWebViewDialog", StringComparison.Ordinal)
				|| stack.Contains("GtkWebView", StringComparison.Ordinal)
				|| stack.Contains("AvaloniaGtk", StringComparison.Ordinal)
				|| stack.Contains("WebViewControlAvalonia", StringComparison.Ordinal));
	}
}
