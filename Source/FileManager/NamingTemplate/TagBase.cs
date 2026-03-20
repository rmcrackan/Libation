using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace FileManager.NamingTemplate;

internal enum OutputType
{
	String,
	Object
}

internal interface IPropertyTag
{
	/// <summary>The tag that will be matched in a tag string</summary>
	ITemplateTag TemplateTag { get; }

	/// <summary><see cref="TemplateTag"/>'s <see cref="Type"/></summary>
	Type ReturnType { get; }

	/// <summary>The <see cref="Regex"/> used to match <see cref="TemplateTag"/> in template strings.</summary>
	Regex NameMatcher { get; }

	/// <summary>
	/// Determine if the template string starts with <see cref="TemplateTag"/>, and if it does parse the tag to an <see cref="Expression"/>
	/// </summary>
	/// <param name="templateString">Template string</param>
	/// <param name="outputType">Whether to return a string or object expression</param>
	/// <param name="exactName">The <paramref name="templateString"/> substring that was matched.</param>
	/// <param name="propertyValue">The <see cref="Expression"/> that returns the property's value</param>
	/// <returns>True if the <paramref name="templateString"/> starts with this tag.</returns>
	bool StartsWith(string templateString, OutputType outputType, [NotNullWhen(true)] out string? exactName, [NotNullWhen(true)] out Expression? propertyValue);
}

internal abstract class TagBase(ITemplateTag templateTag, Expression propertyExpression) : IPropertyTag
{
	public ITemplateTag TemplateTag { get; } = templateTag;
	public abstract Regex NameMatcher { get; }
	public Type ReturnType => ValueExpression.Type;
	protected Expression ValueExpression { get; } = propertyExpression;

	/// <summary>Create an <see cref="Expression"/> that returns the property's value.</summary>
	/// <param name="exactName">The exact string that was matched to <see cref="ITemplateTag"/></param>
	/// <param name="matchData">Optional extra data parsed from the tag, such as a format string in the match the square brackets, logical negation, and conditional options</param>
	/// <param name="outputType">Whether to return a string or object expression</param>
	protected abstract Expression GetTagExpression(string exactName, Dictionary<string, Group> matchData, OutputType outputType);

	public bool StartsWith(string templateString, OutputType outputType, [NotNullWhen(true)] out string? exactName, [NotNullWhen(true)] out Expression? propertyValue)
	{
		var match = NameMatcher.Match(templateString);
		if (match.Success)
		{
			exactName = match.Value;
			propertyValue = GetTagExpression(exactName, match.Groups.Values.Skip(1).ToDictionary(v => v.Name, v => v), outputType);
			return true;
		}

		exactName = null;
		propertyValue = null;
		return false;
	}

	protected string TagNameForRegex()
	{
		return TemplateTag.TagName.Replace(" ", @"\s*").Replace("#", @"\#");
	}

	protected static string? Unescape(Group? group)
	{
		return group?.Success ?? false ? Unescape(group.ValueSpan) : null;
	}

	protected static string Unescape(ReadOnlySpan<char> valueSpan)
	{
		if (valueSpan.IsEmpty) return "";

		var first = valueSpan.IndexOf('\\');
		if (first < 0)
			return valueSpan.ToString();

		var sb = new StringBuilder(valueSpan.Length);
		sb.Append(valueSpan[..first]);
		for (var i = first; i < valueSpan.Length; i++)
		{
			if (valueSpan[i] == '\\' && i + 1 < valueSpan.Length)
				i++; // skip backslash and take the next char
			sb.Append(valueSpan[i]);
		}

		return sb.ToString();
	}

	public override string ToString()
	{
		return $"[Name = {TemplateTag.TagName}, Type = {ReturnType.Name}]";
	}
}
