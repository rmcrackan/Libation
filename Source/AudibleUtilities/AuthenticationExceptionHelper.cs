namespace AudibleUtilities;

public static class AuthenticationExceptionHelper
{
	public static bool IsAuthenticationFailure(Exception ex)
	{
		if (ex is AggregateException aggregate)
		{
			return aggregate.InnerExceptions.Any(IsAuthenticationFailure)
				|| (aggregate.InnerException is not null && IsAuthenticationFailure(aggregate.InnerException));
		}

		for (var current = ex; current is not null; current = current.InnerException)
		{
			if (current is AuthenticationRequiredException)
				return true;

			if (current is InvalidOperationException { Message: var message }
				&& message.Contains("ADP token is null", StringComparison.Ordinal))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Finds the <see cref="AuthenticationRequiredException"/> in <paramref name="ex"/> or its inner chain, which is
	/// the one that knows which account needs a login.
	/// </summary>
	public static AuthenticationRequiredException? FindAuthenticationRequired(Exception ex)
	{
		for (var current = ex; current is not null; current = current.InnerException)
		{
			if (current is AuthenticationRequiredException auth)
				return auth;
		}

		if (ex is AggregateException aggregate)
		{
			foreach (var inner in aggregate.InnerExceptions)
			{
				if (FindAuthenticationRequired(inner) is { } found)
					return found;
			}
		}

		return null;
	}
}
