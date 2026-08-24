using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Tokenattributes;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibationSearchEngine;

internal static partial class QuerySanitizer
{
	private static readonly HashSet<string> idTerms
		= SearchEngine.FieldIndexRules.IdFieldNames
		.Select(n => n.ToLowerInvariant())
		.ToHashSet();

	private static readonly HashSet<string> boolTerms
		= SearchEngine.FieldIndexRules.BoolFieldNames
		.Select(n => n.ToLowerInvariant())
		.ToHashSet();

	private static readonly HashSet<string> numberTerms
		= SearchEngine.FieldIndexRules.NumberFieldNames
		.Select(n => n.ToLowerInvariant())
		.ToHashSet();

	private static readonly HashSet<string> fieldTerms
		= SearchEngine.FieldIndexRules
		.SelectMany(r => r.FieldNames)
		.Select(n => n.ToLowerInvariant())
		.ToHashSet();

	private static readonly Regex tagRegex = TagRegex();

	internal static string Sanitize(string searchString, StandardAnalyzer analyzer)
	{
		if (string.IsNullOrWhiteSpace(searchString))
			return SearchEngine.ALL_QUERY;

		//Replace a block tags with tags with proper tag query syntax
		//eg: [foo] -> tags:foo
		searchString = tagRegex.Replace(searchString, $"{SearchEngine.TAGS}:$1 ");

		// range operator " TO " and bool operators " AND " and " OR " must be uppercase
		searchString
			= searchString
			.Replace(" to ", " TO ", System.StringComparison.OrdinalIgnoreCase)
			.Replace(" and ", " AND ", System.StringComparison.OrdinalIgnoreCase)
			.Replace(" or ", " OR ", System.StringComparison.OrdinalIgnoreCase);

		using var tokenStream = analyzer.TokenStream(SearchEngine.ALL, new System.IO.StringReader(searchString));

		var partList = new List<string>();
		int previousEndOffset = 0, rangeDepth = 0;
		bool previousIsBool = false, previousIsTags = false, previousIsAsin = false, previousIsField = false, previousIsNumberField = false, inPhrase = false;

		while (tokenStream.IncrementToken())
		{
			var term = tokenStream.GetAttribute<ITermAttribute>().Term;
			var offset = tokenStream.GetAttribute<IOffsetAttribute>();

			var betweenTokens = searchString.Substring(previousEndOffset, offset.StartOffset - previousEndOffset);

			//Neither a range nor a phrase can hold the "(x OR y)" a bare number expands to below, so keep
			//track of the punctuation that opens one. The analyzer throws punctuation away, which is why
			//this reads the untokenized text between the tokens rather than the tokens themselves.
			foreach (var c in betweenTokens)
			{
				if (c is '[' or '{') rangeDepth++;
				else if (c is ']' or '}') rangeDepth = System.Math.Max(0, rangeDepth - 1);
				else if (c is '"') inPhrase = !inPhrase;
			}

			//A colon right after a field name makes this term that field's value, whatever it is called.
			//Plenty of field names are ordinary words a title or a category can contain, and without this
			//they were read as a second field name: "title:absent" became "title:absent:True", which Lucene
			//cannot parse at all. The bool, ASIN and tag fields have their own handling below, which runs
			//first and stays in charge of its own value.
			var isFieldValue = previousIsField && betweenTokens.StartsWith(':');

			if (previousIsBool && !isFieldValue && !bool.TryParse(term, out _))
			{
				//The previous term was a boolean tag and this term is NOT a bool value
				//Add the default ":True" bool and continue parsing the current term
				partList.Add(":True");
				previousIsBool = false;
			}

			//Add all text between the current token and the previous token
			partList.Add(betweenTokens);

			previousIsField = false;

			if (previousIsBool)
			{
				//The previous term was a boolean tag and this term is a bool value
				addUnalteredToken(offset);
				previousIsBool = false;
			}
			else if (previousIsAsin)
			{
				//The previous term was an ASIN field ID, so this term is an ASIN
				partList.Add(term);
				previousIsAsin = false;
			}
			else if (previousIsTags)
			{
				//This term is a tag. Do this check before checking if term is a defined field
				//so that "tags:israted" does not parse as a bool
				addUnalteredToken(offset);
				previousIsTags = false;
			}
			else if (double.TryParse(term, out var num))
			{
				//Which spelling of a number to search for depends on what it is being compared against.
				//A number field is indexed zero-padded so that a range sorts correctly; every other field
				//is indexed as written. Padding regardless meant "title:1984" looked for "00001984.00" in
				//the titles and found nothing, and a bare "1984" could never find the novel.
				var padded = num.ToLuceneString();
				var asWritten = searchString.Substring(offset.StartOffset, offset.EndOffset - offset.StartOffset);

				if (isFieldValue)
					partList.Add(previousIsNumberField ? padded : asWritten);
				else if (rangeDepth > 0)
					//Only the padded spelling sorts, and a range is numeric by nature
					partList.Add(padded);
				else if (inPhrase)
					//A phrase is text by nature
					partList.Add(asWritten);
				else
					//No field was named, so this searches the default field, which holds both spellings:
					//the novel's title as written, and every number field padded. Search for both.
					partList.Add($"({asWritten} OR {padded})");
			}
			else if (!isFieldValue && fieldTerms.Contains(term))
			{
				//Term is a defined search field, add it.
				//The StandardAnalyzer already converts all terms to lowercase
				partList.Add(term);
				previousIsBool = boolTerms.Contains(term);
				previousIsAsin = idTerms.Contains(term);
				previousIsTags = term == SearchEngine.TAGS;
				previousIsNumberField = numberTerms.Contains(term);
				previousIsField = true;
			}
			else
			{
				//Term is any other user-defined constant value
				addUnalteredToken(offset);
			}

			previousEndOffset = offset.EndOffset;
		}

		if (previousIsBool)
			partList.Add(":True");

		//Add ending non-token text
		partList.Add(searchString.Substring(previousEndOffset, searchString.Length - previousEndOffset));

		return string.Concat(partList);

		//Add the full, unaltered token as well as all inter-token text
		void addUnalteredToken(IOffsetAttribute offset) =>
			partList.Add(searchString.Substring(offset.StartOffset, offset.EndOffset - offset.StartOffset));
	}

	[GeneratedRegex(@"(?<!\\)\[\u0020*(\w+)\u0020*\]", RegexOptions.Compiled)]
	private static partial Regex TagRegex();
}
