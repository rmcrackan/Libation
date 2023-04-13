using LibationFileManager;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Threading.Tasks;

#nullable enable
namespace WindowsConfigApp;

internal class WindowsWebView2Adapter : IWebViewAdapter, IDisposable
{
	public object NativeWebView { get; }
	private readonly WebView2 _webView;

	public WindowsWebView2Adapter()
	{
		NativeWebView = _webView = new WebView2();
		PlatformHandle = new WebView2Handle { Handle = _webView.Handle };

		_webView.CoreWebView2InitializationCompleted += _webView_CoreWebView2InitializationCompleted;

		_webView.NavigationStarting += (s, a) =>
		{
			NavigationStarted?.Invoke(this, new WebViewNavigationEventArgs { Request = new Uri(a.Uri) });
		};
		_webView.NavigationCompleted += (s, a) =>
		{
			NavigationCompleted?.Invoke(this, new WebViewNavigationEventArgs { Request = _webView.Source });
		};
	}

	private void _webView_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
	{
		_webView.CoreWebView2.DOMContentLoaded -= CoreWebView2_DOMContentLoaded;
		_webView.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
	}

	private void CoreWebView2_DOMContentLoaded(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs e)
		=> DOMContentLoaded?.Invoke(this, e);

	public IPlatformHandle2 PlatformHandle { get; } 

	public bool CanGoBack => _webView.CanGoBack;

	public bool CanGoForward => _webView.CanGoForward;

	public Uri? Source
	{
		get => _webView.Source;
		set => _webView.Source = value;
	}

	public event EventHandler<WebViewNavigationEventArgs>? NavigationStarted;
	public event EventHandler<WebViewNavigationEventArgs>? NavigationCompleted;
	public event EventHandler? DOMContentLoaded;

	public void Dispose()
	{
		_webView.Dispose();
	}

	public bool GoBack()
	{
		_webView.GoBack();
		return true;
	}

	public bool GoForward()
	{
		_webView.GoForward();
		return true;
	}

	public async Task<string?> InvokeScriptAsync(string scriptName)
	{
		return await _webView.ExecuteScriptAsync(scriptName);
	}

	public void Navigate(Uri url)
	{
		_webView.Source = url;
	}

	public async Task NavigateToString(string text)
	{
		await _webView.EnsureCoreWebView2Async();

		_webView.NavigateToString(text);
	}

	public void Refresh()
	{
		_webView.Refresh();
	}

	public void Stop()
	{
		_webView.Stop();
	}

	public void HandleResize(int width, int height, float zoom)
	{
	}

	public bool HandleKeyDown(uint key, uint keyModifiers)
	{
		return false;
	}
}

internal class WebView2Handle : IPlatformHandle2
{
	public IntPtr Handle { get; init; }
	public string HandleDescriptor => "HWND";
}
