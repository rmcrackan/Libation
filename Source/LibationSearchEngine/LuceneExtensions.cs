using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace LibationSearchEngine
{
    // field names are case specific and, due to StandardAnalyzer, content is case INspecific
    internal static class LuceneExtensions
    {
        internal static void AddRaw(this Document document, string name, string value)
            => document.Add(new Field(name, value, Field.Store.YES, Field.Index.NOT_ANALYZED));

        internal static void AddAnalyzed(this Document document, string name, string value)
        {
            if (value is not null)
                document.Add(new Field(name.ToLowerInvariant(), value, Field.Store.YES, Field.Index.ANALYZED));
        }

        internal static void AddNotAnalyzed(this Document document, string name, string value)
            => document.Add(new Field(name.ToLowerInvariant(), value, Field.Store.YES, Field.Index.NOT_ANALYZED));

        internal static void AddBool(this Document document, string name, bool value)
            => document.Add(new Field(name.ToLowerInvariant(), value.ToString(), Field.Store.YES, Field.Index.ANALYZED_NO_NORMS));

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
