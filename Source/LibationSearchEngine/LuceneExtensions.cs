using System;
using System.Collections.Generic;
using DataLayer;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace LibationSearchEngine
{
    // field names are case specific and, due to StandardAnalyzer, content is case INspecific
    internal static class LuceneExtensions
	{
		internal static void AddAnalyzed(this Document document, string name, string value)
		{
			if (value is not null)
				document.Add(new Field(name.ToLowerInvariant(), value, Field.Store.YES, Field.Index.ANALYZED));
		}        

        internal static void RemoveRule(this Document document, IndexRule rule)
		{
			// fields are key value pairs. MULTIPLE FIELDS CAN POTENTIALLY HAVE THE SAME KEY.
			// ie: must remove old before adding new else will create unwanted duplicates.
			foreach (var name in rule.FieldNames)
				document.RemoveFields(name.ToLowerInvariant());
		}

        internal static void AddIndexRule(this Document document, IndexRule rule, LibraryBook libraryBook)
		{
			if (rule.GetValue(libraryBook) is not string value)
				return;

			foreach (var name in rule.FieldNames)
			{
				// fields are key value pairs and MULTIPLE FIELDS CAN HAVE THE SAME KEY.
				// splitting authors and narrators and/or tags into multiple fields could be interesting research.
				// it could allow for more advanced searches, or maybe it could break broad searches.

				// all searching should be lowercase
				// external callers have the reasonable expectation that product id will be returned CASE SPECIFIC
				var field = rule.FieldType switch
				{
					FieldType.Bool => new Field(name.ToLowerInvariant(), value, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS),
					FieldType.String => new Field(name.ToLowerInvariant(), value, Field.Store.YES, Field.Index.ANALYZED),
					FieldType.Number => new Field(name.ToLowerInvariant(), value, Field.Store.YES, Field.Index.NOT_ANALYZED),
					FieldType.ID => new Field(name.ToLowerInvariant(), value, Field.Store.YES, Field.Index.NOT_ANALYZED),
					FieldType.Raw => new Field(name, value, Field.Store.YES, Field.Index.NOT_ANALYZED),
					_ => throw new KeyNotFoundException(),
				};

				document.Add(field);
			}
		}

        internal static Query GetQuery(this Analyzer analyzer, string defaultField, string searchString)
            => new QueryParser(SearchEngine.Version, defaultField.ToLowerInvariant(), analyzer).Parse(searchString);

        // put all numbers, including dates, into this format:
        // ########.##
        internal const int PAD_DIGITS = 8;
        internal const string DECIMAL_PRECISION = ".00";
        internal static string ToLuceneString(this int i) => ((double)i).ToLuceneString();
        internal static string ToLuceneString(this float f) => ((double)f).ToLuceneString();
        internal static string ToLuceneString(this DateTime dt)
            => dt.ToString("yyyyMMdd") + DECIMAL_PRECISION;
        internal static string ToLuceneString(this DateTime? dt)
            => dt?.ToLuceneString() ?? "";
        internal static string ToLuceneString(this double d)
            => d.ToString("0" + DECIMAL_PRECISION).PadLeft(PAD_DIGITS + DECIMAL_PRECISION.Length, '0');
    }
}
