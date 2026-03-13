using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using PF = FileManager.NamingTemplate.CommonFormatters;

namespace FileManager.NamingTemplate;

public class PropertyTagCollection<TClass> : TagCollection
{
	private readonly Dictionary<Type, MulticastDelegate> _defaultFormatters = new();

	public PropertyTagCollection(bool caseSensitive = true, params MulticastDelegate[] defaultFormatters) : base(typeof(TClass), caseSensitive)
	{
		foreach (var formatter in defaultFormatters)
		{
			var parameters = formatter.Method.GetParameters();

			if (formatter.Method.ReturnType != typeof(string)
			    || parameters.Length != 4
				|| parameters[0].ParameterType != typeof(ITemplateTag)
			    || parameters[2].ParameterType != typeof(string)
			    || !typeof(CultureInfo).IsAssignableFrom(parameters[3].ParameterType))
				throw new ArgumentException(
					$"{nameof(defaultFormatters)} must have a signature of [{nameof(String)} PropertyFormatter<T>({nameof(ITemplateTag)}, T, {nameof(String)}, {nameof(CultureInfo)})]");

			this._defaultFormatters[parameters[1].ParameterType] = formatter;
		}
	}

	/// <summary>
	/// Register a nullable value type <typeparamref name="TClass"/> property.
	/// </summary>
	/// <typeparam name="TProperty">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="templateTag"></param>
	/// <param name="propertyGetter">A Func to get the property value from <see cref="TClass"/></param>
	/// <param name="formatter">Optional formatting function that accepts the <typeparamref name="TProperty"/> property
	/// and a formatting string and returns the value the formatted string. If <c>null</c>, use the default
	/// <typeparamref name="TProperty"/> formatter if present, or <see cref="object.ToString"/></param>
	public void Add<TProperty>(ITemplateTag templateTag, Func<TClass, TProperty?> propertyGetter, PF.PropertyFormatter<TProperty>? formatter = null)
		where TProperty : struct
		=> RegisterWithFormatter(templateTag, propertyGetter, formatter);

	/// <summary>
	/// Register a nullable value type <typeparamref name="TClass"/> property.
	/// </summary>
	/// <typeparam name="TProperty">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="templateTag"></param>
	/// <param name="propertyGetter">A Func to get the string property from <see cref="TClass"/></param>
	/// <param name="toString">ToString function that accepts the <typeparamref name="TProperty"/> property and returns a string</param>
	public void Add<TProperty>(ITemplateTag templateTag, Func<TClass, TProperty?> propertyGetter, Func<TProperty, string> toString)
		where TProperty : struct
		=> RegisterWithToString(templateTag, propertyGetter, toString);

	/// <summary>
	/// Register a <typeparamref name="TClass"/> property
	/// </summary>
	/// <typeparam name="TProperty">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="templateTag"></param>
	/// <param name="propertyGetter">A Func to get the property value from <see cref="TClass"/></param>
	/// <param name="formatter">Optional formatting function that accepts the <typeparamref name="TProperty"/> property
	/// and a formatting string and returns the value formatted to string. If <c>null</c>, use the default
	/// <typeparamref name="TProperty"/> formatter if present, or <see cref="object.ToString"/></param>
	public void Add<TProperty>(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, PF.PropertyFormatter<TProperty>? formatter = null)
		=> RegisterWithFormatter(templateTag, propertyGetter, formatter);

	/// <summary>
	/// Register a <typeparamref name="TClass"/> property.
	/// </summary>
	/// <typeparam name="TProperty">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="templateTag"></param>
	/// <param name="propertyGetter">A Func to get the string property from <see cref="TClass"/></param>
	/// <param name="toString">ToString function that accepts the <typeparamref name="TProperty"/> property and returns a string</param>
	public void Add<TProperty>(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, Func<TProperty, string> toString)
		=> RegisterWithToString(templateTag, propertyGetter, toString);

	private void RegisterWithFormatter<TProperty, TPropertyValue>
		(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, PF.PropertyFormatter<TPropertyValue>? formatter)
	{
		ArgumentValidator.EnsureNotNull(templateTag, nameof(templateTag));
		ArgumentValidator.EnsureNotNull(propertyGetter, nameof(propertyGetter));

		var expr = Expression.Call(Expression.Constant(propertyGetter.Target), propertyGetter.Method, Parameter);
		formatter ??= GetDefaultFormatter<TPropertyValue>();

		AddPropertyTag(formatter is null
			? new PropertyTag<TPropertyValue>(templateTag, Options, expr, ToStringFunc)
			: new PropertyTag<TPropertyValue>(templateTag, Options, expr, formatter));
	}

