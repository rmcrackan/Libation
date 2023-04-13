/* Work-in-progress
 * 
 * 
using LibationFileManager;
using ObjCRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebKit;

#nullable enable

namespace MacOSConfigApp;

internal class WKNavigationDelegate1 : WKNavigationDelegate
{
	public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
	{
		base.DidStartProvisionalNavigation(webView, navigation);
	}
}
internal class MacWebViewAdapter : IWebViewAdapter, IDisposable
{
	private readonly WKWebView _webView;
	public IPlatformHandle2 PlatformHandle { get; }

	public bool CanGoBack => _webView.CanGoBack;

	public bool CanGoForward => _webView.CanGoForward;

	public Uri? Source { get => _webView?.Url; set => throw new NotImplementedException(); }

	public object NativeWebView { get; }

	public event EventHandler<WebViewNavigationEventArgs>? NavigationCompleted;
	public event EventHandler<WebViewNavigationEventArgs>? NavigationStarted;
	public event EventHandler? DOMContentLoaded;

	WKNavigationDelegate1 navDelegate;
	public MacWebViewAdapter()
	{
		var frame = new CGRect(0, 0, 500, 800);
		NativeWebView = _webView = new WKWebView(frame, new WKWebViewConfiguration());
		_webView.NavigationDelegate = navDelegate = new WKNavigationDelegate1();
		PlatformHandle = new MacViewHandle(_webView.Handle);
	}

	public void Dispose()
	{
		_webView?.Dispose();
	}

	public bool GoBack()
	{
		if (_webView.CanGoBack)
		{
			_webView.GoBack();
			return true;
		}
		else return false;
	}

	public bool GoForward()
	{
		if (_webView.CanGoForward)
		{
			_webView.GoForward();
			return true;
		}
		else return false;
	}

	public bool HandleKeyDown(uint key, uint keyModifiers)
	{
		return false;
	}

	public void HandleResize(int width, int height, float zoom)
	{

	}

	public async Task<string?> InvokeScriptAsync(string scriptName)
	{
		var result = await _webView.EvaluateJavaScriptAsync(scriptName);

		return result.ToString();
	}

	public void Navigate(Uri url)
	{
		NSUrl? nsurl = url;
		if (nsurl is null) return;

		var request = new NSUrlRequest(nsurl);

		_webView.LoadRequest(request);

	}


	public Task NavigateToString(string text)
	{
		throw new NotImplementedException();
	}

	public void Refresh()
	{
		throw new NotImplementedException();
	}

	public void Stop()
	{
		throw new NotImplementedException();
	}
}


internal class MacViewHandle : IPlatformHandle2
{
	private NativeHandle? _view;

	public MacViewHandle(NativeHandle view)
	{
		_view = view;
	}

	public nint Handle => _view?.Handle ?? 0;
	public string HandleDescriptor => "NativeHandle";
}

*/