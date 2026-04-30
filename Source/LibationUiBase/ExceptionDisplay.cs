using System;
using System.Collections.Generic;
using System.Text;

namespace LibationUiBase;

/// <summary>Formats exceptions for read-only UI (crash dialog, etc.).</summary>
public static class ExceptionDisplay
{
	const int FirstInnerCount = 10;
	const int TailInnerCount = 2;
	/// <summary>Cap when walking <see cref="Exception.InnerException"/> (pathological chains).</summary>
	const int MaxInnerCollect = 1000;

	/// <summary>
	/// Primary message, nested inner messages (first 10, then optional omission count, then deepest 2 when the chain is longer than 10), then the outer stack trace.
	/// </summary>
	public static string FormatMessageAndStackTrace(Exception exception)
	{
		var sb = new StringBuilder();
		sb.AppendLine(exception.Message);

		var inners = new List<Exception>();
		for (var inner = exception.InnerException; inner is not null && inners.Count < MaxInnerCollect; inner = inner.InnerException)
			inners.Add(inner);

		var n = inners.Count;
		if (n > 0)
		{
			if (n <= FirstInnerCount)
			{
				foreach (var ex in inners)
					AppendInner(sb, ex);
			}
			else
			{
				for (var i = 0; i < FirstInnerCount; i++)
					AppendInner(sb, inners[i]);

				var omitted = n - FirstInnerCount - TailInnerCount;
				if (omitted > 0)
				{
					sb.AppendLine();
					sb.Append(omitted);
					sb.Append(" inner exception");
					if (omitted != 1)
						sb.Append('s');
					sb.AppendLine(" omitted.");
				}

				var startTail = Math.Max(FirstInnerCount, n - TailInnerCount);
				for (var i = startTail; i < n; i++)
					AppendInner(sb, inners[i]);
			}
		}

		sb.AppendLine();
		sb.Append(exception.StackTrace);
		return sb.ToString();
	}

	private static void AppendInner(StringBuilder sb, Exception ex)
	{
		sb.AppendLine();
		sb.Append("Inner exception: ");
		sb.AppendLine(ex.Message);
	}
}