	private void RegisterWithToString<TProperty, TPropertyValue>
		(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, Func<TPropertyValue, string> toString)
	{
		ArgumentValidator.EnsureNotNull(templateTag, nameof(templateTag));
		ArgumentValidator.EnsureNotNull(propertyGetter, nameof(propertyGetter));

		var expr = Expression.Call(Expression.Constant(propertyGetter.Target), propertyGetter.Method, Parameter);
		AddPropertyTag(new PropertyTag<TPropertyValue>(templateTag, Options, expr, toString));
	}

	private static string ToStringFunc<T>(T propertyValue) => propertyValue?.ToString() ?? "";

	private PF.PropertyFormatter<T>? GetDefaultFormatter<T>()
	{
		try
		{
			var del = _defaultFormatters.FirstOrDefault(kvp => kvp.Key == typeof(T)).Value;
			return del is null ? null : Delegate.CreateDelegate(typeof(PF.PropertyFormatter<T>), del.Target, del.Method) as PF.PropertyFormatter<T>;
		}
		catch { return null; }
	}

	/// <summary>
	/// Try to get the default (unformatted) value of a property tag.
	/// </summary>
	/// <param name="tagName">Name of the tag value to get</param>
	/// <param name="object">The property class from which the tag's value is read</param>
	/// <param name="value"><paramref name="tagName"/>'s string value if it is in this collection, otherwise null</param>
	/// <returns>True if the <paramref name="tagName"/> is in this collection, otherwise false</returns>
	public bool TryGetValue(string tagName, TClass @object, CultureInfo? culture, [NotNullWhen(true)] out string? value)
	{
		value = null;

		if (!StartsWith($"<{tagName}>", out _, out _, out var valueExpression))
			return false;

		var func = Expression.Lambda<Func<TClass, CultureInfo?, string>>(valueExpression, Parameter, CultureParameter).Compile();
		value = func(@object, culture);
		return true;
	}

	private class PropertyTag<TPropertyValue> : TagBase
	{
		public override Regex NameMatcher { get; }
		private Func<Expression, string, Expression> CreateToStringExpression { get; }

		public PropertyTag(ITemplateTag templateTag, RegexOptions options, Expression propertyGetter, PF.PropertyFormatter<TPropertyValue> formatter)
			: base(templateTag, propertyGetter)
		{
			NameMatcher = new Regex($"""
			                         (?x)                     # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
			                         ^<                       # tags start with a '<'
			                         {TagNameForRegex()}      # next the tagname needs to be matched with space being made optional. Also escape all '#'
			                         (?:\s*                   # optional whitespace
			                             \[                   # optional format details enclosed in '[' and ']'.
			                                 (?<format>       # - capture inner part as <format>
			                                     [^\]]*?      # - match any character except ']'
			                                 )                #
			                             \]                   # - closing the format part
			                         )?\s*>                   # Tags end with '>'
			                         """
				, options | RegexOptions.Compiled);
			CreateToStringExpression = (expVal, format) =>
				Expression.Call(
					formatter.Target is null ? null : Expression.Constant(formatter.Target),
					formatter.Method,
					Expression.Constant(templateTag),
					expVal,
					Expression.Constant(format),
					CultureParameter);
		}

		public PropertyTag(ITemplateTag templateTag, RegexOptions options, Expression propertyGetter, Func<TPropertyValue, string> toString)
			: base(templateTag, propertyGetter)
		{
			NameMatcher = new Regex(@$"^<{TagNameForRegex()}>", options | RegexOptions.Compiled);
			CreateToStringExpression = (expVal, _) =>
					Expression.Call(
						toString.Target is null ? null : Expression.Constant(toString.Target),
						toString.Method,
						expVal);
		}

		protected override Expression GetTagExpression(string exactName, string[] extraData)
		{
			if (extraData.Length is not (0 or 1))
				return Expression.Constant(exactName);

			string formatString = extraData.Length == 1 ? extraData[0] : "";

			Expression toStringExpression
				= !ReturnType.IsValueType
				? Expression.Condition(
					Expression.Equal(ValueExpression, Expression.Constant(null)),
					Expression.Constant(""),
					CreateToStringExpression(ValueExpression, formatString))
				: Nullable.GetUnderlyingType(ReturnType) is null
				? CreateToStringExpression(ValueExpression, formatString)
				: Expression.Condition(
					Expression.PropertyOrField(ValueExpression, "HasValue"),
					CreateToStringExpression(Expression.PropertyOrField(ValueExpression, "Value"), formatString),
					Expression.Constant(""));

			return Expression.TryCatch(toStringExpression, Expression.Catch(typeof(Exception), Expression.Constant(exactName)));
		}
	}
}
