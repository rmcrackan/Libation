using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace AudibleDotComAutomation
{
    public static class IWebElementExt
    {
        // allows getting Text from elements even if hidden
        // this only works on visible elements: webElement.Text
        // http://yizeng.me/2014/04/08/get-text-from-hidden-elements-using-selenium-webdriver/#c-sharp
        //
        public static string GetText(this IWebElement webElement) => webElement.GetAttribute("textContent");

        public static string GetValue(this IWebElement webElement) => webElement.GetAttribute("value");
    }

    public static class IWebDriverExt
    {
        /// <summary>Use this instead of element.Click() to ensure that the element is clicked even if it's not currently scrolled into view</summary>
        public static void Click(this IWebDriver driver, IWebElement element)
        {
            // from: https://stackoverflow.com/questions/12035023/selenium-webdriver-cant-click-on-a-link-outside-the-page


            //// this works but isn't really the same
            //element.SendKeys(Keys.Enter);


            //// didn't work for me
            //new Actions(driver)
            //    .MoveToElement(element)
            //    .Click()
            //    .Build()
            //    .Perform();

            driver.ScrollIntoView(element);
            element.Click();
        }
        public static void ScrollIntoView(this IWebDriver driver, IWebElement element)
            => ((IJavaScriptExecutor)driver).ExecuteScript($"window.scroll({element.Location.X}, {element.Location.Y})");
    }
}
