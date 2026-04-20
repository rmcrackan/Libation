using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace FileManager.NamingTemplate;

public static class RegExpExtensions
{
	extension(Group group)
	{
		public string? ValueOrNull() => group.Success ? group.Value : null;
		public string? UnescapeValueOrNull() => group.Success ? CommonFormatters.Unescape(group.ValueSpan, []) : null;
		public string UnescapeValue() => group.Success ? CommonFormatters.Unescape(group.ValueSpan, []) : string.Empty;
		public ReadOnlySpan<char> ValueSpanOrNull() => group.Success ? group.ValueSpan : null;
	}

	extension(Match match)
	{
		public Group Resolve(string? groupName = null)
		{
			if (groupName is not null && match.Groups.TryGetValue(groupName, out var group))
				return group;
			return match.Groups.Count > 1 ? match.Groups[1] : match.Groups[0];
		}

		public string? ResolveValue(string? groupName = null) => match.Resolve(groupName).ValueOrNull();

		public bool TryParseInt(string? groupName, out int value)
		{
			var span = match.Resolve(groupName).ValueSpanOrNull();

			return int.TryParse(span, out value);
		}

		public string UnescapeValue(string? groupName = null) => match.Resolve(groupName).UnescapeValue();
		public string? UnescapeValueOrNull(string? groupName = null) => match.Resolve(groupName).UnescapeValueOrNull();
	}

	extension(Regex regex)
	{
		public bool TryMatch(string input, [NotNullWhen(true)] out Match? match)
		{
			var m = regex.Match(input);
			match = m.Success ? m : null;
			return m.Success;
		}

		public string ReplaceWithGaps(string input, MatchEvaluator matchEvaluator, Func<string, string> gapEvaluator)
		{
			var sb = new StringBuilder(input.Length);
			var pos = 0;

			foreach (Match m in regex.Matches(input))
			{
				// part before match
				if (m.Index > pos)
					sb.Append(gapEvaluator(input[pos .. m.Index]));

				// the match itself
				sb.Append(matchEvaluator(m));

				pos = m.Index + m.Length;
			}

			// part after last match
			if (pos < input.Length)
				sb.Append(gapEvaluator(input[pos..]));

			return sb.ToString();
		}
	}
}