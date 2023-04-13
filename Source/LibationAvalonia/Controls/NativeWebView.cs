using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia;
using LibationFileManager;
using System;
using System.Threading.Tasks;

namespace LibationAvalonia.Controls;

#nullable enable
public class NativeWebView : NativeControlHost, IWebView
{
	private IWebViewAdapter? _webViewAdapter;
	private Uri? _delayedSource;
	private TaskCompletionSource _webViewReadyCompletion = new();

	public event EventHandler<WebViewNavigationEventArgs>? NavigationCompleted;

	public event EventHandler<WebViewNavigationEventArgs>? NavigationStarted;
	public event EventHandler? DOMContentLoaded;

	public bool CanGoBack => _webViewAdapter?.CanGoBack ?? false;

	public bool CanGoForward => _webViewAdapter?.CanGoForward ?? false;

	public Uri? Source
	{
		get => _webViewAdapter?.Source ?? throw new InvalidOperationException("Control was not initialized");
		set
		{
			if (_webViewAdapter is null)
			{
				_delayedSource = value;
				return;
			}
			_webViewAdapter.Source = value;
		}
	}


	public bool GoBack()
	{
		return _webViewAdapter?.GoBack() ?? throw new InvalidOperationException("Control was not initialized");
	}

	public bool GoForward()
	{
		return _webViewAdapter?.GoForward() ?? throw new InvalidOperationException("Control was not initialized");
	}

	public Task<string?> InvokeScriptAsync(string scriptName)
	{
		return _webViewAdapter is null
			? throw new InvalidOperationException("Control was not initialized")
			: _webViewAdapter.InvokeScriptAsync(scriptName);
	}

	public void Navigate(Uri url)
	{
		(_webViewAdapter ?? throw new InvalidOperationException("Control was not initialized"))
			.Navigate(url);
	}

	public Task NavigateToString(string text)
	{
		return (_webViewAdapter ?? throw new InvalidOperationException("Control was not initialized"))
			.NavigateToString(text);
	}

	public void Refresh()
	{
		(_webViewAdapter ?? throw new InvalidOperationException("Control was not initialized"))
			.Refresh();
	}

	public void Stop()
	{
		(_webViewAdapter ?? throw new InvalidOperationException("Control was not initialized"))
			.Stop();
	}

	public Task WaitForNativeHost()
	{
		return _webViewReadyCompletion.Task;
	}

	private class PlatformHandle : IPlatformHandle
	{
		public nint Handle { get; init; }

		public string? HandleDescriptor { get; init; }
	}

	protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
	{
		_webViewAdapter = InteropFactory.Create().CreateWebViewAdapter();

		if (_webViewAdapter is null)
			return base.CreateNativeControlCore(parent);
		else
		{
			SubscribeOnEvents();
			var handle = new PlatformHandle
			{
				Handle = _webViewAdapter.PlatformHandle.Handle,
				HandleDescriptor = _webViewAdapter.PlatformHandle.HandleDescriptor
			};

			if (_delayedSource is not null)
			{
				_webViewAdapter.Source = _delayedSource;
			}

			_webViewReadyCompletion.TrySetResult();

			return handle;
		}
	}

	private void SubscribeOnEvents()
	{
		if (_webViewAdapter is not null)
		{
			_webViewAdapter.NavigationStarted += WebViewAdapterOnNavigationStarted;
			_webViewAdapter.NavigationCompleted += WebViewAdapterOnNavigationCompleted;
			_webViewAdapter.DOMContentLoaded += _webViewAdapter_DOMContentLoaded;
		}
	}

	private void _webViewAdapter_DOMContentLoaded(object? sender, EventArgs e)
	{
		DOMContentLoaded?.Invoke(this, e);
	}

	private void WebViewAdapterOnNavigationStarted(object? sender, WebViewNavigationEventArgs e)
	{
		NavigationStarted?.Invoke(this, e);
	}

	private void WebViewAdapterOnNavigationCompleted(object? sender, WebViewNavigationEventArgs e)
	{
		NavigationCompleted?.Invoke(this, e);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);
		if (change.Property == BoundsProperty && change.NewValue is Rect rect)
		{
			var scaling = (float)(VisualRoot?.RenderScaling ?? 1.0f);
			_webViewAdapter?.HandleResize((int)(rect.Width * scaling), (int)(rect.Height * scaling), scaling);
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (_webViewAdapter != null)
		{
			e.Handled = _webViewAdapter.HandleKeyDown((uint)e.Key, (uint)e.KeyModifiers);
		}

		base.OnKeyDown(e);
	}

	protected override void DestroyNativeControlCore(IPlatformHandle control)
	{
		if (_webViewAdapter is not null)
		{
			_webViewReadyCompletion = new TaskCompletionSource();
			_webViewAdapter.NavigationStarted -= WebViewAdapterOnNavigationStarted;
			_webViewAdapter.NavigationCompleted -= WebViewAdapterOnNavigationCompleted;
			(_webViewAdapter as IDisposable)?.Dispose();
		}
	}
}
