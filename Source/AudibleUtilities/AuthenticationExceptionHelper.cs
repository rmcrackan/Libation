using AudibleApi.Authentication;

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
			if (current is AuthenticationRequiredException or LoginFailedException)
				return true;

			if (current is InvalidOperationException { Message: var message }
				&& message.Contains("ADP token is null", StringComparison.Ordinal))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Finds the first <see cref="AuthenticationRequiredException"/> in <paramref name="ex"/> or its inner chain.
	/// </summary>
	public static AuthenticationRequiredException? FindAuthenticationRequired(Exception ex)
	{
		if (ex is AggregateException aggregate)
		{
			foreach (var inner in aggregate.InnerExceptions)
			{
				var found = FindAuthenticationRequired(inner);
				if (found is not null)
					return found;
			}
		}

		for (var current = ex; current is not null; current = current.InnerException)
		{
			if (current is AuthenticationRequiredException auth)
				return auth;
		}

		return null;
	}
}
