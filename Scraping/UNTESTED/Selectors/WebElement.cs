using System.Collections.ObjectModel;
using System.Linq;
using HtmlAgilityPack;

namespace Scraping.Selectors
{
    internal class WebElement
    {
        public HtmlNode Node { get; }
        public WebElement(HtmlNode htmlNode) => Node = htmlNode;

        public ReadOnlyCollection<WebElement> FindElements(By by) => by.FindElementsMethod(this);

        /// <summary>Finds the single element matching the criteria.</summary>
        public WebElement FindElement(By by) => FindElements(by).Single();

        public string GetAttribute(string attributeName) => Node?.Attributes[attributeName]?.Value;
        public string Text => System.Net.WebUtility.HtmlDecode(Node.InnerText);
        public string Value => GetAttribute("value");
    }
}
