using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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

public delegate object? ValueProvider<in T>(ITemplateTag templateTag, T value, string condition, CultureInfo? culture);

public delegate bool ConditionEvaluator(object? value1, object? value2, CultureInfo? culture);

public class ConditionalTagCollection<TClass>(bool caseSensitive = true) : TagCollection(typeof(TClass), caseSensitive)
{
	/// <summary>
	/// Register a conditional tag.
	/// </summary>
	/// <param name="templateTag"></param>
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
	/// <param name="templateTag"></param>
	/// <param name="valueProvider">A <see cref="ValueProvider{T}"/> to get the condition's value</param>
	/// <param name="conditionEvaluator">A <see cref="ConditionEvaluator"/> to evaluate the condition's value</param>
	public void Add(ITemplateTag templateTag, ValueProvider<TClass> valueProvider, ConditionEvaluator conditionEvaluator)
	{
		AddPropertyTag(new ConditionalTag(templateTag, Options, Parameter, valueProvider, conditionEvaluator));
	}

	/// <summary>
	/// Register a conditional tag.
	/// </summary>
	/// <param name="templateTag"></param>
	/// <param name="valueProvider">A <see cref="ValueProvider{T}"/> to get the condition's value. The value will be evaluated by a check specified by the tag itself.</param>
	public void Add(ITemplateTag templateTag, ValueProvider<TClass> valueProvider)
	{
		AddPropertyTag(new ConditionalTag(templateTag, Options, Parameter, valueProvider));
	}

	/// <summary>
	/// Register a conditional tag.
	/// </summary>
	/// <param name="templateTag"></param>
	/// <param name="valueProvider1">A <see cref="ValueProvider{T}"/> to get the first condition's value. The values will be evaluated by a check specified by the tag itself.</param>
	/// <param name="valueProvider2">A <see cref="ValueProvider{T}"/> to get the second condition's value. The values will be evaluated by a check specified by the tag itself.</param>
	public void Add(ITemplateTag templateTag, ValueProvider<TClass> valueProvider1, ValueProvider<TClass> valueProvider2)
	{
		AddPropertyTag(new ConditionalTag(templateTag, Options, Parameter, valueProvider1, valueProvider2));
	}

	private class ConditionalTag : TagBase, IClosingPropertyTag
	{
		public override Regex NameMatcher { get; }
		public Regex NameCloseMatcher { get; }

		private Func<string, string?, string?, string?, Expression> CreateConditionExpression { get; }

		public ConditionalTag(ITemplateTag templateTag, RegexOptions options, Expression conditionExpression)
			: base(templateTag, conditionExpression)
		{
			var tagNameRe = TagNameForRegex();
			NameMatcher = new Regex($"^<(?<not>!)?{tagNameRe}->", options);
			NameCloseMatcher = new Regex($"^<-{tagNameRe}>", options);

			CreateConditionExpression = (_, _, _, _) => conditionExpression;
		}

		public ConditionalTag(ITemplateTag templateTag, RegexOptions options, ParameterExpression parameter, ValueProvider<TClass> valueProvider, ConditionEvaluator conditionEvaluator)
			: base(templateTag, Expression.Constant(false))
		{
			// <property> needs to match on at least one character, which is not a space
			NameMatcher = new Regex($"""
			                         (?x)                     # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
			                         ^<(?<not>!)?             # tags start with a '<'. Conditionals allow an optional ! captured in <not> to negate the condition
			                         {TagNameForRegex()}      # next the tagname needs to be matched with space being made optional. Also escape all '#'
			                         (?:\s+                   # the following part is optional. If present it starts with some whitespace
			                             (?<property>.+?)     # - capture the <property> non greedy so it won't end on whitespace, '[' or '-' (if match is possible)
			                         )?                       # end of optional property and check part
			                         \s*->                    # Opening tags end with '->' and closing tags begin with '<-', so both sides visually point toward each other
			                         """
				, options);
			NameCloseMatcher = new Regex($"^<-{templateTag.TagName}>", options);

			CreateConditionExpression = (_, property, _, _)
				=> ConditionEvaluatorCall(conditionEvaluator,
					ValueProviderCall(templateTag, parameter, valueProvider, property),
					Expression.Constant(null));
		}

		public ConditionalTag(ITemplateTag templateTag, RegexOptions options, ParameterExpression parameter, ValueProvider<TClass> valueProvider)
			: base(templateTag, Expression.Constant(false))
		{
			// <property> needs to match on at least one character, which is not a space.
			// though we will capture the group named `check_or_op` enclosed in [] at the end of the tag, the property itself might also have a [] part for formatting purposes
			NameMatcher = new Regex($"""
			                         (?x)                     # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
			                         ^<(?<not>!)?             # tags start with a '<'. Conditionals allow an optional ! captured in <not> to negate the condition
			                         {TagNameForRegex()}      # next the tagname needs to be matched with space being made optional. Also escape all '#'
			                         (?:\s+                   # the following part is optional. If present it starts with some whitespace
			                             (?<property>.+?      # - capture the <property> non greedy so it won't end on whitespace, '[' or '-' (if match is possible)
			                                 (?<!\s))         # - don't let <property> end with a whitepace. Otherwise "<tagname  [foobar]->" would be matchable.
			                             (?:\s*\[\s*          # optional check details enclosed in '[' and ']'. Check shall start with an operator. So match whitespace first
			                                 (?<check_or_op>  # - capture inner part as <check_or_op>
			                                     (?:\\.       # - '\' escapes always the next character. Especially further '\' and the closing ']'
			                                     |[^\\\]])* ) # - match any character except '\' and ']'. check_or_op may end in whitespace!
			                             \])?                 # - closing the check_or_op part
			                         )?                       # end of optional property and check_or_op part
			                         \s*->                    # Opening tags end with '->' and closing tags begin with '<-', so both sides visually point toward each other
			                         """
				, options);
			NameCloseMatcher = new Regex($"^<-{templateTag.TagName}>", options);

			CreateConditionExpression = (exactName, property, checkString, _) =>
			{
				var (value, conditionEvaluator) = CompareCondition.GetPredicateAndValue(exactName, checkString);
				return ConditionEvaluatorCall(conditionEvaluator,
					ValueProviderCall(templateTag, parameter, valueProvider, property),
					BuildArgument(value, conditionEvaluator.Method.GetParameters()[1].ParameterType));
			};
		}

