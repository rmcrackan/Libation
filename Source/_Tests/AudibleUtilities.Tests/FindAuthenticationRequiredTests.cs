using AssertionHelper;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AudibleUtilities.Tests;

/// <summary>
/// The scan wraps failures on the way up, so the exception that knows which account needs a login has to be dug
/// back out before the auto-scan dialog can name it.
/// </summary>
[TestClass]
public class FindAuthenticationRequiredTests
{
	[TestMethod]
	public void the_exception_itself_is_returned()
	{
		var auth = new AuthenticationRequiredException(new Account("user@example.com"));

		AuthenticationExceptionHelper.FindAuthenticationRequired(auth).Should().BeSameAs(auth);
	}

	[TestMethod]
	public void a_wrapped_exception_is_found()
	{
		var auth = new AuthenticationRequiredException(new Account("user@example.com"), "need login");
		var wrapped = new Exception("Error importing library", new Exception("inner", auth));

		AuthenticationExceptionHelper.FindAuthenticationRequired(wrapped).Should().BeSameAs(auth);
	}

	/// <summary>Scanning several accounts at once surfaces failures as an AggregateException.</summary>
	[TestMethod]
	public void an_aggregated_exception_is_found()
	{
		var auth = new AuthenticationRequiredException(new Account("user@example.com"), "need login");
		var aggregate = new AggregateException(new InvalidOperationException("unrelated"), auth);

		AuthenticationExceptionHelper.FindAuthenticationRequired(aggregate).Should().BeSameAs(auth);
	}

	[TestMethod]
	public void an_unrelated_exception_yields_nothing()
		=> AuthenticationExceptionHelper.FindAuthenticationRequired(new InvalidOperationException("unrelated"))
			.Should().BeNull();
}
