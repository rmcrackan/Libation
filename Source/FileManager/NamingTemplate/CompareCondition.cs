using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileManager.NamingTemplate;

public static partial class CompareCondition
{
	private static readonly ConcurrentDictionary<string, Regex> RegexCache = new();

	private static readonly TimeSpan RegexpCheckTimeout = TimeSpan.FromMilliseconds(100);

	public static (object, ConditionEvaluator) GetPredicateAndValue(string exactName, string? checkString)
	{
		if (checkString is null)
			return (string.Empty, (v1, _, _) => v1 switch
			{
				null => false,
				IEnumerable<object> e => e.Any(),
				_ => !string.IsNullOrWhiteSpace(v1.ToString())
			});

		var match = GetMatch(exactName, checkString);
		var valStr = match.UnescapeValue("val");
		var (evaluator, opGroup) = GetPredicate(exactName, match);

		return (opGroup.Name switch
		{
			"num_op" => int.Parse(valStr), // at this stage <val> should have matched digits in CheckRegex
			"list_op" => new[] { valStr },
			_ => valStr
		}, evaluator);
	}

	public static ConditionEvaluator GetPredicate(string exactName, string? checkString)
	{
		return GetPredicate(exactName, GetMatch(exactName, checkString)).Item1;
	}

	private static Match GetMatch(string exactName, string? checkString)
	{
		return CheckRegex().TryMatch(checkString, out var match)
			? match
			: throw new ArgumentException($"Invalid check or operator format in conditional tag '{exactName}'. Check string: '{checkString}'");
	}

	private static (ConditionEvaluator, Group) GetPredicate(string exactName, Match match)
	{
		var group = match.Groups["num_op"];
		if (group.Success)
		{
			return (GetPredicateForNumOp(exactName, group.ValueSpan), group);
		}

		group = match.Groups["list_op"];
		if (group.Success)
		{
			return (GetPredicateForListOp(exactName, group.ValueSpan), group);
		}

		group = match.Groups["op"];
		return (GetPredicateForStringOp(exactName, group.ValueSpan), group);
	}

	private static ConditionEvaluator GetPredicateForNumOp(string _, ReadOnlySpan<char> opString)
	{
		Func<int, int, CultureInfo?, bool> checkInt = opString switch
		{
			"#=" => (v1, v2, _) => v1 == v2,
			"#!=" or "≠" or "≠" => (v1, v2, _) => v1 != v2,
			"#>=" or ">=" or "≥" => (v1, v2, _) => v1 >= v2,
			"#>" or ">" => (v1, v2, _) => v1 > v2,
			"#<=" or "<=" or "≤" => (v1, v2, _) => v1 <= v2,
			"#<" or "<" => (v1, v2, _) => v1 < v2,
			_ => throw new ArgumentOutOfRangeException() // this should never happen because the regex only allows these values
		};
		return (v1, v2, culture) => ToIntObject(v1) is { } i1 && ToIntObject(v2) is { } i2 && checkInt(i1, i2, culture);
	}

	private static int? ToIntObject(object? value)
	{
		return value switch
		{
			null => null,
			IEnumerable<object> e => e.Count(),
			TimeSpan ts => (int)ts.TotalMinutes,
			DateTime dt => (int)dt.ToOADate(),
			string s => s.Length,
			int i => i,
			_ => null // language and such shall never match
		};
	}

