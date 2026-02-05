using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

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

public delegate bool Conditional<T>(ITemplateTag templateTag, T value, string condition);

public class ConditionalTagCollection<TClass> : TagCollection
{
	public ConditionalTagCollection(bool caseSensative = true) : base(typeof(TClass), caseSensative) { }

	/// <summary>
	/// Register a conditional tag.
	/// </summary>
	/// <param name="propertyGetter">A Func to get the condition's <see cref="bool"/> value from <see cref="TClass"/></param>
	public void Add(ITemplateTag templateTag, Func<TClass, bool> propertyGetter)
	{
		var target = propertyGetter.Target is null ? null : Expression.Constant(propertyGetter.Target);
		var expr = Expression.Call(target, propertyGetter.Method, Parameter);
		AddPropertyTag(new ConditionalTag(templateTag, Options, expr));
	}

	/// <summary>
	/// Register a conditional tag.
	/// </summary>
	/// <param name="conditional">A <see cref="Conditional{TClass}"/> to get the condition's <see cref="bool"/> value</param>
	public void Add(ITemplateTag templateTag, Conditional<TClass> conditional)
	{
		AddPropertyTag(new ConditionalTag(templateTag, Options, Parameter, conditional));
	}

	private class ConditionalTag : TagBase, IClosingPropertyTag
	{
		public override Regex NameMatcher { get; }
		public Regex NameCloseMatcher { get; }

		private Func<string?, Expression> CreateConditionExpression { get; }

		public ConditionalTag(ITemplateTag templateTag, RegexOptions options, Expression conditionExpression)
			: base(templateTag, conditionExpression)
		{
			NameMatcher = new Regex(@$"^<(!)?{templateTag.TagName}->", options);
			NameCloseMatcher = new Regex($"^<-{templateTag.TagName}>", options);
			CreateConditionExpression = _ => conditionExpression;
		}

		public ConditionalTag(ITemplateTag templateTag, RegexOptions options, ParameterExpression parameter, Conditional<TClass> conditional)
			: base(templateTag, Expression.Constant(false))
		{
			NameMatcher = new Regex(@$"^<(!)?{templateTag.TagName}(?:\s+?(.*?)\s*?)?->", options);
			NameCloseMatcher = new Regex($"^<-{templateTag.TagName}>", options);

			var target = conditional.Target is null ? null : Expression.Constant(conditional.Target);
			CreateConditionExpression = condition
				=> Expression.Call(
					conditional.Target is null ? null : Expression.Constant(conditional.Target),
					conditional.Method,
					Expression.Constant(templateTag),
					parameter,
					Expression.Constant(condition));
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

		protected override Expression GetTagExpression(string exactName, string[] extraData)
		{
			if (extraData.Length is not (1 or 2) || extraData[0] is not ("!" or "") || extraData.Length == 2 && string.IsNullOrWhiteSpace(extraData[1]))
				return Expression.Constant(false);

			var getBool = extraData.Length == 2 ? CreateConditionExpression(extraData[1]) : CreateConditionExpression(null);
			return extraData[0] == "!" ? Expression.Not(getBool) : getBool;
		}
	}
}
