using System;
using System.Collections.ObjectModel;

// adapted from OpenQA.Selenium
// https://github.com/SeleniumHQ/selenium/blob/master/dotnet/src/webdriver/By.cs
namespace Scraping.Selectors
{
    /// <summary>Provides a mechanism by which to find elements within a document.</summary>
    [Serializable]
    internal partial class By
    {
        private string description { get; }

        /// <summary>Gets or sets the method used to find all elements matching specified criteria.</summary>
        public Func<WebElement, ReadOnlyCollection<WebElement>> FindElementsMethod { get; private set; }

        protected By(string description, Func<WebElement, ReadOnlyCollection<WebElement>> findElementsMethod)
        {
            this.description = description;
            FindElementsMethod = findElementsMethod;
        }

        public override string ToString() => description;
    }
}
