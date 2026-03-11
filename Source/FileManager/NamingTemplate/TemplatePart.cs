using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace FileManager.NamingTemplate;

/// <summary>Represents one part of an evaluated <see cref="NamingTemplate"/>.</summary>
public class TemplatePart : IEnumerable<TemplatePart>
{
	/// <summary>The <see cref="TemplatePart"/> name. If <see cref="TemplatePart"/> is
	/// a registered property, this value is <see cref="ITemplateTag.TagName"/></summary>
	public string TagName { get; }

	/// <summary> The <see cref="IPropertyTag"/>'s <see cref="ITemplateTag"/> if <see cref="TemplatePart"/> is
	/// a registered property, otherwise <c>null</c> for string literals. </summary>
	public ITemplateTag? TemplateTag { get; }

	/// <summary>The evaluated string.</summary>
	public string Value { get; }

	private TemplatePart? _previous;
	private TemplatePart? _next;
	private TemplatePart(string name, string value)
	{
		TagName = name;
		Value = value;
	}
	private TemplatePart(ITemplateTag templateTag, string value)
	{
		TemplateTag = templateTag;
		TagName = templateTag.TagName;
		Value = value;
	}

	internal static Expression Blank
		=> CreateExpression("Blank", Expression.Constant(""));

	internal static Expression CreateLiteral(string constant)
		=> CreateExpression("Literal", Expression.Constant(constant));

	internal static Expression CreateProperty(ITemplateTag templateTag, Expression property)
		=> Expression.New(tagTemplateConstructorInfo, Expression.Constant(templateTag), property);

	internal static Expression CreateConcatenation(Expression left, Expression right)
	{
		if (left.Type != typeof(TemplatePart) || right.Type != typeof(TemplatePart))
			throw new InvalidOperationException($"Cannot concatenate expressions of types {left.Type.Name} and {right.Type.Name}");
		return Expression.Add(left, right, addMethodInfo);
	}

	private static Expression CreateExpression(string name, Expression value)
		=> Expression.New(constructorInfo, Expression.Constant(name), value);

	private static readonly ConstructorInfo constructorInfo;
	private static readonly ConstructorInfo tagTemplateConstructorInfo;
	private static readonly MethodInfo addMethodInfo;
	static TemplatePart()
	{
		var type = typeof(TemplatePart);

		if (type.GetConstructor(
			BindingFlags.NonPublic | BindingFlags.Instance,
			[typeof(string), typeof(string)]) is not { } c1)
			throw new MissingMethodException(nameof(TemplatePart));

		if (type.GetConstructor(
			BindingFlags.NonPublic | BindingFlags.Instance,
			[typeof(ITemplateTag), typeof(string)]) is not { } c2)
			throw new MissingMethodException(nameof(TemplatePart));

		if (type.GetMethod(
			nameof(Concatenate),
			BindingFlags.NonPublic | BindingFlags.Static,
			[typeof(TemplatePart), typeof(TemplatePart)]) is not { } m1)
			throw new MissingMethodException(nameof(Concatenate));

		constructorInfo = c1;
		tagTemplateConstructorInfo = c2;
		addMethodInfo = m1;
	}

	public IEnumerator<TemplatePart> GetEnumerator()
	{
		var firstPart = FirstPart;

		do
		{
			if (firstPart.TemplateTag is not null || firstPart.TagName is not "Blank")
				yield return firstPart;
			firstPart = firstPart._next;
		}
		while (firstPart is not null);
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	internal TemplatePart FirstPart
	{
		get
		{
			var part = this;
			while (part._previous is not null)
				part = part._previous;
			return part;
		}
	}

	private TemplatePart LastPart
	{
		get
		{
			var part = this;
			while (part._next is not null)
				part = part._next;
			return part;
		}
	}

	private static TemplatePart Concatenate(TemplatePart left, TemplatePart right)
	{
		var last = left.LastPart;
		last._next = right;
		right._previous = last;
		return left.FirstPart;
	}
}
