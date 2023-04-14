using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LibationFileManager
{
#nullable enable
	public interface IInteropFunctions
    {
		/// <summary>
		/// Implementation of native web view control https://github.com/maxkatz6/AvaloniaWebView
		/// </summary>
		IWebViewAdapter? CreateWebViewAdapter();
        void SetFolderIcon(string image, string directory);
        void DeleteFolderIcon(string directory);
        Process RunAsRoot(string exe, string args);
        void InstallUpgrade(string upgradeBundle);
        bool CanUpgrade { get; }
    }

	public class WebViewNavigationEventArgs : EventArgs
	{
		public Uri? Request { get; init; }
	}

	public interface IWebView
	{
		event EventHandler<WebViewNavigationEventArgs>? NavigationCompleted;
		event EventHandler<WebViewNavigationEventArgs>? NavigationStarted;
		event EventHandler? DOMContentLoaded;
		bool CanGoBack { get; }
		bool CanGoForward { get; }
		Uri? Source { get; set; }
		bool GoBack();
		bool GoForward();
		Task<string?> InvokeScriptAsync(string scriptName);
		void Navigate(Uri url);
		Task NavigateToString(string text);
		void Refresh();
		void Stop();
	}

	public interface IWebViewAdapter : IWebView
	{
		object NativeWebView { get; }
		IPlatformHandle2 PlatformHandle { get; }
		void HandleResize(int width, int height, float zoom);
		bool HandleKeyDown(uint key, uint keyModifiers);
	}

	public interface IPlatformHandle2
	{
		IntPtr Handle { get; }
		string? HandleDescriptor { get; }
	}
}
