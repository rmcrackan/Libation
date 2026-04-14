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

public delegate bool ConditionEvaluator(object? value1, object? value2, CultureInfo? culture);

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

	private partial class ConditionalTag : TagBase, IClosingPropertyTag
	{
		private static readonly TimeSpan RegexpCheckTimeout = TimeSpan.FromMilliseconds(100);

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
			                         ^<(?<not>!)?             # tags start with a '<'. Condtionals allow an optional ! captured in <not> to negate the condition
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
			                         ^<(?<not>!)?             # tags start with a '<'. Condtionals allow an optional ! captured in <not> to negate the condition
			                         {TagNameForRegex()}      # next the tagname needs to be matched with space being made optional. Also escape all '#'
			                         (?:\s+                   # the following part is optional. If present it starts with some whitespace
			                             (?<property>(?:      # capture the <property>
			                                   [^<=~>!]       # - match any character with some exclusions that should only be used in operands
			                             ) +? (?<!\s))        # - don't let <property> end with a whitepace. Otherwise "<tagname  = tag2->" would be matchable.
			                             (?:\s*\[\s*          # optional check details enclosed in '[' and ']'. Check shall start with an operator. So match whitespace first
			                                 (?<check_or_op>  # - capture inner part as <check_or_op>
			                                     (?:\\.       # - '\' escapes allways the next character. Especially further '\' and the closing ']'
			                                     |[^\\\]])* ) # - match any character except '\' and ']'. check_or_op may end in whitespace!
			                             \])?                 # - closing the check_or_op part
			                         )?                       # end of optional property and check_or_op part
			                         \s*->                    # Opening tags end with '->' and closing tags begin with '<-', so both sides visually point toward each other
			                         """
				, options);
			NameCloseMatcher = new Regex($"^<-{templateTag.TagName}>", options);

			CreateConditionExpression = (exactName, property, checkString, _) =>
			{
				var (value, conditionEvaluator) = GetPredicate(exactName, checkString);
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
			                         ^<(?<not>!)?             # tags start with a '<'. Condtionals allow an optional ! captured in <not> to negate the condition
			                         {TagNameForRegex()}      # next the tagname needs to be matched with space being made optional. Also escape all '#'
			                         \s+                      # Separate the following with whitespace
			                         (?<property>             # capture the <property>
			                               '(?:[^']|'')*'     # - allow 'string' to be included in the format, with '' being an escaped ' character
			                             | "(?:[^"]|"")*"     # - allow "string" to be included in the format, with "" being an escaped " character
			                             | (?: \[ (?: \\.     # - properties may have optional formatting details enclosed in '[' and ']'. '\' escapes allways the next character
			                                       | [^\\\]]  #   unescaped characters except ']' and '\' are allowed in the formatting details
			                                       )* \]      #   closing the formatting details part
			                                   | . )+?        # - match any character to form the property name. Capture non greedy so it won't match the operator part.
			                            (?<!\s))              # - don't let <property> end with a whitepace. Otherwise "<tagname  = tag2->" would be matchable.
			                         \s+                      # Separate the following operand with whitespace
			                         (?<check_or_op>          # capture operator in <op> and <num_op> with every char escapable
			                             [\#!≡=≠~<>≤≥&∉∌∈∌⋂⊆⊇⊂⊃-]+  # allow a wide range of operators, all non alphanumeric
			                             | :[a-z_]+:          # allow :named: operators for readability, e.g. :contains:
			                         ) \s+                    # ignore space between operator and second property
			                         (?<second_property>.+?   # - capture the <second_property> non greedy so it won't end on whitespace
			                             (?<!\s))             # - don't let <second_property> end with a whitepace. Otherwise "<tagname tag1 =  ->" would be matchable.
			                         \s*->                    # Opening tags end with '->' and closing tags begin with '<-', so both sides visually point toward each other
			                         """
				, options);
			NameCloseMatcher = new Regex($"^<-{templateTag.TagName}>", options);

			CreateConditionExpression = (exactName, property1, checkString, property2) =>
			{
				var (_, conditionEvaluator) = GetPredicate(exactName, checkString);
				return ConditionEvaluatorCall(conditionEvaluator,
					ValueProviderCall(templateTag, parameter, valueProvider1, property1),
					ValueProviderCall(templateTag, parameter, valueProvider2, property2));
			};
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

		private static (Object, ConditionEvaluator) GetPredicate(string exactName, string? checkString)
		{
			if (checkString is null)
				return ("", (v1, v2, _) => v1 switch
				{
					null => false,
					IEnumerable<object> e => e.Any(),
					_ => !string.IsNullOrWhiteSpace(v1.ToString())
				});

			var match = CheckRegex().Match(checkString);
			var valStr = Unescape(match.Groups["val"]) ?? "";

			if (match.Groups["num_op"].Success)
			{
				Func<int, int, CultureInfo?, bool> checkInt = match.Groups["op"].ValueSpan switch
				{
					"#=" => (v1, v2, _) => v1 == v2,
					"#!=" or "≠" or "≠" => (v1, v2, _) => v1 != v2,
					"#>=" or ">=" or "≥" => (v1, v2, _) => v1 >= v2,
					"#>" or ">" => (v1, v2, _) => v1 > v2,
					"#<=" or "<=" or "≤" => (v1, v2, _) => v1 <= v2,
					"#<" or "<" => (v1, v2, _) => v1 < v2,
					_ => throw new ArgumentOutOfRangeException() // this should never happen because the regex only allows these values
				};
				int.TryParse(valStr, out var valInt);
				return (valInt,
					(v1, v2, culture) => v1 is not null && v2 is not null && checkInt(ToIntObject(v1), ToIntObject(v2), culture));
			}

			if (match.Groups["list_op"].Success)
			{
				var stringEqCheck = GetStringEqCheck();
				Func<IEnumerable<string>, IEnumerable<string>, CultureInfo?, bool> checklist = match.Groups["op"].ValueSpan switch
				{
					"∋" or ">>" or ":contains:" => Swap<IEnumerable<string>>(IsSubset),
					"⊇" or ">=>" or ":superset:" => Swap<IEnumerable<string>>(IsSubset),
					"⊃" or ">->" or ":proper_superset:" => Swap<IEnumerable<string>>(IsProperSubset),
					"∌" or "!>>" or "∌" or ":not_contains:" => Invert(Swap<IEnumerable<string>>(IsSubset)),
					"∈" or "<<" or ":in:" => IsSubset,
					"⊆" or "<=<" or ":subset:" => IsSubset,
					"⊂" or "<-<" or ":proper_subset:" => IsProperSubset,
					"∉" or "!<<" or "∉" or ":not_in:" => Invert<IEnumerable<string>>(IsSubset),
					"⋂" or "&&" or ":overlaps:" => Overlaps,
					"⋂̸" or "&&!" or "⋂!" or ":disjoint:" => Invert<IEnumerable<string>>(Overlaps),
					"≡" or "==" or ":equals:" => (e1, e2, culture) =>
					{
						var cmp = GetStringComparer(culture);
						return e1.OrderBy(e => e, cmp).SequenceEqual(e2.OrderBy(e => e, cmp), cmp);
					},
					_ => throw new ArgumentOutOfRangeException() // this should never happen because the regex only allows these values
				};
				return (new[] { valStr },
					(v1, v2, culture) => v1 is not null && v2 is not null && checklist(ToEnumerable(v1), ToEnumerable(v2), culture));
			}

			Func<object, object, CultureInfo?, bool> checkItem = match.Groups["op"].Value switch
				{
					"=" or "" => GetStringEqCheck(),
					"!=" or "!" => Invert(GetStringEqCheck()),
					"=~" or "~" => GetRegExpCheck(exactName),
					"!~" => Invert(GetRegExpCheck(exactName)),
					_ => throw new ArgumentOutOfRangeException() // this should never happen because the regex only allows these values
				};
			return (valStr,
				(v1, v2, culture) => (v1, v2) switch
				{
					(null, _) => false,
					(_, null) => false,
					(IEnumerable<object> e1, _) => e1.Any(l => checkItem(l, v2, culture)),
					(_, IEnumerable<object> e2) => e2.Any(r => checkItem(v1, r, culture)),
					_ => checkItem(v1, v2, culture)
				});
		}

		private static int ToIntObject(object value)
		{
			return value switch
			{
				IEnumerable<object> e => e.Count(),
				TimeSpan ts => (int)ts.TotalMinutes,
				string s => s.Length,
				int i => i,
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		private static IEnumerable<string> ToEnumerable(object value)
		{
			return value switch
			{
				IEnumerable<string> e => e,
				IEnumerable<object> e => e.Select(o => o.ToString() ?? ""),
				string s => [s],
				_ => [value.ToString() ?? ""]
			};
		}

		private static bool Overlaps(IEnumerable<string> e1, IEnumerable<string> e2, CultureInfo? culture)
		{
			var comparer = GetStringComparer(culture);
			return e1.Any(l => e2.Contains(l, comparer));
		}

		private static bool IsSubset(IEnumerable<string> e1, IEnumerable<string> e2, CultureInfo? culture)
		{
			var comparer = GetStringComparer(culture);
			return e1.All(l => e2.Contains(l, comparer));
		}

		private static bool IsProperSubset(IEnumerable<string> e1, IEnumerable<string> e2, CultureInfo? culture)
		{
			var comparer = GetStringComparer(culture);
			return e1.All(l => e2.Contains(l, comparer)) && e2.Any(r => !e1.Contains(r, comparer));
		}

		private static Func<T, T, CultureInfo?, bool> Invert<T>(Func<T, T, CultureInfo?, bool> condition) => (v1, v2, culture) => !condition(v1, v2, culture);
		private static Func<T, T, CultureInfo?, bool> Swap<T>(Func<T, T, CultureInfo?, bool> condition) => (v1, v2, culture) => condition(v2, v1, culture);

		private static Func<object, object, CultureInfo?, bool> GetStringEqCheck()
		{
			return (v1, v2, culture) => GetStringComparer(culture).Equals(v1?.ToString(), v2.ToString());
		}

		private static StringComparer GetStringComparer(CultureInfo? culture)
		{
			return StringComparer.Create(culture ?? CultureInfo.CurrentCulture, ignoreCase: true);
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
		private static Func<object, object, CultureInfo?, bool> GetRegExpCheck(string exactName)
		{
			return (v1, v2, _) =>
			{
				Regex regex;
				var pattern = v2.ToString() ?? "";
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
					return regex.IsMatch(v1.ToString() ?? "");
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
				matchData.GetValueOrDefault("check_or_op")?.ValueOrNull(),
				matchData.GetValueOrDefault("second_property")?.ValueOrNull());
			return matchData["not"].Success ? Expression.Not(getBool) : getBool;
		}

		[GeneratedRegex("""
		                (?x)                       # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
		                ^(?<op>(?<list_op>         # anchor at start of line. capture operator in <op>, <list_op> and <num_op> with every char escapable
		                          ≡ |  == |      :equals:           # - list operators: ≡ for checking if two lists contain the same items regardless of order
		                        | ∌ | !>> | ∌  | :not_contains:     # - list operators: ∌ for checking if the first list does not contain any item of the second list
		                        | ∋ |  >> |      :contains:         # - list operators: ∋ for checking if the first list contains all items of the second list
		                        | ∉ | !<< | ∉  | :not_in:           # -	list operators: ∉ for checking if the first list is not contained in the second list
		                        | ∈ |  << |      :in:               # - list operators: ∈ for checking if the first list is contained in the second list
		                        | ⋂̸ | &&! | ⋂! | :disjoint:         # - list operators: ⋂̸ for checking if the two lists are disjoint
		                        | ⋂ | &&  |      :overlaps:         # - list operators: ⋂ for checking if the two lists overlap in at least one item
		                        | ⊆ | <=< |      :subset:           # - list operators: ⊆ for checking if the first list is a subset of the second list (may be equal)
		                        | ⊇ | >=> |      :superset:         # - list operators: ⊇ for checking if the first list is a superset of the second list (may be equal)
		                        | ⊂ | <-< |      :proper_subset:    # - list operators: ⊂ for checking if the first list is a proper subset of the second list (not equal)
		                        | ⊃ | >-> |      :proper_superset:  # -	list operators: ⊃ for checking if the first list is a proper superset of the second list (not equal)
		                    ) | (?<num_op>
		                	      \#!?=    | ≠ | ≠ # - numerical operators: #= #!= ≠
		                	    | \#[<>]=?         # - numerical operators: #<= #>= #< #>
		                	    |   [<>]=? | ≤ | ≥ # - numerical operators: <= >= < > ≤ ≥
		                    ) | [=!]?~ | !=? | =?  # - string comparison operators including ~ for regexp, = and !=. No operator is like =
		                ) \s*?                     # ignore space between operator and value
		                (?<val>(?(num_op)          # capture value in <val>
		                	(?:\d)*                # - numerical operators have to be followed by a number
		                	| (?:\\.|[^\\])* )     # - string for comparison. May be empty. Capturing also all whitespace up to the end as this must have been escaped.
		                )$                         # match to the end
		                """)]
		private static partial Regex CheckRegex();
	}
}
