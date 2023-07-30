using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

#nullable enable
namespace FileManager.NamingTemplate;

internal interface IClosingPropertyTag : IPropertyTag
{
	/// <summary>The <see cref="Regex"/> used to match the closing <see cref="IPropertyTag.TemplateTag"/> in template strings.</summary>
	public Regex NameCloseMatcher { get; }

	/// <summary>
	/// Determine if the template string starts with <see cref="IPropertyTag.TemplateTag"/>'s closing tag signature,
	/// and if it does output the matching tag's <see cref="ITemplateTag"/>
	/// </summary>
	/// <param name="templateString">Template string</param>
	/// <param name="exactName">The <paramref name="templateString"/> substring that was matched.</param>
	/// <param name="propertyTag">The registered <see cref="IPropertyTag"/></param>
	/// <returns>True if the <paramref name="templateString"/> starts with this tag.</returns>
	bool StartsWithClosing(string templateString, [NotNullWhen(true)] out string? exactName, [NotNullWhen(true)] out IClosingPropertyTag? propertyTag);
}

public class ConditionalTagCollection<TClass> : TagCollection
{
	public ConditionalTagCollection(bool caseSensative = true) :base(typeof(TClass), caseSensative) { }

	/// <summary>
	/// Register a conditional tag.
	/// </summary>
	/// <param name="propertyGetter">A Func to get the condition's <see cref="bool"/> value from <see cref="TClass"/></param>
	public void Add(ITemplateTag templateTag, Func<TClass, bool> propertyGetter)
	{
		var expr = Expression.Call(Expression.Constant(propertyGetter.Target), propertyGetter.Method, Parameter);

		AddPropertyTag(new ConditionalTag(templateTag, Options, expr));
	}
	
	private class ConditionalTag : TagBase, IClosingPropertyTag
	{
		public override Regex NameMatcher { get; }
		public Regex NameCloseMatcher { get; }

		public ConditionalTag(ITemplateTag templateTag, RegexOptions options, Expression conditionExpression)
			: base(templateTag, conditionExpression)
		{
			NameMatcher = new Regex($"^<(!)?{templateTag.TagName}->", options);
			NameCloseMatcher = new Regex($"^<-{templateTag.TagName}>", options);
		}

		public bool StartsWithClosing(string templateString, [NotNullWhen(true)] out string? exactName, [NotNullWhen(true)] out IClosingPropertyTag? propertyTag)
		{
			var match = NameCloseMatcher.Match(templateString);
			if (match.Success)
			{
				exactName = match.Value;
				propertyTag = this;
				return true;
			}

			exactName = null;
			propertyTag = null;
			return false;
		}

		protected override Expression GetTagExpression(string exactName, string formatter) => formatter == "!" ? Expression.Not(ValueExpression) : ValueExpression;
	}
}
