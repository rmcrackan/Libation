using LibationUiBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.CompilerServices;

namespace LibationUiBase.Tests;

[TestClass]
public class WebView2LoginErrorMessageTests
{
	[TestMethod]
	public void missing_webkit2gtk_dll_is_recognized_without_stack_trace()
	{
		var ex = new DllNotFoundException("Unable to load shared library 'libwebkit2gtk-4.1.so.0'");

		Assert.IsTrue(WebView2LoginErrorMessage.IsWebView2SignInInfrastructureFailure(ex));
	}

	[TestMethod]
	public void missing_gtk3_dll_is_recognized_without_stack_trace()
	{
		var ex = new DllNotFoundException("Unable to load shared library 'libgtk-3.so.0'");

		Assert.IsTrue(WebView2LoginErrorMessage.IsWebView2SignInInfrastructureFailure(ex));
	}

	[TestMethod]
	public void gtk_initialization_failure_is_recognized_without_stack_trace()
	{
		var ex = new InvalidOperationException("Unable to initialize GTK");

		Assert.IsTrue(WebView2LoginErrorMessage.IsWebView2SignInInfrastructureFailure(ex));
	}

	[TestMethod]
	public void invalid_operation_from_avalonia_gtk_webview_stack_is_recognized()
	{
		Exception? caught = null;
		try
		{
			FakeAvaloniaGtkStack.ThrowFromGtkNativeWebViewDialog();
		}
		catch (Exception ex)
		{
			caught = ex;
		}

		Assert.IsNotNull(caught);
		Assert.IsTrue(WebView2LoginErrorMessage.IsWebView2SignInInfrastructureFailure(caught));
		var agg = new AggregateException(new Exception("outer"), caught);
		Assert.IsTrue(WebView2LoginErrorMessage.TryFindInTree(agg, out var match));
		Assert.IsNotNull(match);
	}

	[TestMethod]
	public void unrelated_invalid_operation_is_not_treated_as_webview_failure()
	{
		var ex = new InvalidOperationException("Something else went wrong");

		Assert.IsFalse(WebView2LoginErrorMessage.IsWebView2SignInInfrastructureFailure(ex));
	}

	private static class FakeAvaloniaGtkStack
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void ThrowFromGtkNativeWebViewDialog()
			=> throw new InvalidOperationException("Avalonia.X11 is not referenced");
	}
}