	private static ConditionEvaluator GetPredicateForListOp(string _, ReadOnlySpan<char> opString)
	{
		var checklist = opString switch
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
		return (v1, v2, culture) => v1 is not null && v2 is not null && checklist(ToEnumerable(v1), ToEnumerable(v2), culture);
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

	private static ConditionEvaluator GetPredicateForStringOp(string exactName, ReadOnlySpan<char> opString)
	{
		var checkItem = opString switch
		{
			"=" or "" => GetStringEqCheck(),
			"!=" or "!" => Invert(GetStringEqCheck()),
			"=~" or "~" => GetRegExpCheck(exactName),
			"!~" => Invert(GetRegExpCheck(exactName)),
			_ => throw new ArgumentOutOfRangeException() // this should never happen because the regex only allows these values
		};
		return (v1, v2, culture) => (v1, v2) switch
		{
			(null, _) => false,
			(_, null) => false,
			(IEnumerable<object> e1, _) => e1.Any(l => checkItem(l, v2, culture)),
			(_, IEnumerable<object> e2) => e2.Any(r => checkItem(v1, r, culture)),
			_ => checkItem(v1, v2, culture)
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
		// ReSharper disable PossibleMultipleEnumeration
		return e1.All(l => e2.Contains(l, comparer)) && e2.Any(r => !e1.Contains(r, comparer));
		// ReSharper restore PossibleMultipleEnumeration
	}

	private static Func<T, T, CultureInfo?, bool> Invert<T>(Func<T, T, CultureInfo?, bool> condition) => (v1, v2, culture) => !condition(v1, v2, culture);
	private static Func<T, T, CultureInfo?, bool> Swap<T>(Func<T, T, CultureInfo?, bool> condition) => (v1, v2, culture) => condition(v2, v1, culture);

	private static Func<object, object, CultureInfo?, bool> GetStringEqCheck()
	{
		return (v1, v2, culture) => GetStringComparer(culture).Equals(ValueToString(v1, culture), ValueToString(v2, culture));
	}

	private static string? ValueToString(object value, CultureInfo? culture)
	{
		return value switch
		{
			TimeSpan ts => ts.TotalMinutes.ToString("0", culture),
			IFormattable f => f.ToString(null, culture),
			_ => value.ToString()
		};
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
	/// <returns>check function to validate an object</returns>
	/// <exception cref="InvalidOperationException">Thrown when regex parsing fails or when regex matching times out, indicating faulty user input</exception>
	private static Func<object, object, CultureInfo?, bool> GetRegExpCheck(string exactName)
	{
		return (v1, v2, _) =>
		{
			var pattern = v2.ToString() ?? "";
			var regex = RegexCache.GetOrAdd(pattern, p =>
			{
				try
				{
					// Compile regex with timeout to prevent catastrophic backtracking
					return new Regex(p,
						RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled, RegexpCheckTimeout);
				}
				catch (ArgumentException ex)
				{
					// If regex compilation fails, throw as faulty user input
					var errorMessage = BuildErrorMessage(exactName, p, "Invalid regular expression pattern. Correct the pattern and escaping or remove that condition");
					throw new InvalidOperationException(errorMessage, ex);
				}
			});
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

		// Build a full message with the pattern
		var fullMsg = $"{errorType}: {exactName} -> Pattern: {pattern}";

		// Return a full message if it's within the character limit
		if (fullMsg.Length <= maxMessageLen) return fullMsg;

		// Keep the error type and as much pattern as possible
		var maxPatternLen = maxMessageLen - errorType.Length - 23; // Account for ". Pattern starts with: "
		var trimmedPattern = pattern.Length > maxPatternLen ? pattern[..(maxPatternLen - 3)] + "..." : pattern;
		return $"{errorType}. Pattern starts with: {trimmedPattern}";
	}

	[GeneratedRegex("""
	                (?x)                       # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
	                ^(?>(?<op>(?<list_op>      # anchor at start of line. Capture operator in <op>, <list_op> and <num_op>
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
	                )) \s*?                    # ignore space between operator and value
	                (?<val>(?(num_op)          # capture value in <val>
	                	(?:\d)+                # - numerical operators have to be followed by a number
	                	| (?:\\.|[^\\])+ )     # - string for comparison. May be empty. Capturing also all whitespace up to the end as this must have been escaped.
	                )?$                        # match to the end
	                """)]
	private static partial Regex CheckRegex();
}