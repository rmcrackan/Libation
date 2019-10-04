using System;
using System.Collections.ObjectModel;

namespace Scraping.Selectors
{
    // example custom "By" locator. from: https://stackoverflow.com/questions/14263483
    internal class CustomSelector : By
    {
        public CustomSelector(string description, Func<WebElement, ReadOnlyCollection<WebElement>> findElementsMethod) : base(description, findElementsMethod) { }
        public static By Image(string imageBySource)
        {
            if (imageBySource == null)
                throw new ArgumentNullException(nameof(imageBySource), "Cannot find elements when image string is null.");

            return new CustomSelector(
                nameof(CustomSelector) + ".Image: " + imageBySource,
                (context) => context.FindElements(XPath("//img[@src='" + imageBySource + "']"))
                );
        }
    }
}
