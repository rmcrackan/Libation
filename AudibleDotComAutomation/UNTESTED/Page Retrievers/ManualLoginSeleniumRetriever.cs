using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AudibleDotComAutomation
{
    /// <summary>online. get auth by logging in with provided username and password
    /// retain no chrome cookies. enter user + pw login</summary>
    public class ManualLoginSeleniumRetriever : AuthSeleniumRetriever
    {
        string _username;
        string _password;
        public ManualLoginSeleniumRetriever(string username, string password) : base()
        {
            _username = username;
            _password = password;
        }
        protected override async Task FirstRunAsync()
        {
            await base.FirstRunAsync();

            // can't extract this into AuthSeleniumRetriever ctor. can't use username/pw until prev ctors are complete

            // click login link
            await AudibleLinkClickAsync(getLoginLink());

            // wait until login page loads
            new WebDriverWait(Driver, TimeSpan.FromSeconds(60)).Until(ExpectedConditions.ElementIsVisible(By.Id("ap_email")));

            // insert credentials
            Driver
                .FindElement(By.Id("ap_email"))
                .SendKeys(_username);
            Driver
                .FindElement(By.Id("ap_password"))
                .SendKeys(_password);

            // submit
            var submitElement
                = Driver.FindElements(By.Id("signInSubmit")).FirstOrDefault()
                ?? Driver.FindElement(By.Id("signInSubmit-input"));
            await AudibleLinkClickAsync(submitElement);

            // wait until audible page loads
            new WebDriverWait(Driver, TimeSpan.FromSeconds(60))
                .Until(d => GetListenerPageLink());

            if (!IsLoggedIn)
                throw new Exception("not logged in");
        }
        private IWebElement getLoginLink()
        {
            {
                var loginLinkElements1 = Driver.FindElements(By.XPath("//a[contains(@href, '/signin')]"));
                if (loginLinkElements1.Any())
                    return loginLinkElements1[0];
            }

            //
            // ADD ADDITIONAL ACCEPTABLE PATTERNS HERE
            //
            //{
            //    var loginLinkElements2 = Driver.FindElements(By.XPath("//a[contains(@href, '/signin')]"));
            //    if (loginLinkElements2.Any())
            //        return loginLinkElements2[0];
            //}

            throw new NotFoundException("Cannot locate login link");
        }
    }
}
