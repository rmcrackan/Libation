using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace AudibleDotComAutomation
{
    /// <summary>for user collections: lib, WL</summary>
    public abstract class AuthSeleniumRetriever : SeleniumRetriever
    {
        protected bool IsLoggedIn => GetListenerPageLink() != null;

        // needed?
        protected AuthSeleniumRetriever() : base() { }

        protected IWebElement GetListenerPageLink()
        {
            var listenerPageElement = Driver.FindElements(By.XPath("//a[contains(@href, '/review-by-author')]"));
            if (listenerPageElement.Count > 0)
                return listenerPageElement[0];
            return null;
        }
    }
}
