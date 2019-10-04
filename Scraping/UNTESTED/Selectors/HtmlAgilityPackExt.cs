using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HtmlAgilityPack;

namespace Scraping.Selectors
{
    internal static class HtmlAgilityPackExt
    {
        public static ReadOnlyCollection<WebElement> ToReadOnlyCollection(this IEnumerable<HtmlNode> nodeCollection)
            => (nodeCollection ?? new List<HtmlNode>())
                .Select(n => new WebElement(n) as WebElement)
                .ToList()
                .AsReadOnly();
    }
}
