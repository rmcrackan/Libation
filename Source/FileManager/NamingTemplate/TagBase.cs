using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace FileManager.NamingTemplate;

internal interface IPropertyTag
{
	/// <summary>The tag that will be matched in a tag string</summary>
	ITemplateTag TemplateTag { get; }

	/// <summary><see cref="TemplateTag"/>'s <see cref="Type"/></summary>
	Type ReturnType { get; }

	/// <summary>The <see cref="Regex"/> used to match <see cref="TemplateTag"/> in template strings.</summary>
	public Regex NameMatcher { get; }

	/// <summary>
	/// Determine if the template string starts with <see cref="TemplateTag"/>, and if it does parse the tag to an <see cref="Expression"/>
	/// </summary>
	/// <param name="templateString">Template string</param>
	/// <param name="exactName">The <paramref name="templateString"/> substring that was matched.</param>
	/// <param name="propertyValue">The <see cref="Expression"/> that returns the property's value</param>
	/// <returns>True if the <paramref name="templateString"/> starts with this tag.</returns>
	bool StartsWith(string templateString, out string exactName, out Expression propertyValue);
}

internal abstract class TagBase : IPropertyTag
{
	public ITemplateTag TemplateTag { get; }
	public Regex NameMatcher { get; protected init; }
	public Type ReturnType => ExpressionValue.Type;
	protected Expression ExpressionValue { get; }

	protected TagBase(ITemplateTag templateTag, Expression propertyExpression)
	{
		TemplateTag = templateTag;
		ExpressionValue = propertyExpression;
	}

	/// <summary>Create an <see cref="Expression"/> that returns the property's value.</summary>
	/// <param name="exactName">The exact string that was matched to <see cref="ITemplateTag"/></param>
	/// <param name="formatter">The optional format string in the match inside the square brackets</param>
	protected abstract Expression GetTagExpression(string exactName, string formatter);

	public bool StartsWith(string templateString, out string exactName, out Expression propertyValue)
	{
		var match = NameMatcher.Match(templateString);
		if (match.Success)
		{
			exactName = match.Value;
			propertyValue = GetTagExpression(exactName, match.Groups.Count == 2 ? match.Groups[1].Value.Trim() : "");
			return true;
		}
		else
		{
			exactName = null;
			propertyValue = null;
			return false;
		}
	}

	public override string ToString()
	{
		return $"[Name = {TemplateTag.TagName}, Type = {ReturnType.Name}]";
	}
}
