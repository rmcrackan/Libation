using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

#nullable enable
namespace FileManager.NamingTemplate;

/// <summary>Represents one part of an evaluated <see cref="NamingTemplate"/>.</summary>
public class TemplatePart : IEnumerable<TemplatePart>
{
	/// <summary>The <see cref="TemplatePart"/> name. If <see cref="TemplatePart"/> is
	/// a registered property, this value is <see cref="ITemplateTag.TagName"/></summary>
	public string TagName { get; }

	/// <summary> The <see cref="IPropertyTag"/>'s <see cref="ITemplateTag"/> if <see cref="TemplatePart"/> is
	/// a registered property, otherwise <see cref="null"/> for string literals. </summary>
	public ITemplateTag? TemplateTag { get; }

	/// <summary>The evaluated string.</summary>
	public string Value { get; }

	private TemplatePart? previous;
	private TemplatePart? next;
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
			new Type[] { typeof(string), typeof(string) }) is not ConstructorInfo c1)
			throw new MissingMethodException(nameof(TemplatePart));

		if (type.GetConstructor(
			BindingFlags.NonPublic | BindingFlags.Instance,
			new Type[] { typeof(ITemplateTag), typeof(string) }) is not ConstructorInfo c2)
			throw new MissingMethodException(nameof(TemplatePart));

		if (type.GetMethod(
			nameof(Concatenate),
			BindingFlags.NonPublic | BindingFlags.Static,
			new Type[] { typeof(TemplatePart), typeof(TemplatePart) }) is not MethodInfo m1)
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
			firstPart = firstPart.next;
		}
		while (firstPart is not null);
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	internal TemplatePart FirstPart
	{
		get
		{
			var part = this;
			while (part.previous is not null)
				part = part.previous;
			return part;
		}
	}

	private TemplatePart LastPart
	{
		get
		{
			var part = this;
			while (part.next is not null)
				part = part.next;
			return part;
		}
	}

	private static TemplatePart Concatenate(TemplatePart left, TemplatePart right)
	{
		var last = left.LastPart;
		last.next = right;
		right.previous = last;
		return left.FirstPart;
	}
}
