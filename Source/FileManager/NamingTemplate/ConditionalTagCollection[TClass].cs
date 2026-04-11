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
		private static readonly TimeSpan RegexpCheckTimeout = TimeSpan.FromMilliseconds(100);

		public override Regex NameMatcher { get; }
		public Regex NameCloseMatcher { get; }

		private Func<string, string?, string?, Expression> CreateConditionExpression { get; }

		public ConditionalTag(ITemplateTag templateTag, RegexOptions options, Expression conditionExpression)
			: base(templateTag, conditionExpression)
		{
			var tagNameRe = TagNameForRegex();
			NameMatcher = new Regex($"^<(?<not>!)?{tagNameRe}->", options);
			NameCloseMatcher = new Regex($"^<-{tagNameRe}>", options);

			CreateConditionExpression = (_, _, _) => conditionExpression;
		}

		public ConditionalTag(ITemplateTag templateTag, RegexOptions options, ParameterExpression parameter, ValueProvider<TClass> valueProvider, ConditionEvaluator conditionEvaluator)
			: base(templateTag, Expression.Constant(false))
		{
			// <property> needs to match on at least one character, which is not a space
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

			CreateConditionExpression = (_, property, _)
				=> ConditionEvaluatorCall(templateTag, parameter, valueProvider, property, conditionEvaluator);
		}

		public ConditionalTag(ITemplateTag templateTag, RegexOptions options, ParameterExpression parameter, ValueProvider<TClass> valueProvider)
			: base(templateTag, Expression.Constant(false))
		{
			// <property> needs to match on at least one character, which is not a space.
			// though we will capture the group named `check` enclosed in [] at the end of the tag, the property itself might also have a [] part for formatting purposes
			NameMatcher = new Regex($"""
			                         (?x)                     # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
			                         ^<(?<not>!)?             # tags start with a '<'. Condtionals allow an optional ! captured in <not> to negate the condition
			                         {TagNameForRegex()}      # next the tagname needs to be matched with space being made optional. Also escape all '#'
			                         (?:\s+                   # the following part is optional. If present it starts with some whitespace
			                             (?<property>.+?      # - capture the <property> non greedy so it won't end on whitespace, '[' or '-' (if match is possible)
			                                 (?<!\s))         # - don't let <property> end with a whitepace. Otherwise "<tagname  [foobar]->" would be matchable.
			                             (?:\s*\[\s*          # optional check details enclosed in '[' and ']'. Check shall start with an operator. So match whitespace first
			                                 (?<check>        # - capture inner part as <check>
			                                     (?:\\.       # - '\' escapes always the next character. Especially further '\' and the closing ']'
			                                     |[^\\\]])* ) # - match any character except '\' and ']'. Check may end in whitespace!
			                             \])?                 # - closing the check part
			                         )?                       # end of optional property and check part
			                         \s*->                    # Opening tags end with '->' and closing tags begin with '<-', so both sides visually point toward each other
			                         """
				, options);
			NameCloseMatcher = new Regex($"^<-{templateTag.TagName}>", options);

			CreateConditionExpression = (exactName, property, checkString) =>
			{
				var conditionEvaluator = GetPredicate(exactName, checkString);
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

		private static ConditionEvaluator GetPredicate(string exactName, string? checkString)
		{
			if (checkString == null)
				return (v, _) => v switch
				{
					null => false,
					IEnumerable<object> e => e.Any(),
					_ => !string.IsNullOrWhiteSpace(v.ToString())
				};

			var match = CheckRegex().Match(checkString);

			var valStr = Unescape(match.Groups["val"]) ?? "";
			var iVal = -1;
			var isNumericalOperator = match.Groups["num_op"].Success && int.TryParse(valStr, out iVal);

			var checkItem = Unescape(match.Groups["op"]) switch
			{
				"=" or "" => (v, culture) => VComparedToStr(v, culture, valStr) == 0,
				"!=" or "!" => (v, culture) => VComparedToStr(v, culture, valStr) != 0,
				"~" => GetRegExpCheck(exactName, valStr),
				"#=" => (v, _) => VAsInt(v) == iVal,
				"#!=" => (v, _) => VAsInt(v) != iVal,
				"#>=" or ">=" => (v, _) => VAsInt(v) >= iVal,
				"#>" or ">" => (v, _) => VAsInt(v) > iVal,
				"#<=" or "<=" => (v, _) => VAsInt(v) <= iVal,
				"#<" or "<" => (v, _) => VAsInt(v) < iVal,
				_ => (v, _) => !string.IsNullOrWhiteSpace(v.ToString())
			};
			return isNumericalOperator
				? (v, culture) => v switch
				{
					null => false,
					IEnumerable<object> e => checkItem(e.Count(), culture),
					string s => checkItem(s.Length, culture),
					TimeSpan ts => checkItem(ts.TotalMinutes, culture),
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
			return culture.CompareInfo.Compare(v?.ToString(), valStr, CompareOptions.IgnoreCase);
		}

		/// <summary>
		/// Build a regular expression check. Uses culture-invariant matching for thread-safety and consistency.
		/// Applies a timeout to prevent regex patterns from causing excessive backtracking and blocking.
		/// Throws InvalidOperationException if the regex pattern is invalid or evaluation times out.
		/// </summary>
		/// <param name="exactName">The full tag string for context in error messages</param>
		/// <param name="pattern">The regex pattern to match</param>
		/// <returns>check function to validate an object</returns>
		/// <exception cref="InvalidOperationException">Thrown when regex parsing fails or when regex matching times out, indicating faulty user input</exception>
		private static Func<object, CultureInfo?, bool> GetRegExpCheck(string exactName, string pattern)
		{
			Regex regex;
			try
			{
				// Compile regex with timeout to prevent catastrophic backtracking
				regex = new Regex(pattern,
					RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled,
					RegexpCheckTimeout);
			}
			catch (ArgumentException ex)
			{
				// If regex compilation fails, throw as faulty user input
				var errorMessage = BuildErrorMessage(exactName, pattern, "Invalid regular expression pattern. Correct the pattern and escaping or remove that condition");
				throw new InvalidOperationException(errorMessage, ex);
			}

			return (v, _) =>
			{
				try
				{
					// CultureInfo parameter is intentionally ignored (discarded with _).
					// RegexOptions.CultureInvariant ensures culture-independent matching for predictable behavior.
					// This is preferred for template conditions because:
					// 1. Thread-safety: Regex operations are isolated and don't depend on thread-local culture
					// 2. Consistency: Template matches produce identical results regardless of system locale
					// 3. Predictability: Rules don't unexpectedly change based on user's OS settings
					//
					// Culture-sensitive matching would be problematic in cases like:
					// - Turkish locale: 'I' has different case folding (I ↔ ı vs. I ↔ i). Pattern "[i-z]" might match Turkish 'ı'.
					// - German locale: ß might be treated as equivalent to 'ss' during case-insensitive matching.
					// - Lithuanian locale: 'i' after 'ž' has an accent that affects sorting/matching.
					// 
					// For naming templates, culture-invariant is the safer default.
					return regex.IsMatch(v.ToString() ?? "");
				}
				catch (RegexMatchTimeoutException ex)
				{
					// Throw if regex evaluation times out, indicating faulty user input (e.g., catastrophic backtracking)
					var errorMessage = BuildErrorMessage(exactName, pattern, "Regular expression pattern evaluation timed out. Use a simpler pattern or remove that condition");
					throw new InvalidOperationException(errorMessage, ex);
				}
			};
		}

		private static string BuildErrorMessage(string exactName, string pattern, string errorType)
		{
			const int maxMessageLen = 200;

			// Build full message with pattern
			var fullMsg = $"{errorType}: {exactName} -> Pattern: {pattern}";

			// Return full message if it's within the character limit
			if (fullMsg.Length <= maxMessageLen) return fullMsg;

			// Keep the error type and as much pattern as possible
			var maxPatternLen = maxMessageLen - errorType.Length - 23; // Account for ". Pattern starts with: "
			var trimmedPattern = pattern.Length > maxPatternLen ? pattern[..(maxPatternLen - 3)] + "..." : pattern;
			return $"{errorType}. Pattern starts with: {trimmedPattern}";

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
				matchData.GetValueOrDefault("check")?.ValueOrNull());
			return matchData["not"].Success ? Expression.Not(getBool) : getBool;
		}

		[GeneratedRegex("""
		                (?x)						          # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
		                ^(?<op>(?<num_op>                     # anchor at start of linecapture operator in <op> and <num_op> with every char escapable
		                	    \\?\#(?:\\?!)?\\?=            # - numerical operators: #= #!=
		                	    | \\?\#\\?[<>](?:\\?=)?       # - numerical operators: #>= #<= #> #<
		                	    |      \\?[<>](?:\\?=)?       # - numerical operators: >= <= > <
		                    ) | \\?~|\\?!(?:\\?=)?|(?:\\?=)?  # - string comparison operators including ~ for regexp, = and !=. No operator is like =
		                ) \s*?                                # ignore space between operator and value
		                (?<val>(?(num_op)                     # capture value in <val>
		                	(?:\\?\d)+                        # - numerical operators have to be followed by a number
		                	| (?:\\.|[^\\])* )                # - string for comparison. May be empty. Capturing also all whitespace up to the end as this must have been escaped.
		                )$                                    # match to the end
		                """)]
		private static partial Regex CheckRegex();
	}
}
