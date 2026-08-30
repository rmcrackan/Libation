using System.Net;
using System.Text.RegularExpressions;

namespace AudibleUtilities;

/// <summary>
/// Audible returns a book's summary as HTML. Nothing downstream of the API renders markup - not the
/// library grid, not the exports, and least of all the metadata tags written into an audio file - so
/// the markup is flattened here, on the way in, rather than carried around and stripped at each use.
/// </summary>
public static partial class HtmlText
{
    /// <param name="paragraphSeparator">Joins the block-level runs. A single newline matches what
    /// Audible itself embeds in the description tags of its .aaxc files.</param>
    public static string ToPlainText(string? html, string paragraphSeparator = "\n")
    {
        if (string.IsNullOrWhiteSpace(html))
            return "";

        // Not every summary is marked up. Running the unmarked ones through the parser risks a stray
        // '<' swallowing the rest of the sentence, and buys nothing.
        if (!html.Contains('<'))
            return (WebUtility.HtmlDecode(html) ?? html).Trim();

        // Replace block-level boundaries with newlines, then strip all remaining tags.
        var stripped = StripTags().Replace(BlockBoundary().Replace(html, "\n"), "");

        var text = WebUtility.HtmlDecode(stripped) ?? stripped;

        var paragraphs = text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0);

        return string.Join(paragraphSeparator, paragraphs);
    }

    /// <summary>Every tag that ends a run of text. A line break counts as a paragraph break because
    /// Audible's summaries mark up paragraphs and nothing finer.</summary>
    [GeneratedRegex(@"<\s*br\s*/?\s*>|<\s*/\s*(?:p|div|li|tr|blockquote|h[1-6])\s*>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockBoundary();

    /// <summary>Matches any HTML/XML tag.</summary>
    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex StripTags();
}
