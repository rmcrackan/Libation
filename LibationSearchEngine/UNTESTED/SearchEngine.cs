using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core;
using FileManager;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace LibationSearchEngine
{
    public class SearchEngine
    {
        public const Lucene.Net.Util.Version Version = Lucene.Net.Util.Version.LUCENE_30;

        // not customizable. don't move to config
        private static string SearchEngineDirectory { get; }
            = new System.IO.DirectoryInfo(Configuration.Instance.LibationFiles).CreateSubdirectory("SearchEngine").FullName;

        public const string _ID_ = "_ID_";
        public const string TAGS = "tags";
        public const string ALL = "all";

        private static ReadOnlyDictionary<string, Func<LibraryBook, string>> idIndexRules { get; }
            = new ReadOnlyDictionary<string, Func<LibraryBook, string>>(
                new Dictionary<string, Func<LibraryBook, string>>
                {
                    [nameof(Book.AudibleProductId)] = lb => lb.Book.AudibleProductId,
                    ["ProductId"] = lb => lb.Book.AudibleProductId,
                    ["Id"] = lb => lb.Book.AudibleProductId,
                    ["ASIN"] = lb => lb.Book.AudibleProductId
                }
                );
        private static ReadOnlyDictionary<string, Func<LibraryBook, string>> stringIndexRules { get; }
            = new ReadOnlyDictionary<string, Func<LibraryBook, string>>(
                new Dictionary<string, Func<LibraryBook, string>>
                {
                    [nameof(LibraryBook.DateAdded)] = lb => lb.DateAdded.ToLuceneString(),
                    [nameof(Book.DatePublished)] = lb => lb.Book.DatePublished?.ToLuceneString(),

                    [nameof(Book.Title)] = lb => lb.Book.Title,
                    [nameof(Book.AuthorNames)] = lb => lb.Book.AuthorNames,
                    ["Author"] = lb => lb.Book.AuthorNames,
                    ["Authors"] = lb => lb.Book.AuthorNames,
                    [nameof(Book.NarratorNames)] = lb => lb.Book.NarratorNames,
                    ["Narrator"] = lb => lb.Book.NarratorNames,
                    ["Narrators"] = lb => lb.Book.NarratorNames,
                    [nameof(Book.Publisher)] = lb => lb.Book.Publisher,

                    [nameof(Book.SeriesNames)] = lb => string.Join(
                        ", ",
                        lb.Book.SeriesLink
                            .Where(s => !string.IsNullOrWhiteSpace(s.Series.Name))
                            .Select(s => s.Series.AudibleSeriesId)),
                    ["Series"] = lb => string.Join(
                        ", ",
                        lb.Book.SeriesLink
                            .Where(s => !string.IsNullOrWhiteSpace(s.Series.Name))
                            .Select(s => s.Series.AudibleSeriesId)),
                    ["SeriesId"] = lb => string.Join(", ", lb.Book.SeriesLink.Select(s => s.Series.AudibleSeriesId)),

                    [nameof(Book.CategoriesNames)] = lb => lb.Book.CategoriesIds == null ? null : string.Join(", ", lb.Book.CategoriesIds),
                    [nameof(Book.Category)] = lb => lb.Book.CategoriesIds == null ? null : string.Join(", ", lb.Book.CategoriesIds),
                    ["Categories"] = lb => lb.Book.CategoriesIds == null ? null : string.Join(", ", lb.Book.CategoriesIds),
                    ["CategoriesId"] = lb => lb.Book.CategoriesIds == null ? null : string.Join(", ", lb.Book.CategoriesIds),
                    ["CategoryId"] = lb => lb.Book.CategoriesIds == null ? null : string.Join(", ", lb.Book.CategoriesIds),

                    [TAGS.FirstCharToUpper()] = lb => lb.Book.UserDefinedItem.Tags
                }
                );

        private static ReadOnlyDictionary<string, Func<LibraryBook, string>> numberIndexRules { get; }
            = new ReadOnlyDictionary<string, Func<LibraryBook, string>>(
                new Dictionary<string, Func<LibraryBook, string>>
                {
                    // for now, all numbers are padded to 8 char.s
                    // This will allow a single method to auto-pad numbers. The method will match these as well as date: yyyymmdd
                    [nameof(Book.LengthInMinutes)] = lb => lb.Book.LengthInMinutes.ToLuceneString(),
                    ["Length"] = lb => lb.Book.LengthInMinutes.ToLuceneString(),
                    ["Minutes"] = lb => lb.Book.LengthInMinutes.ToLuceneString(),
                    ["Hours"] = lb => (lb.Book.LengthInMinutes / 60).ToLuceneString(),

                    ["ProductRating"] = lb => lb.Book.Rating.OverallRating.ToLuceneString(),
                    ["Rating"] = lb => lb.Book.Rating.OverallRating.ToLuceneString(),
                    ["UserRating"] = lb => lb.Book.UserDefinedItem.Rating.OverallRating.ToLuceneString(),
                    ["MyRating"] = lb => lb.Book.UserDefinedItem.Rating.OverallRating.ToLuceneString()
                }
                );
        private static ReadOnlyDictionary<string, Func<LibraryBook, bool>> boolIndexRules { get; }
            = new ReadOnlyDictionary<string, Func<LibraryBook, bool>>(
                new Dictionary<string, Func<LibraryBook, bool>>
                {
                    ["HasDownloads"] = lb => lb.Book.Supplements.Any(),
                    ["HasDownload"] = lb => lb.Book.Supplements.Any(),
                    ["Downloads"] = lb => lb.Book.Supplements.Any(),
                    ["Download"] = lb => lb.Book.Supplements.Any(),
                    ["HasPDFs"] = lb => lb.Book.Supplements.Any(),
                    ["HasPDF"] = lb => lb.Book.Supplements.Any(),
                    ["PDFs"] = lb => lb.Book.Supplements.Any(),
                    ["PDF"] = lb => lb.Book.Supplements.Any(),
                    ["IsRated"] = lb => lb.Book.UserDefinedItem.Rating.OverallRating > 0f,
                    ["Rated"] = lb => lb.Book.UserDefinedItem.Rating.OverallRating > 0f,
                    ["IsAuthorNarrated"] = lb => lb.Book.Authors.Intersect(lb.Book.Narrators).Any(),
                    ["AuthorNarrated"] = lb => lb.Book.Authors.Intersect(lb.Book.Narrators).Any(),
                    [nameof(Book.IsAbridged)] = lb => lb.Book.IsAbridged,
                    ["Abridged"] = lb => lb.Book.IsAbridged,
                });

        // use these common fields in the "all" default search field
        private static IEnumerable<Func<LibraryBook, string>> allFieldIndexRules { get; }
            = new List<Func<LibraryBook, string>>
            {
                idIndexRules[nameof(Book.AudibleProductId)],
                stringIndexRules[nameof(Book.Title)],
                stringIndexRules[nameof(Book.AuthorNames)],
                stringIndexRules[nameof(Book.NarratorNames)]
            };

        public static IEnumerable<string> GetSearchIdFields()
        {
            foreach (var key in idIndexRules.Keys)
                yield return key;
        }

        public static IEnumerable<string> GetSearchStringFields()
        {
            foreach (var key in stringIndexRules.Keys)
                yield return key;
        }

        public static IEnumerable<string> GetSearchBoolFields()
        {
            foreach (var key in boolIndexRules.Keys)
                yield return key;
        }

        public static IEnumerable<string> GetSearchNumberFields()
        {
            foreach (var key in numberIndexRules.Keys)
                yield return key;
        }

        public static IEnumerable<string> GetSearchFields()
        {
            foreach (var key in idIndexRules.Keys)
                yield return key;
            foreach (var key in stringIndexRules.Keys)
                yield return key;
            foreach (var key in boolIndexRules.Keys)
                yield return key;
            foreach (var key in numberIndexRules.Keys)
                yield return key;
        }

        private Directory getIndex() => FSDirectory.Open(SearchEngineDirectory);

		public void CreateNewIndex(bool overwrite = true)
        {
            // 300 products
            // 1st run after app is started: 400ms
            // subsequent runs: 200ms
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var stamps = new List<long>();
            void log() => stamps.Add(sw.ElapsedMilliseconds);


            log();

            var library = LibraryQueries.GetLibrary_Flat_NoTracking();

            log();

			// location of index/create the index
			using var index = getIndex();
            var exists = IndexReader.IndexExists(index);
            var createNewIndex = overwrite || !exists;

			// analyzer for tokenizing text. same analyzer should be used for indexing and searching
			using var analyzer = new StandardAnalyzer(Version);
			using var ixWriter = new IndexWriter(index, analyzer, createNewIndex, IndexWriter.MaxFieldLength.UNLIMITED);
			foreach (var libraryBook in library)
			{
				var doc = createBookIndexDocument(libraryBook);
				ixWriter.AddDocument(doc);
			}

            log();
        }

        private static Document createBookIndexDocument(LibraryBook libraryBook)
        {
            var doc = new Document();

            // refine with
            // http://codeclimber.net.nz/archive/2009/09/10/how-subtext-lucenenet-index-is-structured/

            // fields are key value pairs and MULTIPLE FIELDS CAN HAVE THE SAME KEY.
            // splitting authors and narrators and/or tags into multiple fields could be interesting research.
            // it could allow for more advanced searches, or maybe it could break broad searches.

            // all searching should be lowercase
            // external callers have the reasonable expectation that product id will be returned CASE SPECIFIC
            doc.AddRaw(_ID_, libraryBook.Book.AudibleProductId);

            // concat all common fields for the default 'all' field
            var allConcat =
                allFieldIndexRules
                .Select(rule => rule(libraryBook))
                .Aggregate((a, b) => $"{a} {b}");
            doc.AddAnalyzed(ALL, allConcat);

            foreach (var kvp in idIndexRules)
                doc.AddNotAnalyzed(kvp.Key, kvp.Value(libraryBook));

            foreach (var kvp in stringIndexRules)
                doc.AddAnalyzed(kvp.Key, kvp.Value(libraryBook));

            foreach (var kvp in boolIndexRules)
                doc.AddBool(kvp.Key, kvp.Value(libraryBook));

            foreach (var kvp in numberIndexRules)
                doc.AddNotAnalyzed(kvp.Key, kvp.Value(libraryBook));

            return doc;
        }

		/// <summary>Long running. Use await Task.Run(() => UpdateBook(productId))</summary>
		public void UpdateBook(string productId)
        {
            var libraryBook = LibraryQueries.GetLibraryBook_Flat_NoTracking(productId);
            var term = new Term(_ID_, productId);

            var document = createBookIndexDocument(libraryBook);
            var createNewIndex = false;

			using var index = getIndex();
			using var analyzer = new StandardAnalyzer(Version);
			using var ixWriter = new IndexWriter(index, analyzer, createNewIndex, IndexWriter.MaxFieldLength.UNLIMITED);
			ixWriter.DeleteDocuments(term);
			ixWriter.AddDocument(document);
		}

        public void UpdateTags(string productId, string tags)
        {
            var productTerm = new Term(_ID_, productId);

			using var index = getIndex();

			// get existing document
			using var searcher = new IndexSearcher(index);
			var query = new TermQuery(productTerm);
			var docs = searcher.Search(query, 1);
			var scoreDoc = docs.ScoreDocs.SingleOrDefault();
			if (scoreDoc == null)
				throw new Exception("document not found");
			var document = searcher.Doc(scoreDoc.Doc);


			// update document entry with new tags
			// fields are key value pairs and MULTIPLE FIELDS CAN HAVE THE SAME KEY. must remove old before adding new
			// REMEMBER: all fields, including 'tags' are case-specific
			document.RemoveField(TAGS);
			document.AddAnalyzed(TAGS, tags);

			// update index
			var createNewIndex = false;
			using var analyzer = new StandardAnalyzer(Version);
			using var ixWriter = new IndexWriter(index, analyzer, createNewIndex, IndexWriter.MaxFieldLength.UNLIMITED);
			ixWriter.UpdateDocument(productTerm, document, analyzer);
		}

        public SearchResultSet Search(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                searchString = "*:*";

            #region apply formatting
            searchString = parseTag(searchString);

            searchString = replaceBools(searchString);

            // in ranges " TO " must be uppercase
            searchString = searchString.Replace(" to ", " TO ");

            searchString = padNumbers(searchString);

            searchString = lowerFieldNames(searchString);
            #endregion

            var results = generalSearch(searchString);

            displayResults(results);

            return results;
        }

        private static string parseTag(string tagSearchString)
        {
            var allMatches = LuceneRegex
                .TagRegex
                .Matches(tagSearchString)
                .Cast<Match>()
                .Select(a => a.ToString())
                .ToList();
            foreach (var match in allMatches)
                tagSearchString = tagSearchString.Replace(
                    match,
                    TAGS + ":" + match.Trim('[', ']').Trim()
                    );

            return tagSearchString;
        }

        private static string replaceBools(string searchString)
        {
            // negative look-ahead for optional spaces then colon. don't want to double-up. eg:"israted:false" => "israted:false:True"
            foreach (var boolSearch in boolIndexRules.Keys)
                searchString = Regex.Replace(searchString, $@"\b({boolSearch})\b(?!\s*:)", @"$1:True", RegexOptions.IgnoreCase);

            return searchString;
        }

        private static string padNumbers(string searchString)
        {
            var matches = LuceneRegex
                .NumbersRegex
                .Matches(searchString)
                .Cast<Match>()
                .OrderByDescending(m => m.Index);

            foreach (var m in matches)
            {
                var replaceString = double.Parse(m.ToString()).ToLuceneString();
                searchString = LuceneRegex.NumbersRegex.Replace(searchString, replaceString, 1, m.Index);
            }

            return searchString;
        }

        private static string lowerFieldNames(string searchString)
        {
            // fields are case specific
            var allMatches = LuceneRegex
                .FieldRegex
                .Matches(searchString)
                .Cast<Match>()
                .Select(a => a.ToString())
                .ToList();

            foreach (var match in allMatches)
                searchString = searchString.Replace(match, match.ToLowerInvariant());

            return searchString;
        }

        public int MaxSearchResultsToReturn { get; set; } = 999;

        private SearchResultSet generalSearch(string searchString)
        {
            Console.WriteLine($"searchString: {searchString}");

            var defaultField = ALL;

			using var index = getIndex();
			using var searcher = new IndexSearcher(index);
			using var analyzer = new StandardAnalyzer(Version);
			var query = analyzer.GetQuery(defaultField, searchString);


			// lucene doesn't allow only negations. eg this returns nothing:
			//     -tags:hidden
			// work arounds: https://kb.ucla.edu/articles/pure-negation-query-in-lucene
			// HOWEVER, doing this to any other type of query can cause EVERYTHING to be a match unless "Occur" is carefully set
			// this should really check that all leaf nodes are MUST_NOT
			if (query is BooleanQuery boolQuery)
			{
				var occurs = getOccurs_recurs(boolQuery);
				if (occurs.Any() && occurs.All(o => o == Occur.MUST_NOT))
					boolQuery.Add(new MatchAllDocsQuery(), Occur.MUST);
			}

			Console.WriteLine($"  query: {query}");

			var docs = searcher
				.Search(query, MaxSearchResultsToReturn)
				.ScoreDocs
				.Select(ds => new ScoreDocExplicit(searcher.Doc(ds.Doc), ds.Score))
				.ToList();
			return new SearchResultSet(query.ToString(), docs);
		}

        private IEnumerable<Occur> getOccurs_recurs(BooleanQuery query)
        {
            var returnList = new List<Occur>();

            foreach (var clause in query)
            {
                returnList.Add(clause.Occur);

                if (clause.Query is BooleanQuery boolQuery)
                    returnList.AddRange(getOccurs_recurs(boolQuery));
            }

            return returnList;
        }

        private void displayResults(SearchResultSet docs)
        {
            Console.WriteLine($"Hit(s): {docs.Docs.Count()}");
            //for (int i = 0; i < docs.Docs.Count(); i++)
            //{
            //    var sde = docs.Docs.First();

            //    Document doc = sde.Doc;
            //    float score = sde.Score;

            //    Console.WriteLine($"{(i + 1)}) score={score}. Fields:");
            //    var allFields = doc.GetFields();
            //    foreach (var f in allFields)
            //        Console.WriteLine($"   [{f.Name}]={f.StringValue}");
            //}

            //Console.WriteLine();
        }
    }
}
