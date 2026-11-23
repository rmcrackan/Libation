using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace LibationUiBase;

public sealed record DescriptionRun(string Text, bool Bold, bool Italic);

public static partial class DescriptionMarkup
{
	[GeneratedRegex(@"<\s*(/?)\s*([a-zA-Z][a-zA-Z0-9]*)[^>]*>")]
	private static partial Regex TagRegex();

	[GeneratedRegex(@"[ \t ]+")]
	private static partial Regex SpaceRegex();

	/// <summary>Paragraphs of styled runs; unknown tags are dropped, their text kept.</summary>
	public static IReadOnlyList<IReadOnlyList<DescriptionRun>> Parse(string? html)
	{
		var paragraphs = new List<IReadOnlyList<DescriptionRun>>();
		if (string.IsNullOrWhiteSpace(html))
			return paragraphs;

		var current = new List<DescriptionRun>();
		int bold = 0, italic = 0;

		void Break()
		{
			var done = Finish(current);
			if (done.Count > 0)
				paragraphs.Add(done);
			current = [];
		}

		void AddText(string raw)
		{
			var parts = WebUtility.HtmlDecode(raw).Split('\n', '\r');
			for (var i = 0; i < parts.Length; i++)
			{
				if (i > 0)
					Break();
				if (parts[i].Length > 0)
					current.Add(new DescriptionRun(parts[i], bold > 0, italic > 0));
			}
		}

		if (!html.Contains('<'))
			AddText(html);
		else
		{
			var pos = 0;
			foreach (Match m in TagRegex().Matches(html))
			{
				AddText(html[pos..m.Index]);
				pos = m.Index + m.Length;

				var closing = m.Groups[1].Length > 0;
				switch (m.Groups[2].Value.ToLowerInvariant())
				{
					case "b" or "strong":
						bold = closing ? Math.Max(0, bold - 1) : bold + 1;
						break;
					case "i" or "em":
						italic = closing ? Math.Max(0, italic - 1) : italic + 1;
						break;
					case "br":
						Break();
						break;
					case "p" or "div" or "li" or "blockquote" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" when closing:
						Break();
						break;
				}
			}
			AddText(html[pos..]);
		}

		Break();
		return paragraphs;
	}

	private static List<DescriptionRun> Finish(List<DescriptionRun> raw)
	{
		var runs = new List<DescriptionRun>();
		foreach (var run in raw)
		{
			var text = SpaceRegex().Replace(run.Text, " ");
			if (runs.Count > 0 && runs[^1].Text.EndsWith(' '))
				text = text.TrimStart(' ');
			if (text.Length == 0)
				continue;

			if (runs.Count == 0)
			{
				text = text.TrimStart(' ');
				if (text.Length == 0)
					continue;
			}

			if (text == " ")
				runs[^1] = runs[^1] with { Text = runs[^1].Text + " " };
			else if (runs.Count > 0 && runs[^1].Bold == run.Bold && runs[^1].Italic == run.Italic)
				runs[^1] = runs[^1] with { Text = runs[^1].Text + text };
			else
				runs.Add(run with { Text = text });
		}

		while (runs.Count > 0)
		{
			var trimmed = runs[^1].Text.TrimEnd(' ');
			if (trimmed.Length > 0)
			{
				runs[^1] = runs[^1] with { Text = trimmed };
				break;
			}
			runs.RemoveAt(runs.Count - 1);
		}
		return runs;
	}
}
