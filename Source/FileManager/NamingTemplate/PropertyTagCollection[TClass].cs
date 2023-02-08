using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace FileManager.NamingTemplate;

public delegate string PropertyFormatter<T>(ITemplateTag templateTag, T value, string formatString);

public class PropertyTagCollection<TClass> : TagCollection
{
	private readonly Dictionary<Type, MulticastDelegate> defaultFormatters = new();

	public PropertyTagCollection(bool caseSensative = true, params MulticastDelegate[] defaultFormatters) : base(typeof(TClass), caseSensative)
	{
		foreach (var formatter in defaultFormatters)
		{
			var parameters = formatter.Method.GetParameters();

			if (formatter.Method.ReturnType != typeof(string)
				|| parameters.Length != 3
				|| parameters[0].ParameterType != typeof(ITemplateTag)
				|| parameters[2].ParameterType != typeof(string))
				throw new ArgumentException($"{nameof(defaultFormatters)} must have a signature of [{nameof(String)} PropertyFormatter<T>({nameof(ITemplateTag)}, T, {nameof(String)})]");

			this.defaultFormatters[parameters[1].ParameterType] = formatter;
		}
	}

	/// <summary>
	/// Register a nullable value type <typeparamref name="TClass"/> property.
	/// </summary>
	/// <typeparam name="TProperty">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="propertyGetter">A Func to get the property value from <see cref="TClass"/></param>
	/// <param name="formatter">Optional formatting function that accepts the <typeparamref name="TProperty"/> property
	/// and a formatting string and returnes the value the formatted string. If <see cref="null"/>, use the default
	/// <typeparamref name="TProperty"/> formatter if present, or <see cref="object.ToString"/></param>
	public void Add<TProperty>(ITemplateTag templateTag, Func<TClass, TProperty?> propertyGetter, PropertyFormatter<TProperty> formatter = null)
		where TProperty : struct
		=> RegisterWithFormatter(templateTag, propertyGetter, formatter);

	/// <summary>
	/// Register a nullable value type <typeparamref name="TClass"/> property.
	/// </summary>
	/// <typeparam name="TProperty">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="propertyGetter">A Func to get the string property from <see cref="TClass"/></param>
	/// <param name="toString">ToString function that accepts the <typeparamref name="TProperty"/> property and returnes a string</param>
	public void Add<TProperty>(ITemplateTag templateTag, Func<TClass, TProperty?> propertyGetter, Func<TProperty, string> toString)
		where TProperty : struct
		=> RegisterWithToString(templateTag, propertyGetter, toString);

	/// <summary>
	/// Register a <typeparamref name="TClass"/> property
	/// </summary>
	/// <typeparam name="TProperty">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="propertyGetter">A Func to get the property value from <see cref="TClass"/></param>
	/// <param name="formatter">Optional formatting function that accepts the <typeparamref name="TProperty"/> property
	/// and a formatting string and returnes the value formatted to string. If <see cref="null"/>, use the default
	/// <typeparamref name="TProperty"/> formatter if present, or <see cref="object.ToString"/></param>
	public void Add<TProperty>(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, PropertyFormatter<TProperty> formatter = null)
		=> RegisterWithFormatter(templateTag, propertyGetter, formatter);

	/// <summary>
	/// Register a <typeparamref name="TClass"/> property.
	/// </summary>
	/// <typeparam name="TProperty">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="propertyGetter">A Func to get the string property from <see cref="TClass"/></param>
	/// <param name="toString">ToString function that accepts the <typeparamref name="TProperty"/> property and returnes a string</param>
	public void Add<TProperty>(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, Func<TProperty, string> toString)
		=> RegisterWithToString(templateTag, propertyGetter, toString);

	private void RegisterWithFormatter<TProperty, TPropertyValue>
		(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, PropertyFormatter<TPropertyValue> formatter)
	{
		ArgumentValidator.EnsureNotNull(templateTag, nameof(templateTag));
		ArgumentValidator.EnsureNotNull(propertyGetter, nameof(propertyGetter));

		var expr = Expression.Call(Expression.Constant(propertyGetter.Target), propertyGetter.Method, Parameter);

		if ((formatter ??= GetDefaultFormatter<TPropertyValue>()) is null)
			AddPropertyTag(new PropertyTag<TPropertyValue>(templateTag, Options, expr, ToStringFunc));
		else
			AddPropertyTag(new PropertyTag<TPropertyValue>(templateTag, Options, expr, formatter));
	}

	private void RegisterWithToString<TProperty, TPropertyValue>
		(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, Func<TPropertyValue, string> toString)
	{
		ArgumentValidator.EnsureNotNull(templateTag, nameof(templateTag));
		ArgumentValidator.EnsureNotNull(propertyGetter, nameof(propertyGetter));

		var expr = Expression.Call(Expression.Constant(propertyGetter.Target), propertyGetter.Method, Parameter);
		AddPropertyTag(new PropertyTag<TPropertyValue>(templateTag, Options, expr, toString ?? ToStringFunc));
	}

	private static string ToStringFunc<T>(T propertyValue) => propertyValue?.ToString() ?? "";

	private PropertyFormatter<T> GetDefaultFormatter<T>()
	{
		try
		{
			var del = defaultFormatters.FirstOrDefault(kvp => kvp.Key == typeof(T)).Value;
			return del is null ? null : Delegate.CreateDelegate(typeof(PropertyFormatter<T>), del.Target, del.Method) as PropertyFormatter<T>;
		}
		catch { return null; }
	}

	private class PropertyTag<TPropertyValue> : TagBase
	{
		private Func<Expression, string, Expression> CreateToStringExpression { get; }

		public PropertyTag(ITemplateTag templateTag, RegexOptions options, Expression propertyGetter, PropertyFormatter<TPropertyValue> formatter)
			: base(templateTag, propertyGetter)
		{
			NameMatcher = new Regex(@$"^<{templateTag.TagName.Replace(" ", "\\s*?")}\s*?(?:\[([^\[\]]*?)\]\s*?)?>", options);
			CreateToStringExpression = (expVal, format) =>
				Expression.Call(
					formatter.Target is null ? null : Expression.Constant(formatter.Target),
					formatter.Method,
					Expression.Constant(templateTag),
					expVal,
					Expression.Constant(format));
		}

		public PropertyTag(ITemplateTag templateTag, RegexOptions options, Expression propertyGetter, Func<TPropertyValue, string> toString)
			: base(templateTag, propertyGetter)
		{
			NameMatcher = new Regex(@$"^<{templateTag.TagName.Replace(" ", "\\s*?")}>", options);
			CreateToStringExpression = (expVal, _) =>
					Expression.Call(
						toString.Target is null ? null : Expression.Constant(toString.Target),
						toString.Method,
						expVal);
		}

		protected override Expression GetTagExpression(string exactName, string formatString)
		{
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
