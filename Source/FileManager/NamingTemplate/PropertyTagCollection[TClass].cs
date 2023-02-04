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
	private readonly Dictionary<Type, Delegate> defaultFormatters = new();

	public PropertyTagCollection(bool caseSensative = true, params Delegate[] defaultFormatters) : base(typeof(TClass), caseSensative)
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
	/// Register a nullable value type property.
	/// </summary>
	/// <typeparam name="U">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="propertyGetter">A Func to get the property value from <see cref="TClass"/></param>
	/// <param name="formatter">Optional formatting function that accepts the <typeparamref name="U"/> property
	/// and a formatting string and returnes the value the formatted string. If <see cref="null"/>, use the default
	/// <typeparamref name="U"/> formatter if present, or <see cref="object.ToString"/></param>
	public void Add<U>(ITemplateTag templateTag, Func<TClass, U?> propertyGetter, PropertyFormatter<U> formatter = null)
		where U : struct
		=> RegisterWithFormatter(templateTag, propertyGetter, formatter);

	/// <summary>
	/// Register a nullable value type property.
	/// </summary>
	/// <param name="propertyGetter">A Func to get the string property from <see cref="TClass"/></param>
	/// <param name="toString">ToString function that accepts the <typeparamref name="U"/> property and returnes a string</param>
	public void Add<U>(ITemplateTag templateTag, Func<TClass, U?> propertyGetter, Func<U, string> toString)
		where U : struct
		=> RegisterWithToString(templateTag, propertyGetter, toString);

	/// <summary>
	/// Register a non-nullable value type property
	/// </summary>
	/// <typeparam name="U">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="propertyGetter">A Func to get the property value from <see cref="TClass"/></param>
	/// <param name="formatter">Optional formatting function that accepts the <typeparamref name="U"/> property
	/// and a formatting string and returnes the value formatted to string. If <see cref="null"/>, use the default
	/// <typeparamref name="U"/> formatter if present, or <see cref="object.ToString"/></param>
	public void Add<U>(ITemplateTag templateTag, Func<TClass, U> propertyGetter, PropertyFormatter<U> formatter = null)
		where U : struct
		=> RegisterWithFormatter(templateTag, propertyGetter, formatter);

	/// <summary>
	/// Register a non-nullable value type property.
	/// </summary>
	/// <param name="propertyGetter">A Func to get the string property from <see cref="TClass"/></param>
	/// <param name="toString">ToString function that accepts the <typeparamref name="U"/> property and returnes a string</param>
	public void Add<U>(ITemplateTag templateTag, Func<TClass, U> propertyGetter, Func<U, string> toString)
		where U : struct
		=> RegisterWithToString(templateTag, propertyGetter, toString);

	/// <summary>
	/// Register a string type property
	/// </summary>
	/// <param name="propertyGetter">A Func to get the string property from <see cref="TClass"/></param>
	/// <param name="formatter">Optional formatting function that accepts the string property and a formatting
	/// string and returnes the value formatted to string. If <see cref="null"/>, use the default <see cref="string"/> 
	/// formatter if present, or <see cref="string.ToString"/></param>
	public void Add(ITemplateTag templateTag, Func<TClass, string> propertyGetter, PropertyFormatter<string> formatter = null)
		=> RegisterWithFormatter(templateTag, propertyGetter, formatter);

	/// <summary>
	/// Register a string type property.
	/// </summary>
	/// <param name="propertyGetter">A Func to get the string property from <see cref="TClass"/></param>
	/// <param name="toString">ToString function that accepts the string property and returnes a string</param>
	public void Add(ITemplateTag templateTag, Func<TClass, string> propertyGetter, Func<string, string> toString)
	=> RegisterWithToString(templateTag, propertyGetter, toString);

	private void RegisterWithFormatter<T,U>(ITemplateTag templateTag, Func<TClass, T> propertyGetter, PropertyFormatter<U> formatter)
	{
		static string ToStringFunc(U value) => value is string str ? str : value.ToString();
		ArgumentValidator.EnsureNotNull(templateTag, nameof(templateTag));
		ArgumentValidator.EnsureNotNull(propertyGetter, nameof(propertyGetter));

		var formatDelegate = formatter ?? defaultFormatters.FirstOrDefault(kvp => kvp.Key == typeof(U)).Value;

		var expr = Expression.Call(Expression.Constant(propertyGetter.Target), propertyGetter.Method, Parameter);

		if (formatDelegate is null)
			AddPropertyTag(PropertyTag.CreateWithToString(templateTag, Options, expr, ToStringFunc));
		else
			AddPropertyTag(PropertyTag.CreateWithFormatter(templateTag, Options, expr, formatDelegate));
	}

	private void RegisterWithToString<T,U>(ITemplateTag templateTag, Func<TClass, T> propertyGetter, Func<U, string> toString)
	{
		static string ToStringFunc(U value) => value is string str ? str : value.ToString();
		ArgumentValidator.EnsureNotNull(templateTag, nameof(templateTag));
		ArgumentValidator.EnsureNotNull(propertyGetter, nameof(propertyGetter));

		toString ??= ToStringFunc;

		var expr = Expression.Call(Expression.Constant(propertyGetter.Target), propertyGetter.Method, Parameter);
		AddPropertyTag(PropertyTag.CreateWithToString(templateTag, Options, expr, toString));
	}

	private class PropertyTag : TagBase
	{
		private Func<Expression, string, Expression> CreateToStringExpression { get; init; }
		private PropertyTag(ITemplateTag templateTag, Expression propertyExpression) : base(templateTag, propertyExpression) { }

		public static PropertyTag CreateWithFormatter(ITemplateTag templateTag, RegexOptions options, Expression propertyExpression, Delegate formatter)
		{
			return new PropertyTag(templateTag, propertyExpression)
			{
				NameMatcher = new Regex(@$"^<{templateTag.TagName.Replace(" ", "\\s*?")}\s*?(?:\[([^\[\]]*?)\]\s*?)?>", options),
				CreateToStringExpression = (expVal, format) =>
				Expression.Call(
					formatter.Target is null ? null : Expression.Constant(formatter.Target),
					formatter.Method,
					Expression.Constant(templateTag),
					expVal,
					Expression.Constant(format))
			};
		}

		public static PropertyTag CreateWithToString(ITemplateTag templateTag, RegexOptions options, Expression propertyExpression, Delegate toString)
		{
			return new PropertyTag(templateTag, propertyExpression)
			{
				NameMatcher = new Regex(@$"^<{templateTag.TagName}>", options),
				CreateToStringExpression = (expVal, _) =>
					Expression.Call(
						toString.Target is null ? null : Expression.Constant(toString.Target),
						toString.Method,
						expVal)
			};
		}

		protected override Expression GetTagExpression(string exactName, string formatString)
		{
			Expression toStringExpression
				= ReturnType == typeof(string)
				? CreateToStringExpression(Expression.Coalesce(ValueExpression, Expression.Constant("")), formatString)
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
