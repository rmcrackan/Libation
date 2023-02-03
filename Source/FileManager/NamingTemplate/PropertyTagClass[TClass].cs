using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FileManager.NamingTemplate;

public delegate string PropertyFormatter<T>(ITemplateTag templateTag, T value, string formatString);

public class PropertyTagClass<TClass> : TagClass
{
	public PropertyTagClass(bool caseSensative = true) : base(typeof(TClass), caseSensative) { }

	/// <summary>
	/// Register a nullable value type property.
	/// </summary>
	/// <typeparam name="U">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="propertyGetter">A Func to get the property value from <see cref="TClass"/></param>
	/// <param name="formatter">Optional formatting function that accepts the <typeparamref name="U"/> property and a formatting string and returnes the value formatted to string</param>
	public void Add<U>(ITemplateTag templateTag, Func<TClass, U?> propertyGetter, PropertyFormatter<U> formatter = null)
		where U : struct
		=> RegisterProperty(templateTag, propertyGetter, formatter);

	/// <summary>
	/// Register a non-nullable value type property
	/// </summary>
	/// <typeparam name="U">Type of the property from <see cref="TClass"/></typeparam>
	/// <param name="propertyGetter">A Func to get the property value from <see cref="TClass"/></param>
	/// <param name="formatter">Optional formatting function that accepts the <typeparamref name="U"/> property and a formatting string and returnes the value formatted to string</param>
	public void Add<U>(ITemplateTag templateTag, Func<TClass, U> propertyGetter, PropertyFormatter<U> formatter = null)
		where U : struct
		=> RegisterProperty(templateTag, propertyGetter, formatter);

	/// <summary>
	/// Register a string type property.
	/// </summary>
	/// <param name="propertyGetter">A Func to get the string property from <see cref="TClass"/></param>
	/// <param name="formatter">Optional formatting function that accepts the string property and a formatting string and returnes the value formatted to string</param>
	public void Add(ITemplateTag templateTag, Func<TClass, string> propertyGetter, PropertyFormatter<string> formatter = null)
		=> RegisterProperty(templateTag, propertyGetter, formatter);

	private void RegisterProperty(ITemplateTag templateTag, Delegate propertyGetter, Delegate formatter)
	{
		if (formatter?.Target is not null)
			throw new ArgumentException($"{nameof(formatter)} must be a static method");

		var expr = Expression.Call(Expression.Constant(propertyGetter.Target), propertyGetter.Method, Parameter);

		AddPropertyTag(new PropertyTag(templateTag, Options, expr, formatter?.Method));
	}

	private class PropertyTag : TagBase
	{
		private readonly Func<Expression, Type, string, Expression> createToStringExpression;

		public PropertyTag(ITemplateTag templateTag, RegexOptions options, Expression propertyExpression, MethodInfo formatter)
			: base(templateTag, propertyExpression)
		{
			var regexStr = formatter is null ? @$"^<{TemplateTag.TagName}>" : @$"^<{TemplateTag.TagName.Replace(" ", "\\s*?")}\s*?(?:\[([^\[\]]*?)\]\s*?)?>";
			NameMatcher = new Regex(regexStr, options);

			//Create the ToString() expression for the TagBase.ExpressionValue's type.
			//If a formatter delegate was registered for this property, use that.
			//Otherwise use the object.Tostring() method.
			createToStringExpression
				= formatter is null
				? (expValue, retTyp, format) => Expression.Call(expValue, retTyp.GetMethod(nameof(object.ToString), Array.Empty<Type>()))
				: (expValue, retTyp, format) => Expression.Call(null, formatter, Expression.Constant(templateTag), expValue, Expression.Constant(format));			
		}

		protected override Expression GetTagExpression(string exactName, string formatString)
		{
			var underlyingType = Nullable.GetUnderlyingType(ReturnType);

			Expression toStringExpression
				= ReturnType == typeof(string)
				? createToStringExpression(Expression.Coalesce(ExpressionValue, Expression.Constant("")), ReturnType, formatString)
				: underlyingType is null
				? createToStringExpression(ExpressionValue, ReturnType, formatString)
				: Expression.Condition(
					Expression.PropertyOrField(ExpressionValue, "HasValue"),
					createToStringExpression(Expression.PropertyOrField(ExpressionValue, "Value"), underlyingType, formatString),
					Expression.Constant(""));

			return Expression.TryCatch(toStringExpression, Expression.Catch(typeof(Exception), Expression.Constant(exactName)));
		}
	}
}
