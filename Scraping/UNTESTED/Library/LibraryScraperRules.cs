using System;
using Scraping.Rules;
using Scraping.Selectors;

using DTO = DTOs.LibraryDTO;

namespace Scraping.Library
{
    /// <summary>not the same as LocatedRuleSet. IRuleClass only acts upon 1 product item at a time. RuleFamily returns many product items</summary>
    internal class RuleFamilyLib : RuleFamily<DTO> { }

    internal interface IRuleClassLib : IRuleClass<DTO> { }

    internal class BasicRuleLib : BasicRule<DTO>, IRuleClassLib
    {
        public BasicRuleLib() : base() { }
        public BasicRuleLib(Action<WebElement, DTO> action) : base(action) { }
    }

    internal class RuleSetLib : RuleSet<DTO>, IRuleClassLib { }

    /// <summary>LocatedRuleSet loops through found items. When it's 0 or 1, LocatedRuleSet is an easy way to parse only if exists</summary>
    internal class LocatedRuleSetLib : LocatedRuleSet<DTO>, IRuleClassLib
    {
        public LocatedRuleSetLib(By elementsLocator) : base(elementsLocator) { }
    }
}
