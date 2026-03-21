using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
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

public delegate bool ConditionEvaluator(object? value, CultureInfo? culture);

public partial class ConditionalTagCollection<TClass>(bool caseSensitive = true) : TagCollection(typeof(TClass), caseSensitive)
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

	private partial class ConditionalTag : TagBase, IClosingPropertyTag
	{
		public override Regex NameMatcher { get; }
		public Regex NameCloseMatcher { get; }

		private Func<string?, string?, Expression> CreateConditionExpression { get; }

		public ConditionalTag(ITemplateTag templateTag, RegexOptions options, Expression conditionExpression)
			: base(templateTag, conditionExpression)
		{
			var tagNameRe = TagNameForRegex();
			NameMatcher = new Regex($"^<(?<not>!)?{tagNameRe}->", options);
			NameCloseMatcher = new Regex($"^<-{tagNameRe}>", options);

			CreateConditionExpression = (_, _) => conditionExpression;
		}

		public ConditionalTag(ITemplateTag templateTag, RegexOptions options, ParameterExpression parameter, ValueProvider<TClass> valueProvider, ConditionEvaluator conditionEvaluator)
			: base(templateTag, Expression.Constant(false))
		{
			// <property> needs to match on at least one character which is not a space
			NameMatcher = new Regex($"""
			                         (?x)                     # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
			                         ^<(?<not>!)?             # tags start with a '<'. Condtionals allow an optional ! captured in <not> to negate the condition
			                         {TagNameForRegex()}      # next the tagname needs to be matched with space being made optional. Also escape all '#'
			                         (?:\s+                   # the following part is optional. If present it starts with some whitespace
			                             (?<property>.+?)     # - capture the <property> non greedy so it won't end on whitespace, '[' or '-' (if match is possible)
			                         )?                       # end of optional property and check part
			                         \s*->                    # Opening tags end with '->' and closing tags begin with '<-', so both sides visually point toward each other
			                         """
				, options);
			NameCloseMatcher = new Regex($"^<-{templateTag.TagName}>", options);

			CreateConditionExpression = (property, _)
				=> ConditionEvaluatorCall(templateTag, parameter, valueProvider, property, conditionEvaluator);
		}

		public ConditionalTag(ITemplateTag templateTag, RegexOptions options, ParameterExpression parameter, ValueProvider<TClass> valueProvider)
			: base(templateTag, Expression.Constant(false))
		{
			// <property> needs to match on at least one character which is not a space
			// though we will capture check enclosed in [] at the end of the tag the property itself migth also have a [] part for formatting purposes
			NameMatcher = new Regex($"""
			                         (?x)                     # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
			                         ^<(?<not>!)?             # tags start with a '<'. Condtionals allow an optional ! captured in <not> to negate the condition
			                         {TagNameForRegex()}      # next the tagname needs to be matched with space being made optional. Also escape all '#'
			                         (?:\s+                   # the following part is optional. If present it starts with some whitespace
			                             (?<property>.+?      # - capture the <property> non greedy so it won't end on whitespace, '[' or '-' (if match is possible)
			                                 (?<!\s))         # - don't let <property> end with a whitepace. Otherwise "<tagname  [foobar]->" would be matchable.
			                             (?:\s*\[\s*          # optional check details enclosed in '[' and ']'. Check shall be trimmed. So match whitespace first
			                                 (?<check>        # - capture inner part as <check>
			                                     (?:\\.       # - '\' escapes allways the next character. Especially further '\' and the closing ']'
			                                     |[^\\\]])*?  # - match any character except '\' and ']' non greedy so the match won't end whith whitespace
			                                 )\s*             # - the whitespace after the check is optional
			                             \])?                 # - closing the check part
			                         )?                       # end of optional property and check part
			                         \s*->                    # Opening tags end with '->' and closing tags begin with '<-', so both sides visually point toward each other
			                         """
				, options);
			NameCloseMatcher = new Regex($"^<-{templateTag.TagName}>", options);

			CreateConditionExpression = (property, checkString) =>
			{
				var conditionEvaluator = GetPredicate(checkString);
				return ConditionEvaluatorCall(templateTag, parameter, valueProvider, property, conditionEvaluator);
			};
		}

		private static MethodCallExpression ConditionEvaluatorCall(ITemplateTag templateTag, ParameterExpression parameter, ValueProvider<TClass> valueProvider, string? property,
			ConditionEvaluator conditionEvaluator)
		{
			return Expression.Call(
				conditionEvaluator.Target is null ? null : Expression.Constant(conditionEvaluator.Target),
				conditionEvaluator.Method,
				ValueProviderCall(templateTag, parameter, valueProvider, property),
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

		private static ConditionEvaluator GetPredicate(string? checkString)
		{
			if (checkString == null)
				return DefaultPredicate;

			var match = CheckRegex().Match(checkString);

			var valStr = match.Groups["val"].Value;
			var ival = -1;
			var isNumop = match.Groups["numop"].Success && int.TryParse(valStr, out ival);

			var checkItem = match.Groups["op"].ValueSpan switch
			{
				"=" or "" => (v, culture) => VComparedToStr(v, culture, valStr) == 0,
				"!=" or "!" => (v, culture) => VComparedToStr(v, culture, valStr) != 0,
				"~" => GetRegExpCheck(valStr),
				"#=" => (v, _) => VAsInt(v) == ival,
				"#!=" => (v, _) => VAsInt(v) != ival,
				"#>=" or ">=" => (v, _) => VAsInt(v) >= ival,
				"#>" or ">" => (v, _) => VAsInt(v) > ival,
				"#<=" or "<=" => (v, _) => VAsInt(v) <= ival,
				"#<" or "<" => (v, _) => VAsInt(v) < ival,
				_ => DefaultPredicate,
			};
			return isNumop
				? (v, culture) => v switch
				{
					null => false,
					IEnumerable<object> e => checkItem(e.Count(), culture),
					string s => checkItem(s.Length, culture),
					_ => checkItem(v, culture)
				}
				: (v, culture) => v switch
				{
					null => false,
					IEnumerable<object> e => e.Any(o => checkItem(o, culture)),
					_ => checkItem(v, culture)
				};

			int? VAsInt(object v) => v is int iv ? iv : int.TryParse(v.ToString(), out var parsed) ? parsed : null;
		}

		private static int VComparedToStr(object? v, CultureInfo? culture, string valStr)
		{
			culture ??= CultureInfo.CurrentCulture;
			return culture.CompareInfo.Compare(v?.ToString()?.Trim(), valStr, CompareOptions.IgnoreCase);
		}

		/// <summary>
		/// build a regular expression check which take the <see cref="CultureInfo"/> into account.
		/// </summary>
		/// <param name="valStr"></param>
		/// <returns>check function to validate an object</returns>
		private static ConditionEvaluator GetRegExpCheck(string valStr)
		{
			return (v, culture) =>
			{
				var old = CultureInfo.CurrentCulture;
				try
				{
					CultureInfo.CurrentCulture = culture ?? CultureInfo.CurrentCulture;
					return Regex.IsMatch(v?.ToString().Trim() ?? "", valStr, RegexOptions.IgnoreCase | RegexOptions.Compiled);
				}
				finally
				{
					CultureInfo.CurrentCulture = old;
				}
			};
		}

		// without any special check only the existance of the property is checked. Strings need to be non empty.
		private static readonly ConditionEvaluator DefaultPredicate = (v, _) => v switch
		{
			null => false,
			IEnumerable<object> e => e.Any(),
			_ => !string.IsNullOrWhiteSpace(v.ToString())
		};

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
				matchData.GetValueOrDefault("property")?.Value,
				Unescape(matchData.GetValueOrDefault("check")));
			return matchData["not"].Success ? Expression.Not(getBool) : getBool;
		}

		[GeneratedRegex("""
		                (?x)						                    # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
		                ^\s*                                            # anchor at start of line trimming leading whitespace
		                (?<op>                                          # capture operator in <op> and <numop>
		                	(?<numop>\#=|\#!=|\#?>=|\#?>|\#?<=|\#?<)    # - numerical operators start with a # and might be omitted if unique
		                	| ~|!=?|=?                                  # - string comparison operators including ~ for regexp. No operator is like =
		                ) \s*                                           # ignore space between operator and value
		                (?<val>(?(numop)                                # capture value in <val>
		                	\d+                                         # - numerical operators have to be followed by a number
		                	| .*? )                                     # - string for comparison. May be empty. Non-greedy capture resulting in no whitespace at the end
		                )\s*$                                           # trimming up to the end
		                """)]
		private static partial Regex CheckRegex();
	}
}
