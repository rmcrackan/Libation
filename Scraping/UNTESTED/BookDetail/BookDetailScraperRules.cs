using System;
using Scraping.Rules;
using Scraping.Selectors;

using DTO = DTOs.BookDetailDTO;

namespace Scraping.BookDetail
{
    /// <summary>not the same as LocatedRuleSet. IRuleClass only acts upon 1 product item at a time. RuleFamily returns many product items</summary>
    internal class RuleFamilyBD : RuleFamily<DTO> { }

    internal interface IRuleClassBD : IRuleClass<DTO> { }

    internal class BasicRuleBD : BasicRule<DTO>, IRuleClassBD
    {
        public BasicRuleBD() : base() { }
        public BasicRuleBD(Action<WebElement, DTO> action) : base(action) { }
    }

    internal class RuleSetBD : RuleSet<DTO>, IRuleClassBD { }

    /// <summary>LocatedRuleSet loops through found items. When it's 0 or 1, LocatedRuleSet is an easy way to parse only if exists</summary>
    internal class LocatedRuleSetBD : LocatedRuleSet<DTO>, IRuleClassBD
    {
        public LocatedRuleSetBD(By elementsLocator) : base(elementsLocator) { }
    }
}