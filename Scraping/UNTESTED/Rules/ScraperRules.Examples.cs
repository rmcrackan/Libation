using Scraping.Library;

namespace Scraping.Rules.Examples
{
    class ScraperRulesExamples
    {
        RuleSetLib rulesExamples { get; } = new RuleSetLib
        {
            // equivilant ways to declare a simple rule action
            new BasicRuleLib((row, productItem) => { } ),
            new BasicRuleLib { Action = (row, productItem) => { } },
            (row, productItem) => { }
        };
    }
}
