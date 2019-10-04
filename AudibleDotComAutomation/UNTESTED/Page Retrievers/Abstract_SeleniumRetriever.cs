using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudibleDotCom;
using Dinah.Core.Humanizer;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace AudibleDotComAutomation
{
    /// <summary>browser manipulation. web driver access
    /// browser operators. create and store web driver, browser navigation which can vary depending on whether anon or auth'd
    /// 
    /// this base class: is online. no auth. used for most pages. retain no chrome cookies</summary>
    public abstract class SeleniumRetriever : IPageRetriever
    {
        #region // chrome driver details
        /*
           HIDING CHROME CONSOLE WINDOW
           hiding chrome console window has proven to cause more headaches than it solves. here's how to do it though:
               // can also use CreateDefaultService() overloads to specify driver path and/or file name
               var chromeDriverService = ChromeDriverService.CreateDefaultService();
               chromeDriverService.HideCommandPromptWindow = true;
               return new ChromeDriver(chromeDriverService, options);

           HEADLESS CHROME
           this WOULD be how to do headless. but amazon/audible are far too tricksy about their changes and anti-scraping measures
           which renders 'headless' mode useless
               var options = new ChromeOptions();
               options.AddArgument("--headless");

           SPECIFYING DRIVER LOCATION
           if continues to have trouble finding driver:
               var driver = new ChromeDriver(@"C:\my\path\to\chromedriver\directory");
               var chromeDriverService = ChromeDriverService.CreateDefaultService(@"C:\my\path\to\chromedriver\directory");
         */
        #endregion

        protected IWebDriver Driver { get; }
        Humanizer humanizer { get; } = new Humanizer();

        protected SeleniumRetriever()
        {
            Driver = new ChromeDriver(ctorCreateChromeOptions());
        }

        /// <summary>no auth. retain no chrome cookies</summary>
        protected virtual ChromeOptions ctorCreateChromeOptions() => new ChromeOptions();

        protected async Task AudibleLinkClickAsync(IWebElement element)
        {
            // EACH CALL to audible should have a small random wait to reduce chances of scrape detection
            await humanizer.Wait();

            await Task.Run(() => Driver.Click(element));

            await waitForSpinnerAsync();

            // sometimes these clicks just take a while. add a few more seconds
            await Task.Delay(5000);
        }

        By spinnerLocator { get; } = By.Id("library-main-overlay");
        private async Task waitForSpinnerAsync()
        {
            // if loading overlay w/spinner exists: pause, wait for it to end

            await Task.Delay(100);

            if (Driver.FindElements(spinnerLocator).Count > 0)
                new WebDriverWait(Driver, TimeSpan.FromSeconds(60))
                    .Until(ExpectedConditions.InvisibilityOfElementLocated(spinnerLocator));
        }

        private bool isFirstRun = true;
        protected virtual async Task FirstRunAsync()
        {
            // load with no beginning wait. then wait 7 seconds to allow for page flicker. it usually happens after ~5 seconds. can happen irrespective of login state
            await Task.Run(() => Driver.Navigate().GoToUrl("http://www.audible.com/"));
            await Task.Delay(7000);
        }

        public async Task<IEnumerable<AudiblePageSource>> GetPageSourcesAsync(AudiblePageType audiblePage, string pageId = null)
        {
            if (isFirstRun)
            {
                await FirstRunAsync();
                isFirstRun = false;
            }

            await initFirstPageAsync(audiblePage, pageId);

            return await processUrl(audiblePage, pageId);
        }

        private async Task initFirstPageAsync(AudiblePageType audiblePage, string pageId)
        {
            // EACH CALL to audible should have a small random wait to reduce chances of scrape detection
            await humanizer.Wait();

            var url = audiblePage.GetAudiblePageRobust().GetUrl(pageId);
            await Task.Run(() => Driver.Navigate().GoToUrl(url));

            await waitForSpinnerAsync();
        }

        private async Task<IEnumerable<AudiblePageSource>> processUrl(AudiblePageType audiblePage, string pageId)
        {
            var pageSources = new List<AudiblePageSource>();
            do
            {
                pageSources.Add(new AudiblePageSource(audiblePage, Driver.PageSource, pageId));
            }
            while (await hasMorePagesAsync());

            return pageSources;
        }

        #region has more pages
        /// <summary>if no more pages, return false. else, navigate to next page and return true</summary>
        private async Task<bool> hasMorePagesAsync()
        {
            var next = //old_hasMorePages() ??
                new_hasMorePages();
            if (next == null)
                return false;

            await AudibleLinkClickAsync(next);
            return true;
        }

        private IWebElement old_hasMorePages()
        {
            var parentElements = Driver.FindElements(By.ClassName("adbl-page-next"));
            if (parentElements.Count == 0)
                return null;

            var childElements = parentElements[0].FindElements(By.LinkText("NEXT"));
            if (childElements.Count != 1)
                return null;

            return childElements[0];
        }

        // ~ oct 2017
        private IWebElement new_hasMorePages()
        {
            // get all active/enabled navigation links
            var pageNavLinks = Driver.FindElements(By.ClassName("library-load-page"));
            if (pageNavLinks.Count == 0)
                return null;

            // get only the right chevron if active.
            // note: there are also right chevrons which are not for wish list navigation which is why we first filter by library-load-page
            var nextLink = pageNavLinks
                .Where(p => p.FindElements(By.ClassName("bc-icon-chevron-right")).Count > 0)
                .ToList(); // cut-off delayed execution
            if (nextLink.Count == 0)
                return null;

            return nextLink.Single().FindElement(By.TagName("button"));
        }
        #endregion

        #region IDisposable pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && Driver != null)
            {
                // Quit() does cleanup AND disposes
                Driver.Quit();
            }
        }
        #endregion
    }
}
