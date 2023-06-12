using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Tokenattributes;
using System.Collections.Generic;
using System.Linq;

namespace LibationSearchEngine
{
	internal static class QuerySanitizer
	{
		private static readonly HashSet<string> idTerms
			= SearchEngine.FieldIndexRules.IdFieldNames
			.Select(n => n.ToLowerInvariant())
			.ToHashSet();

		private static readonly HashSet<string> boolTerms
			= SearchEngine.FieldIndexRules.BoolFieldNames
			.Select(n => n.ToLowerInvariant())
			.ToHashSet();

		private static readonly HashSet<string> fieldTerms
			= SearchEngine.FieldIndexRules
			.SelectMany(r => r.FieldNames)
			.Select(n => n.ToLowerInvariant())
			.ToHashSet();

		internal static string Sanitize(string searchString, StandardAnalyzer analyzer)
		{
			if (string.IsNullOrWhiteSpace(searchString))
				return SearchEngine.ALL_QUERY;

			// range operator " TO " and bool operators " AND " and " OR " must be uppercase
			searchString
				= searchString
				.Replace(" to ", " TO ", System.StringComparison.OrdinalIgnoreCase)
				.Replace(" and ", " AND ", System.StringComparison.OrdinalIgnoreCase)
				.Replace(" or ", " OR ", System.StringComparison.OrdinalIgnoreCase);

			using var tokenStream = analyzer.TokenStream(SearchEngine.ALL, new System.IO.StringReader(searchString));

			var partList = new List<string>();
			int previousEndOffset = 0;
			bool previousIsBool = false, previousIsTags = false, previousIsAsin = false;

			while (tokenStream.IncrementToken())
			{
				var term = tokenStream.GetAttribute<ITermAttribute>().Term;
				var offset = tokenStream.GetAttribute<IOffsetAttribute>();

				if (previousIsBool && !bool.TryParse(term, out _))
				{
					//The previous term was a boolean tag and this term is NOT a bool value
					//Add the default ":True" bool and continue parsing the current term
					partList.Add(":True");
					previousIsBool = false;
				}

				//Add all text between the current token and the previous token
				partList.Add(searchString.Substring(previousEndOffset, offset.StartOffset - previousEndOffset));

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
				else if (tryParseBlockTag(offset, partList, searchString, out var tagName))
				{
					//The term is a block tag. add it to the part list
					partList.Add($"{SearchEngine.TAGS}:{tagName}");
				}
				else if (double.TryParse(term, out var num))
				{
					//Term is a number so pad it with zeros
					partList.Add(num.ToLuceneString());
				}
				else if (fieldTerms.Contains(term))
				{
					//Term is a defined search field, add it.
					//The StandardAnalyzer already converts all terms to lowercase
					partList.Add(term);
					previousIsBool = boolTerms.Contains(term);
					previousIsAsin = idTerms.Contains(term);
					previousIsTags = term == SearchEngine.TAGS;
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

		private static bool tryParseBlockTag(IOffsetAttribute offset, List<string> partList, string searchString, out string tagName)
		{
			tagName = null;
			if (partList.Count == 0) return false;

			var previous = partList[^1].TrimEnd();

			//cannot be preceeded by an escaping \
			if (previous.Length == 0) return false;
			if (previous[^1] != '[' || (previous.Length > 1 && previous[^2] == '\\')) return false;

			var next = searchString.Substring(offset.EndOffset);
			if (next.Length == 0 || !next.TrimStart().StartsWith(']')) return false;

			tagName = searchString.Substring(offset.StartOffset, offset.EndOffset - offset.StartOffset);

			//Only legal tag characters are letters, numbers and underscores
			//Per DataLayer.UserDefinedItem.IllegalCharacterRegex()
			foreach (var c in tagName)
			{
				if (!char.IsLetterOrDigit(c) && c != '_')
					return false;
			}

			//Remove the leading '['
			partList[^1] = previous[..^1];
			//Ignore the trailing ']'
			offset.SetOffset(offset.StartOffset, searchString.IndexOf(']', offset.EndOffset) + 1);
			return true;
		}
	}
}
