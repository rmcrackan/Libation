using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace FileManager.NamingTemplate;

public static class RegExpExtensions
{
	extension(Group group)
	{
		public string? ValueOrNull() => group.Success ? group.Value : null;
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
	}

	extension(Regex regex)
	{
		public bool TryMatch(string input, [NotNullWhen(true)] out Match? match)
		{
			var m = regex.Match(input);
			match = m.Success ? m : null;
			return m.Success;
		}
	}
}