using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
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

        private LibationContext context { get; }

        // not customizable. don't move to config
        private static string SearchEngineDirectory { get; }
            = new System.IO.DirectoryInfo(Configuration.Instance.LibationFiles).CreateSubdirectory("SearchEngine").FullName;

        public const string _ID_ = "_ID_";
        public const string TAGS = "tags";
        // special field for each book which includes all major parts of the book's metadata. enables non-targetting searching
        public const string ALL = "all";

        // the workaround which allows displaying all books when query is empty
        public const string ALL_QUERY = "*:*";

        public SearchEngine(LibationContext context) => this.context = context;

		#region index rules
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

                    [TAGS.FirstCharToUpper()] = lb => lb.Book.UserDefinedItem.Tags,

                    ["Locale"] = lb => lb.Book.Locale,
                    ["Region"] = lb => lb.Book.Locale,
                    ["Account"] = lb => lb.Account,
                    ["Email"] = lb => lb.Account
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

                    ["IsAuthorNarrated"] = isAuthorNarrated,
                    ["AuthorNarrated"] = isAuthorNarrated,

                    [nameof(Book.IsAbridged)] = lb => lb.Book.IsAbridged,
                    ["Abridged"] = lb => lb.Book.IsAbridged,

                    // this will only be evaluated at time of re-index. ie: state of files moved later will be out of sync until next re-index
                    ["IsLiberated"] = lb => isLiberated(lb.Book),
                    ["Liberated"] = lb => isLiberated(lb.Book),
                    ["LiberatedError"] = lb => liberatedError(lb.Book),
                }
                );

        private static bool isAuthorNarrated(LibraryBook lb)
        {
            var authors = lb.Book.Authors.Select(a => a.Name).ToArray();
            var narrators = lb.Book.Narrators.Select(a => a.Name).ToArray();
            return authors.Intersect(narrators).Any();
        }

        private static bool isLiberated(Book book)
            => book.UserDefinedItem.BookStatus == LiberatedStatus.Liberated
            || AudibleFileStorage.Audio.Exists(book.AudibleProductId);
        private static bool liberatedError(Book book) => book.UserDefinedItem.BookStatus == LiberatedStatus.Error;

        // use these common fields in the "all" default search field
        private static IEnumerable<Func<LibraryBook, string>> allFieldIndexRules { get; }
            = new List<Func<LibraryBook, string>>
            {
                idIndexRules[nameof(Book.AudibleProductId)],
                stringIndexRules[nameof(Book.Title)],
                stringIndexRules[nameof(Book.AuthorNames)],
                stringIndexRules[nameof(Book.NarratorNames)]
            };
		#endregion

		#region get search fields. used for display in help
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
		#endregion

		#region create and update index
        /// <summary>
        /// create new. ie: full re-index
        /// </summary>
        /// <param name="overwrite"></param>
        public void CreateNewIndex(bool overwrite = true)
        {
            //  300 titles:  200- 400 ms
            // 1021 titles: 1777-2250 ms
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var stamps = new List<long>();
            void log() => stamps.Add(sw.ElapsedMilliseconds);


            log();

            var library = context.GetLibrary_Flat_NoTracking();

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

		/// <summary>Long running. Use await Task.Run(() => UpdateBook(productId))</summary>
		public void UpdateBook(string productId)
        {
            var libraryBook = context.GetLibraryBook_Flat_NoTracking(productId);
            var term = new Term(_ID_, productId);

            var document = createBookIndexDocument(libraryBook);
            var createNewIndex = false;

			using var index = getIndex();
			using var analyzer = new StandardAnalyzer(Version);
			using var ixWriter = new IndexWriter(index, analyzer, createNewIndex, IndexWriter.MaxFieldLength.UNLIMITED);
			ixWriter.DeleteDocuments(term);
			ixWriter.AddDocument(document);
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

        // update single document entry
        // all fields, including 'tags' are case-specific
        public void UpdateTags(string productId, string tags) => updateAnalyzedField(productId, TAGS, tags);

        // all fields are case-specific
        private static void updateAnalyzedField(string productId, string fieldName, string newValue)
            => updateDocument(
                productId,
                d =>
                {
                    // fields are key value pairs. MULTIPLE FIELDS CAN POTENTIALLY HAVE THE SAME KEY.
                    // ie: must remove old before adding new else will create unwanted duplicates.
                    d.RemoveField(fieldName);
                    d.AddAnalyzed(fieldName, newValue);
                });

        // update single document entry
        public void UpdateLiberatedStatus(Book book)
            => updateDocument(
                book.AudibleProductId,
                d =>
                {
                    // fields are key value pairs. MULTIPLE FIELDS CAN POTENTIALLY HAVE THE SAME KEY.
                    // ie: must remove old before adding new else will create unwanted duplicates.
                    var v1 = isLiberated(book);
                    d.RemoveField("IsLiberated");
                    d.AddBool("IsLiberated", v1);
                    d.RemoveField("Liberated");
                    d.AddBool("Liberated", v1);

                    var v2 = liberatedError(book);
                    d.RemoveField("LiberatedError");
                    d.AddBool("LiberatedError", v2);
                });

        private static void updateDocument(string productId, Action<Document> action)
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


            // perform update
            action(document);


            // update index
            var createNewIndex = false;
            using var analyzer = new StandardAnalyzer(Version);
            using var ixWriter = new IndexWriter(index, analyzer, createNewIndex, IndexWriter.MaxFieldLength.UNLIMITED);
            ixWriter.UpdateDocument(productTerm, document, analyzer);
        }
		#endregion

		#region search
        public SearchResultSet Search(string searchString)
        {
            Serilog.Log.Logger.Debug("original search string: {@DebugInfo}", new { searchString });
            searchString = FormatSearchQuery(searchString);
            Serilog.Log.Logger.Debug("formatted search string: {@DebugInfo}", new { searchString });

            var results = generalSearch(searchString);
            Serilog.Log.Logger.Debug("Hit(s): {@DebugInfo}", new { count = results.Docs.Count() });
            displayResults(results);

            return results;
        }

        public static string FormatSearchQuery(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return ALL_QUERY;

            searchString = replaceBools(searchString);

            searchString = parseTag(searchString);

            // in ranges " TO " must be uppercase
            searchString = searchString.Replace(" to ", " TO ");

            searchString = padNumbers(searchString);

            searchString = lowerFieldNames(searchString);

            return searchString;
        }

		#region format query string
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
            foreach (var boolSearch in boolIndexRules.Keys)
                searchString =
                    LuceneRegex.GetBoolRegex(boolSearch)
                    .Replace(searchString, @"$1:True");

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
        #endregion

        private SearchResultSet generalSearch(string searchString)
        {
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

			var docs = searcher
				.Search(query, searcher.MaxDoc + 1)
				.ScoreDocs
				.Select(ds => new ScoreDocExplicit(searcher.Doc(ds.Doc), ds.Score))
				.ToList();
            var queryString = query.ToString();
            Serilog.Log.Logger.Debug("query: {@DebugInfo}", new { queryString });
			return new SearchResultSet(queryString, docs);
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
            //for (int i = 0; i < docs.Docs.Count(); i++)
            //{
            //    var sde = docs.Docs.First();

            //    Document doc = sde.Doc;
            //    float score = sde.Score;

            //    Serilog.Log.Logger.Debug($"{(i + 1)}) score={score}. Fields:");
            //    var allFields = doc.GetFields();
            //    foreach (var f in allFields)
            //        Serilog.Log.Logger.Debug($"   [{f.Name}]={f.StringValue}");
            //}
        }
		#endregion

		private static Directory getIndex() => FSDirectory.Open(SearchEngineDirectory);
    }
}
