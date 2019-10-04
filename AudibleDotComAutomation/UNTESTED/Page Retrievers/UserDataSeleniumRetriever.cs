using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;

namespace AudibleDotComAutomation
{
    /// <summary>online. load auth, cookies etc from user data</summary>
    public class UserDataSeleniumRetriever : AuthSeleniumRetriever
    {
        public UserDataSeleniumRetriever() : base()
        {
            // can't extract this into AuthSeleniumRetriever ctor. can't use username/pw until prev ctors are complete
            if (!IsLoggedIn)
                throw new Exception("not logged in");
        }

        /// <summary>Use current user data/chrome cookies. DO NOT use if chrome is already open</summary>
        protected override ChromeOptions ctorCreateChromeOptions()
        {
            var options = base.ctorCreateChromeOptions();

            // load user data incl cookies. default on windows:
            //   %LOCALAPPDATA%\Google\Chrome\User Data
            //   C:\Users\username\AppData\Local\Google\Chrome\User Data
            var chromeDefaultWindowsUserDataDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Google",
                "Chrome",
                "User Data");
            options.AddArguments($"user-data-dir={chromeDefaultWindowsUserDataDir}");

            return options;
        }
    }
}
