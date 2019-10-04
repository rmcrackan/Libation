using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AudibleDotComAutomation.Examples
{
    public class SeleniumExamples
    {
        public IWebDriver Driver { get; set; }

        IWebElement GetListenerPageLink()
        {
            var listenerPageElement = Driver.FindElements(By.XPath("//a[contains(@href, '/review-by-author')]"));
            if (listenerPageElement.Count > 0)
                return listenerPageElement[0];
            return null;
        }
        void wait_examples()
        {
            new WebDriverWait(Driver, TimeSpan.FromSeconds(60))
                .Until(ExpectedConditions.ElementIsVisible(By.Id("mast-member-acct-name")));

            new WebDriverWait(Driver, TimeSpan.FromSeconds(60))
                .Until(d => GetListenerPageLink());

            // https://stackoverflow.com/questions/21339339/how-to-add-custom-expectedconditions-for-selenium
            new WebDriverWait(Driver, TimeSpan.FromSeconds(60))
                .Until((d) =>
                {
                    // could be refactored into OR, AND per the java selenium library

                    // check 1
                    var e1 = Driver.FindElements(By.Id("mast-member-acct-name"));
                    if (e1.Count > 0)
                        return e1[0];
                    // check 2
                    var e2 = Driver.FindElements(By.Id("header-account-info-0"));
                    if (e2.Count > 0)
                        return e2[0];
                    return null;
                });
        }
        void XPath_examples()
        {
            // <tr>
            //   <td>1</td>
            //   <td>2</td>
            // </tr>
            // <tr>
            //   <td>3</td>
            //   <td>4</td>
            // </tr>

            ReadOnlyCollection<IWebElement> all_tr = Driver.FindElements(By.XPath("/tr"));
            IWebElement first_tr = Driver.FindElement(By.XPath("/tr"));
            IWebElement second_tr = Driver.FindElement(By.XPath("/tr[2]"));
            // beginning with a single / starts from root
            IWebElement ERROR_not_at_root = Driver.FindElement(By.XPath("/td"));
            // 2 slashes searches all, NOT just descendants
            IWebElement td1 = Driver.FindElement(By.XPath("//td"));

            // 2 slashes still searches all, NOT just descendants
            IWebElement still_td1 = first_tr.FindElement(By.XPath("//td"));
            
            // dot operator starts from current node specified by first_tr
            // single slash: immediate descendant
            IWebElement td3 = first_tr.FindElement(By.XPath(
                ".//td"));
            // double slash: descendant at any depth
            IWebElement td3_also = first_tr.FindElement(By.XPath(
                "./td"));

            // <input type="hidden" name="asin" value="ABCD1234">
            IWebElement find_anywhere_in_doc = first_tr.FindElement(By.XPath(
                "//input[@name='asin']"));
            IWebElement find_in_subsection = first_tr.FindElement(By.XPath(
                ".//input[@name='asin']"));

            // search entire page. useful for:
            // - RulesLocator to find something that only appears once on the page
            // - non-list pages. eg: product details
            var onePerPageRules = new RuleFamily
            {
                RowsLocator = By.XPath("/*"), // search entire page
                Rules = new RuleSet {
                    (row, productItem) => productItem.CustomerId = row.FindElement(By.XPath("//input[@name='cust_id']")).GetValue(),
                    (row, productItem) => productItem.UserName = row.FindElement(By.XPath("//input[@name='user_name']")).GetValue()
                }
            };
            // - applying conditionals to entire page
            var ruleFamily = new RuleFamily
            {
                RowsLocator = By.XPath("//*[starts-with(@id,'adbl-library-content-row-')]"),
                // Rules = getRuleSet()
            };
        }
        #region Rules classes stubs
        public class RuleFamily { public By RowsLocator; public IRuleClass Rules; }
        public interface IRuleClass { }
        public class RuleSet : IRuleClass, IEnumerable<IRuleClass>
        {
            public void Add(IRuleClass ruleClass) { }
            public void Add(RuleAction action) { }

            public IEnumerator<IRuleClass> GetEnumerator() => throw new NotImplementedException();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
        }
        public delegate void RuleAction(IWebElement row, ProductItem productItem);
        public class ProductItem { public string CustomerId; public string UserName; }
        #endregion
    }
}
