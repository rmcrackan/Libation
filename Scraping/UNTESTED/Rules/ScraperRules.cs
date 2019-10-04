using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scraping.Selectors;

namespace Scraping.Rules
{
    /// <summary>not the same as LocatedRuleSet. IRuleClass only acts upon 1 product item at a time. RuleFamily returns many product items</summary>
    internal class RuleFamily<T>
    {
        public By RowsLocator;
        public IRuleClass<T> Rules;
        public IEnumerable<WebElement> GetRows(WebElement rootWebElement)
            => rootWebElement.FindElements(RowsLocator).ToList();
    }

    internal interface IRuleClass<T> { void Run(WebElement element, T productItem); }

    internal class BasicRule<T> : IRuleClass<T>
    {
        public Action<WebElement, T> Action;

        public BasicRule() { }
        public BasicRule(Action<WebElement, T> action) => Action = action;

        // this is only place that rules actions are actually run
        // error handling, logging, et al. belong here
        public void Run(WebElement element, T productItem) => Action(element, productItem);
    }

    internal class RuleSet<T> : IRuleClass<T>, IEnumerable<IRuleClass<T>>
    {
        private List<IRuleClass<T>> rules { get; } = new List<IRuleClass<T>>();

        public void Add(IRuleClass<T> ruleClass) => rules.Add(ruleClass);
        public void Add(Action<WebElement, T> action) => rules.Add(new BasicRule<T>(action));
        public void AddRange(IEnumerable<IRuleClass<T>> rules) => this.rules.AddRange(rules);

        public IEnumerator<IRuleClass<T>> GetEnumerator() => rules.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => rules.GetEnumerator();

        public virtual void Run(WebElement element, T productItem)
        {
            foreach (var rule in rules)
                rule.Run(element, productItem);
        }
    }

    /// <summary>LocatedRuleSet loops through found items. When it's 0 or 1, LocatedRuleSet is an easy way to parse only if exists</summary>
    internal class LocatedRuleSet<T> : RuleSet<T>
    {
        public By ElementsLocator;
        public LocatedRuleSet(By elementsLocator) => ElementsLocator = elementsLocator;
        public override void Run(WebElement parentElement, T productItem)
        {
            foreach (var childElements in parentElement.FindElements(ElementsLocator))
                base.Run(childElements, productItem);
        }
    }
}
