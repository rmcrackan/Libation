using System;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AudibleUtilities.Tests;

[TestClass]
public class AuthenticationExceptionHelperTests
{
	[TestMethod]
	public void FindAuthenticationRequired_finds_nested()
	{
		var account = new Account("user@example.com");
		var inner = new AuthenticationRequiredException(account, "need login");
		var outer = new Exception("wrapper", inner);

		var found = AuthenticationExceptionHelper.FindAuthenticationRequired(outer);
		Assert.AreSame(inner, found);
	}

	[TestMethod]
	public void FindAuthenticationRequired_returns_null_when_absent()
	{
		var ex = new InvalidOperationException("unrelated");
		Assert.IsNull(AuthenticationExceptionHelper.FindAuthenticationRequired(ex));
	}

	[TestMethod]
	public void IsAuthenticationFailure_detects_AuthenticationRequiredException()
	{
		var ex = new AuthenticationRequiredException(new Account("user@example.com"));
		Assert.IsTrue(AuthenticationExceptionHelper.IsAuthenticationFailure(ex));
	}
}
