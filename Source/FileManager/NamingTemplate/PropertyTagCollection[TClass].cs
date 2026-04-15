using Dinah.Core;
using System;
using System.Collections.Generic;
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

			_defaultFormatters[parameters[1].ParameterType] = formatter;
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
	public void Add<TProperty>(ITemplateTag templateTag, Func<TClass, TProperty?> propertyGetter, PF.PropertyFormatter<TProperty, string>? formatter = null)
		where TProperty : struct
		=> RegisterWithFormatter(templateTag, propertyGetter, formatter);

	/// <summary>
	/// Register a <typeparamref name="TClass"/> property
	/// </summary>
	/// <typeparam name="TProperty">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="templateTag"></param>
	/// <param name="propertyGetter">A Func to get the property value from <see cref="TClass"/></param>
	/// <param name="formatter">Optional formatting function that accepts the <typeparamref name="TProperty"/> property
	/// and a formatting string and returns the value formatted to string. If <c>null</c>, use the default
	/// <typeparamref name="TProperty"/> formatter if present, or <see cref="object.ToString"/></param>
	public void Add<TProperty>(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, PF.PropertyFormatter<TProperty, string>? formatter = null)
		=> RegisterWithFormatter(templateTag, propertyGetter, formatter);

	/// <summary>
	/// Register a nullable value type <typeparamref name="TClass"/> property.
	/// </summary>
	/// <typeparam name="TProperty">Type of the property from <see cref="TClass"/></typeparam>
	/// <typeparam name="TPreFormatted"></typeparam>
	/// <param name="templateTag"></param>
	/// <param name="propertyGetter">A Func to get the property value from <see cref="TClass"/></param>
	/// <param name="preFormatter">A Func used for first filtering and formatting. The result might be a <see cref="string"/></param>
	/// <param name="finalizer">This Func assures a string result</param>
	/// <typeparamref name="TProperty"/> formatter if present, or <see cref="object.ToString"/>
	public void Add<TProperty, TPreFormatted>(ITemplateTag templateTag, Func<TClass, TProperty?> propertyGetter, PF.PropertyFormatter<TProperty, TPreFormatted> preFormatter,
		PF.PropertyFinalizer<TPreFormatted> finalizer)
		where TProperty : struct
		=> RegisterWithPreFormatter(templateTag, propertyGetter, preFormatter, finalizer);

	/// <summary>
	/// Register a nullable value type <typeparamref name="TClass"/> property.
	/// </summary>
	/// <typeparam name="TProperty">Type of the property from <see cref="TClass"/></typeparam>
	/// <typeparam name="TPreFormatted"></typeparam>
	/// <param name="templateTag"></param>
	/// <param name="propertyGetter">A Func to get the property value from <see cref="TClass"/></param>
	/// <param name="preFormatter">A Func used for first filtering and formatting. The result might be a <see cref="string"/></param>
	/// <param name="finalizer">This Func assures a string result</param>
	/// <typeparamref name="TProperty"/> formatter if present, or <see cref="object.ToString"/>
	public void Add<TProperty, TPreFormatted>(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, PF.PropertyFormatter<TProperty, TPreFormatted> preFormatter,
		PF.PropertyFinalizer<TPreFormatted> finalizer)
		=> RegisterWithPreFormatter(templateTag, propertyGetter, preFormatter, finalizer);

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
	/// Register a <typeparamref name="TClass"/> property.
	/// </summary>
	/// <typeparam name="TProperty">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="templateTag"></param>
	/// <param name="propertyGetter">A Func to get the string property from <see cref="TClass"/></param>
	/// <param name="toString">ToString function that accepts the <typeparamref name="TProperty"/> property and returns a string</param>
	public void Add<TProperty>(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, Func<TProperty, string> toString)
		=> RegisterWithToString(templateTag, propertyGetter, toString);

	private void RegisterWithFormatter<TProperty, TPropertyValue>
		(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, PF.PropertyFormatter<TPropertyValue, string>? formatter)
	{
		formatter ??= GetDefaultFormatter<TPropertyValue>();

		if (formatter is null)
			RegisterWithToString<TProperty, TPropertyValue>(templateTag, propertyGetter, ToStringFunc);
		else
			RegisterWithFormatters(templateTag, propertyGetter, formatter, PF.StringFinalizer, PF.ToFinalizer(formatter));
	}

	private void RegisterWithPreFormatter<TProperty, TPropertyValue, TPreFormatted>
	(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, PF.PropertyFormatter<TPropertyValue, TPreFormatted> preFormatter,
		PF.PropertyFinalizer<TPreFormatted> finalizer)
	{
		var formatter = PF.ToPropertyFormatter(preFormatter, finalizer);
		RegisterWithFormatters(templateTag, propertyGetter, preFormatter, finalizer, formatter);
	}

	private void RegisterWithFormatters<TProperty, TPropertyValue, TPreFormatted>
	(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, PF.PropertyFormatter<TPropertyValue, TPreFormatted> preFormatter,
		PF.PropertyFinalizer<TPreFormatted> finalizer, PF.PropertyFinalizer<TPropertyValue> formatter)
	{
		ArgumentValidator.EnsureNotNull(templateTag, nameof(templateTag));
		ArgumentValidator.EnsureNotNull(propertyGetter, nameof(propertyGetter));

		var expr = Expression.Call(Expression.Constant(propertyGetter.Target), propertyGetter.Method, Parameter);
		AddPropertyTag(new PropertyTag<TPropertyValue, TPreFormatted>(templateTag, Options, expr, preFormatter, finalizer, formatter));
	}

	private void RegisterWithToString<TProperty, TPropertyValue>
		(ITemplateTag templateTag, Func<TClass, TProperty> propertyGetter, Func<TPropertyValue, string> toString)
	{
		ArgumentValidator.EnsureNotNull(templateTag, nameof(templateTag));
		ArgumentValidator.EnsureNotNull(propertyGetter, nameof(propertyGetter));

		var expr = Expression.Call(Expression.Constant(propertyGetter.Target), propertyGetter.Method, Parameter);
		AddPropertyTag(new PropertyTag<TPropertyValue, string>(templateTag, Options, expr, toString));
	}

	private static string ToStringFunc<T>(T propertyValue) => propertyValue?.ToString() ?? "";

	private PF.PropertyFormatter<T, string>? GetDefaultFormatter<T>()
	{
		try
		{
			var del = _defaultFormatters.FirstOrDefault(kvp => kvp.Key == typeof(T)).Value;
			return del is null ? null : Delegate.CreateDelegate(typeof(PF.PropertyFormatter<T, string>), del.Target, del.Method) as PF.PropertyFormatter<T, string>;
		}
		catch { return null; }
	}

	/// <summary>
	/// Try to get the default (unformatted) value of a property tag.
	/// </summary>
	/// <param name="tagName">Name of the tag value to get</param>
	/// <param name="object">The property class from which the tag's value is read</param>
	/// <param name="culture"></param>
	/// <param name="value"><paramref name="tagName"/>'s object value if it is in this collection, otherwise null</param>
	/// <returns>True if the <paramref name="tagName"/> is in this collection, otherwise false</returns>
	public bool TryGetObject(string tagName, TClass @object, CultureInfo? culture, out object? value)
	{
		value = null;

		if (!StartsWith($"<{tagName}>", OutputType.Object, out _, out _, out var valueExpression))
			return false;

		var func = Expression.Lambda<Func<TClass, CultureInfo?, object?>>(valueExpression, Parameter, CultureParameter).Compile();
		value = func(@object, culture);
		return true;
	}

	private class PropertyTag<TPropertyValue, TPreFormatted> : TagBase
	{
		public override Regex NameMatcher { get; }
		private Func<Expression, string?, Expression> CreateToStringExpression { get; }
		private Func<Expression, string?, Expression> CreateToObjectExpression { get; } = (expVal, _) => expVal;

		public PropertyTag(ITemplateTag templateTag, RegexOptions options, Expression propertyGetter, PF.PropertyFormatter<TPropertyValue, TPreFormatted> preFormatter,
			PF.PropertyFinalizer<TPreFormatted> finalizer, PF.PropertyFinalizer<TPropertyValue> formatter)
			: base(templateTag, propertyGetter)
		{
			NameMatcher = new Regex($"""
			                         (?x)                         # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
			                         ^<                           # tags start with a '<'
			                         {TagNameForRegex()}          # next the tagname needs to be matched with space being made optional. Also escape all '#'
			                         (?:\s*                       # optional whitespace
			                             \[  (?<format>           # optional format details enclosed in '[' and ']'. Capture inner part as <format>.
			                                     (?:\\.           # - '\' escapes always the next character. Especially further '\' and the closing ']'
			                                     |'(?:[^']|'')*'  # - allow 'string' to be included in the format, with '' being an escaped ' character
			                                     |"(?:[^"]|"")*"  # - allow "string" to be included in the format, with "" being an escaped " character
			                                     |[^\\\]])* )     # - match any character except '\' and ']'. Format may end in whitespace!
			                             \]                       # - closing the format part
			                         )?\s*>                       # Tags end with '>'
			                         """
				, options);

			// if no format is specified, we can directly use the expVal from the property-getter as object value,
			// otherwise we need to call the preFormatter with the format string and culture info to get the formatted value as object.
			CreateToObjectExpression = (expVal, format) =>
				format is null
					? expVal
					: Expression.Call(
						preFormatter.Target is null ? null : Expression.Constant(preFormatter.Target),
						preFormatter.Method,
						Expression.Constant(templateTag),
						expVal,
						Expression.Constant(format),
						CultureParameter);

			// if no format is specified, we can use the specific formatter to format the value to string directly,
			// otherwise we need to call the preFormatter with the format string and culture info to get the formatted value as object,
			// and then call the finalizer to get the final string value.
			CreateToStringExpression = (expVal, format) =>
				format is null
					? Expression.Call(
						formatter.Target is null ? null : Expression.Constant(formatter.Target),
						formatter.Method,
						Expression.Constant(templateTag),
						expVal,
						CultureParameter)
					: Expression.Call(
						finalizer.Target is null ? null : Expression.Constant(finalizer.Target),
						finalizer.Method,
						Expression.Constant(templateTag),
						Expression.Call(
							preFormatter.Target is null ? null : Expression.Constant(preFormatter.Target),
							preFormatter.Method,
							Expression.Constant(templateTag),
							expVal,
							Expression.Constant(format),
							CultureParameter),
						CultureParameter);
		}

		public PropertyTag(ITemplateTag templateTag, RegexOptions options, Expression propertyGetter, Func<TPropertyValue, string> toString)
			: base(templateTag, propertyGetter)
		{
			NameMatcher = new Regex(@$"^<{TagNameForRegex()}>", options);

			CreateToStringExpression = (expVal, _) =>
					Expression.Call(
						toString.Target is null ? null : Expression.Constant(toString.Target),
						toString.Method,
						expVal);
		}

		protected override Expression GetTagExpression(string exactName, Dictionary<string, Group> matchData, OutputType outputType)
		{
			var formatString = matchData.GetValueOrDefault("format")?.ValueOrNull();
			var isReferenceType = !ReturnType.IsValueType;
			var isNullableValueType = Nullable.GetUnderlyingType(ReturnType) is not null;

			Expression isNullExpression = isReferenceType
				? Expression.Equal(ValueExpression, Expression.Constant(null))
				: isNullableValueType
					? Expression.Not(Expression.PropertyOrField(ValueExpression, "HasValue"))
					: Expression.Constant(false);

			// formatters are defined for non-nullable items <see cref="int"/>, <see cref="DateTime"/> and not for <see cref="int?"/> ...
			var formattableValueExpression = isNullableValueType
				? Expression.PropertyOrField(ValueExpression, "Value")
				: ValueExpression;

			if (outputType == OutputType.String)
			{
				Expression toStringExpression =
					Expression.Condition(
						isNullExpression,
						Expression.Constant(null, typeof(string)),
						CreateToStringExpression(formattableValueExpression, formatString));

				return Expression.TryCatch(toStringExpression, Expression.Catch(typeof(Exception), Expression.Constant(exactName)));
			}

			Expression toObjectExpression =
				Expression.Condition(
					isNullExpression,
					Expression.Constant(null, typeof(object)),
					Expression.Convert(CreateToObjectExpression(formattableValueExpression, formatString), typeof(object)));

			return Expression.TryCatch(toObjectExpression, Expression.Catch(typeof(Exception), Expression.Constant(null, typeof(object))));
		}
	}
}
