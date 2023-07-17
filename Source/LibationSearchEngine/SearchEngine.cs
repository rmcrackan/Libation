using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using Dinah.Core;
using LibationFileManager;
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

        public const string _ID_ = "_ID_";
        public const string TAGS = "tags";
        // special field for each book which includes all major parts of the book's metadata. enables non-targetting searching
        public const string ALL = "all";

        #region index rules

        private static bool isAuthorNarrated(Book book)
        {
            var authors = book.Authors.Select(a => a.Name).ToArray();
            var narrators = book.Narrators.Select(a => a.Name).ToArray();
            return authors.Intersect(narrators).Any();
        }

		// use these common fields in the "all" default search field
		public static IndexRuleCollection FieldIndexRules { get; } = new IndexRuleCollection
        {
            { FieldType.ID, lb => lb.Book.AudibleProductId.ToLowerInvariant(), nameof(Book.AudibleProductId), "ProductId", "Id", "ASIN" },
            { FieldType.Raw, lb => lb.Book.AudibleProductId, _ID_ },
            { FieldType.String, lb => lb.Book.TitleWithSubtitle, "Title", "ProductId", "Id", "ASIN" },
            { FieldType.String, lb => lb.Book.AuthorNames(), "AuthorNames", "Author", "Authors" },
            { FieldType.String, lb => lb.Book.NarratorNames(), "NarratorNames", "Narrator", "Narrators" },
            { FieldType.String, lb => lb.Book.Publisher, nameof(Book.Publisher) },
            { FieldType.String, lb => lb.Book.SeriesNames(), "SeriesNames", "Narrator", "Series" },
            { FieldType.String, lb => string.Join(", ", lb.Book.SeriesLink.Select(s => s.Series.AudibleSeriesId)), "SeriesId" },
            { FieldType.String, lb => lb.Book.CategoriesIds() is null ? null : string.Join(", ", lb.Book.CategoriesIds()), "Category", "Categories", "CategoriesId", "CategoryId", "CategoriesNames" },
            { FieldType.String, lb => lb.Book.UserDefinedItem.Tags, TAGS.FirstCharToUpper() },
            { FieldType.String, lb => lb.Book.Locale, "Locale", "Region" },
            { FieldType.String, lb => lb.Account, "Account", "Email" },
            { FieldType.Bool, lb => lb.Book.HasPdf().ToString(), "HasDownloads", "HasDownload", "Downloads" , "Download", "HasPDFs", "HasPDF" , "PDFs", "PDF" },
            { FieldType.Bool, lb => (lb.Book.UserDefinedItem.Rating.OverallRating > 0f).ToString(), "IsRated", "Rated" },
            { FieldType.Bool, lb => isAuthorNarrated(lb.Book).ToString(), "IsAuthorNarrated", "AuthorNarrated" },
            { FieldType.Bool, lb => lb.Book.IsAbridged.ToString(), nameof(Book.IsAbridged), "Abridged" },
            { FieldType.Bool, lb => (lb.Book.UserDefinedItem.BookStatus == LiberatedStatus.Liberated).ToString(), "IsLiberated", "Liberated" },
            { FieldType.Bool, lb => (lb.Book.UserDefinedItem.BookStatus == LiberatedStatus.Error).ToString(), "LiberatedError" },
            { FieldType.Bool, lb => lb.Book.IsEpisodeChild().ToString(), "Podcast", "Podcasts", "IsPodcast", "Episode", "Episodes", "IsEpisode" },
            { FieldType.Bool, lb => lb.AbsentFromLastScan.ToString(), "AbsentFromLastScan", "Absent" },
            // all numbers are padded to 8 char.s
            // This will allow a single method to auto-pad numbers. The method will match these as well as date: yyyymmdd
            { FieldType.Number, lb => lb.Book.LengthInMinutes.ToLuceneString(), nameof(Book.LengthInMinutes), "Length", "Minutes" },
            { FieldType.Number, lb => (lb.Book.LengthInMinutes / 60).ToLuceneString(), "Hours" },
            { FieldType.Number, lb => lb.Book.Rating.OverallRating.ToLuceneString(), "ProductRating", "Rating" },
            { FieldType.Number, lb => lb.Book.UserDefinedItem.Rating.OverallRating.ToLuceneString(), "UserRating", "MyRating" },
            { FieldType.Number, lb => lb.Book.DatePublished?.ToLuceneString() ?? "", nameof(Book.DatePublished) },
            { FieldType.Number, lb => lb.Book.UserDefinedItem.LastDownloaded.ToLuceneString(), nameof(UserDefinedItem.LastDownloaded), "LastDownload" },
            { FieldType.Number, lb => lb.DateAdded.ToLuceneString(), nameof(LibraryBook.DateAdded) }
		};
        #endregion

        #region create and update index
        /// <summary>create new. ie: full re-index</summary>
        public void CreateNewIndex(IEnumerable<LibraryBook> library, bool overwrite = true)
        {
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
        }

        /// <summary>Long running. Use await Task.Run(() => UpdateBook(productId))</summary>
        public void UpdateBook(LibationContext context, string productId)
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

            // concat all common fields for the default 'all' field
            var allConcat =
                FieldIndexRules
                .Select(rule => rule.GetValue(libraryBook))
                .Aggregate((a, b) => $"{a} {b}");
            doc.AddAnalyzed(ALL, allConcat);

            foreach (var rule in FieldIndexRules)
                doc.AddIndexRule(rule, libraryBook);

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
					d.RemoveField(fieldName.ToLower());
                    d.AddAnalyzed(fieldName, newValue);
				});

		// update single document entry
        public void UpdateLiberatedStatus(LibraryBook book)
            => updateDocument(
                book.Book.AudibleProductId,
                d =>
                {
                    var lib = FieldIndexRules.GetRuleByFieldName("IsLiberated");
                    var libError = FieldIndexRules.GetRuleByFieldName("LiberatedError");
                    var lastDl = FieldIndexRules.GetRuleByFieldName(nameof(UserDefinedItem.LastDownloaded));

                    d.RemoveRule(lib);
                    d.RemoveRule(libError);
                    d.RemoveRule(lastDl);

                    d.AddIndexRule(lib, book);
                    d.AddIndexRule(libError, book);
                    d.AddIndexRule(lastDl, book);
				});

        public void UpdateUserRatings(LibraryBook book)
            =>updateDocument(
                book.Book.AudibleProductId,
                d =>
				{
					var rating = FieldIndexRules.GetRuleByFieldName("UserRating");

					d.RemoveRule(rating);
					d.AddIndexRule(rating, book);
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
            if (scoreDoc is null)
                return;
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

        // the workaround which allows displaying all books when query is empty
        public const string ALL_QUERY = "*:*";

        #region search
        public SearchResultSet Search(string searchString)
		{
			using var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

			Serilog.Log.Logger.Debug("original search string: {@DebugInfo}", new { searchString });
            searchString = QuerySanitizer.Sanitize(searchString, analyzer);
            Serilog.Log.Logger.Debug("formatted search string: {@DebugInfo}", new { searchString });

            var results = generalSearch(searchString, analyzer);
            Serilog.Log.Logger.Debug("Hit(s): {@DebugInfo}", new { count = results.Docs.Count() });
            displayResults(results);

            return results;
        }

        private SearchResultSet generalSearch(string searchString, StandardAnalyzer analyzer)
        {
            var defaultField = ALL;

            using var index = getIndex();
            using var searcher = new IndexSearcher(index);
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

        // not customizable. don't move to config
        private static string SearchEngineDirectory { get; }
            = new System.IO.DirectoryInfo(Configuration.Instance.LibationFiles).CreateSubdirectory("SearchEngine").FullName;
    }
}