		public ConditionalTag(ITemplateTag templateTag, RegexOptions options, ParameterExpression parameter, ValueProvider<TClass> valueProvider1, ValueProvider<TClass> valueProvider2)
			: base(templateTag, Expression.Constant(false))
		{
			NameMatcher = new Regex($"""
			                         (?x)                     # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
			                         ^<(?<not>!)?             # tags start with a '<'. Conditionals allow an optional ! captured in <not> to negate the condition
			                         {TagNameForRegex()}      # next the tagname needs to be matched with space being made optional. Also escape all '#'
			                         \s+                      # Separate the following with whitespace
			                         (?<property>             # capture the <property>
			                               '(?:[^']|'')*'     # - allow 'string' to be included in the format, with '' being an escaped ' character
			                             | "(?:[^"]|"")*"     # - allow "string" to be included in the format, with "" being an escaped " character
			                             | (?: \[ (?: \\.     # - properties may have optional formatting details enclosed in '[' and ']'. '\' escapes always the next character
			                                       | [^\\\]]  #   unescaped characters except ']' and '\' are allowed in the formatting details
			                                       )* \]      #   closing the formatting details part
			                                   | . )+?        # - match any character to form the property name. Capture non greedy so it won't match the operator part.
			                            (?<!\s))              # - don't let <property> end with a whitepace. Otherwise "<tagname  = tag2->" would be matchable.
			                         \s+                      # Separate the following operand with whitespace
			                         (?<check_or_op>          # capture operator in <check_or_op>
			                             [\#!≡=≠~<>≤≥&∉∌∈∌⋂⊆⊇⊂⊃-]+  # allow a wide range of operators, all non alphanumeric so that no operator is confused as property
			                             | :[a-z_]+:          # allow :named: operators for readability, e.g. :contains:
			                         ) \s+                    # ignore space between operator and second property
			                         (?<second_property>.+?   # - capture the <second_property> non greedy so it won't end on whitespace
			                             (?<!\s))             # - don't let <second_property> end with a whitepace. Otherwise "<tagname tag1 =  ->" would be matchable.
			                         \s*->                    # Opening tags end with '->' and closing tags begin with '<-', so both sides visually point toward each other
			                         """
				, options);
			NameCloseMatcher = new Regex($"^<-{templateTag.TagName}>", options);

			CreateConditionExpression = (exactName, property1, checkString, property2)
				=> ConditionEvaluatorCall(CompareCondition.GetPredicate(exactName, checkString),
					ValueProviderCall(templateTag, parameter, valueProvider1, property1),
					ValueProviderCall(templateTag, parameter, valueProvider2, property2));
		}

		private static MethodCallExpression ConditionEvaluatorCall(ConditionEvaluator conditionEvaluator, Expression valueExpression1, Expression valueExpression2)
		{
			return Expression.Call(
				conditionEvaluator.Target is null ? null : Expression.Constant(conditionEvaluator.Target),
				conditionEvaluator.Method,
				valueExpression1,
				valueExpression2,
				CultureParameter);
		}

		private static MethodCallExpression ValueProviderCall(ITemplateTag templateTag, ParameterExpression parameter, ValueProvider<TClass> valueProvider, string? property)
		{
			return Expression.Call(
				valueProvider.Target is null ? null : Expression.Constant(valueProvider.Target),
				valueProvider.Method,
				Expression.Constant(templateTag),
				parameter,
				Expression.Constant(property),
				CultureParameter);
		}

		private static Expression BuildArgument(object value, Type targetType)
		{
			var constant = Expression.Constant(value, value.GetType());
			return constant.Type == targetType ? constant : Expression.Convert(constant, targetType);
		}

		// without any special check, only the existence of the property is checked. Strings need to be non-empty.

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

		protected override Expression GetTagExpression(string exactName, Dictionary<string, Group> matchData, OutputType outputType)
		{
			var getBool = CreateConditionExpression(
				exactName,
				matchData.GetValueOrDefault("property")?.Value,
				matchData.GetValueOrDefault("check_or_op")?.ValueOrNull(),
				matchData.GetValueOrDefault("second_property")?.ValueOrNull());
			return matchData["not"].Success ? Expression.Not(getBool) : getBool;
		}
	}
}
