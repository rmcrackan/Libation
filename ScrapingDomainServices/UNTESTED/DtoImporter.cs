using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using DTOs;

namespace ScrapingDomainServices
{
    public class DtoImporter
    {
        LibationContext context { get; }

        public DtoImporter(LibationContext context) => this.context = context;

        #region LibraryDTO
        /// <summary>LONG RUNNING. call with await Task.Run</summary>
        public int ReplaceLibrary(List<LibraryDTO> requestedLibraryDTOs)
        {
            upsertContributors(requestedLibraryDTOs);
            upsertSeries(requestedLibraryDTOs);
            upsertBooks(requestedLibraryDTOs);

            var newAddedCount = upsertLibraryBooks(requestedLibraryDTOs);

            //ReloadBookDetails(requestedLibraryDTOs);
            #region // explanation of: cannot ReloadBookDetails() until context.SaveChanges()
            /*
            setup:
            library page shows narrators "bob smith" "kevin jones" "and others"
            book details shows narrators "bob smith" "kevin jones" "alice liddell"

            error
            creates BookContributors with same keys, even though one is orphaned
            "The instance of entity type cannot be tracked because another instance with the same key value for {'Id'} is already being tracked"
              https://github.com/aspnet/EntityFrameworkCore/issues/12459

            solution:
            replace library
              creates library version
            save
            update book details
              adds new book details version
              removes library version
             */
            #endregion

            return newAddedCount;
        }

        private void upsertContributors(List<LibraryDTO> requestedLibraryDTOs)
        {
            var authorTuples = requestedLibraryDTOs.SelectMany(dto => dto.Authors).ToList();
            var narratorNames = requestedLibraryDTOs.SelectMany(dto => dto.Narrators).ToList();

            var allNames = authorTuples
                .Select(a => a.authorName)
                .Union(narratorNames)
                .ToList();
            loadLocal_contributors(allNames);

            upsertAuthors(authorTuples);
            upsertNarrators(narratorNames);
        }

        private void upsertSeries(List<LibraryDTO> requestedLibraryDTOs)
        {
            var requestedSeries = requestedLibraryDTOs
                .SelectMany(l => l.Series)
                .Select(kvp => (seriesId: kvp.Key, seriesName: kvp.Value))
                .ToList();

            upsertSeries(requestedSeries);
        }

        private void upsertBooks(List<LibraryDTO> requestedLibraryDTOs)
        {
            loadLocal_books(requestedLibraryDTOs.Select(dto => dto.ProductId).ToList());

            foreach (var libraryDTO in requestedLibraryDTOs)
                upsertBook(libraryDTO);
        }

        private void upsertBook(LibraryDTO libraryDTO)
        {
            var book = context.Books.Local.SingleOrDefault(p => p.AudibleProductId == libraryDTO.ProductId);
            if (book == null)
            {
                // nested logic is required so order of names is retained. else, contributors may appear in the order they were inserted into the db
                var authors = libraryDTO
                    .Authors
                    .Select(t => context.Contributors.Local.Single(c => t.authorName == c.Name))
                    .ToList();

                // if no narrators listed, author is the narrator
                if (!libraryDTO.Narrators.Any())
                    libraryDTO.Narrators = libraryDTO.Narrators = authors.Select(a => a.Name).ToArray();

                // nested logic is required so order of names is retained. else, contributors may appear in the order they were inserted into the db
                var narrators = libraryDTO
                    .Narrators
                    .Select(n => context.Contributors.Local.Single(c => n == c.Name))
                    .ToList();

                book = context.Books.Add(new Book(
                    new AudibleProductId(libraryDTO.ProductId), libraryDTO.Title, libraryDTO.Description, libraryDTO.LengthInMinutes, authors, narrators))
                    .Entity;
            }

            // set/update book-specific info which may have changed
            book.PictureId = libraryDTO.PictureId;
            book.UpdateProductRating(libraryDTO.Product_OverallStars, libraryDTO.Product_PerformanceStars, libraryDTO.Product_StoryStars);
            foreach (var url in libraryDTO.SupplementUrls)
                book.AddSupplementDownloadUrl(url);

            // important to update user-specific info. this will have changed if user has rated/reviewed the book since last library import
            book.UserDefinedItem.UpdateRating(libraryDTO.MyUserRating_Overall, libraryDTO.MyUserRating_Performance, libraryDTO.MyUserRating_Story);

            // update series even for existing books. these are occasionally updated
            var seriesIds = libraryDTO.Series.Select(kvp => kvp.Key).ToList();
            var allSeries = context.Series.Local.Where(c => seriesIds.Contains(c.AudibleSeriesId)).ToList();
            foreach (var series in allSeries)
                book.UpsertSeries(series);
        }

        private int upsertLibraryBooks(List<LibraryDTO> requestedLibraryDTOs)
        {
            var currentLibraryProductIds = context.Library.Select(l => l.Book.AudibleProductId).ToList();
            List<LibraryDTO> newLibraryDTOs = requestedLibraryDTOs.Where(dto => !currentLibraryProductIds.Contains(dto.ProductId)).ToList();

            foreach (var newLibraryDTO in newLibraryDTOs)
            {
                var libraryBook = new LibraryBook(
                    context.Books.Local.Single(b => b.AudibleProductId == newLibraryDTO.ProductId),
                    newLibraryDTO.DateAdded,
                    FileManager.FileUtility.RestoreDeclawed(newLibraryDTO.DownloadBookLink));
                context.Library.Add(libraryBook);
            }

            return newLibraryDTOs.Count;
        }

        /// <summary>LONG RUNNING. call with await Task.Run</summary>
        public void ReloadBookDetails(List<LibraryDTO> requestedLibraryDTOs)
        {
            var dtos = requestedLibraryDTOs
                .Select(dto => dto.ProductId)
                .Distinct()
                .Select(productId => FileManager.WebpageStorage.GetBookDetailJsonFileInfo(productId))
                .Where(fi => fi.Exists)
                .Select(fi => Newtonsoft.Json.JsonConvert.DeserializeObject<BookDetailDTO>(System.IO.File.ReadAllText(fi.FullName)))
                .ToList();
            if (dtos.Any())
                UpdateBookDetails(dtos);
        }
        #endregion

        #region BookDetailDTO
        /// <summary>LONG RUNNING. call with await Task.Run</summary>
        public void UpdateBookDetails(BookDetailDTO bookDetailDTO) => UpdateBookDetails(new List<BookDetailDTO> { bookDetailDTO });
        /// <summary>LONG RUNNING. call with await Task.Run</summary>
        public void UpdateBookDetails(List<BookDetailDTO> bookDetailDTOs)
        {
            upsertContributors(bookDetailDTOs);
            upsertCategories(bookDetailDTOs);
            upsertSeries(bookDetailDTOs);
            updateBooks(bookDetailDTOs);
        }

        private void upsertContributors(List<BookDetailDTO> bookDetailDTOs)
        {
            var narratorNames = bookDetailDTOs.SelectMany(dto => dto.Narrators).ToList();
            var publisherNames = bookDetailDTOs.Select(dto => dto.Publisher).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

            var allNames = narratorNames.Union(publisherNames).ToList();
            loadLocal_contributors(allNames);

            upsertNarrators(narratorNames);
            upsertPublishers(publisherNames);
        }

        private void upsertCategories(List<BookDetailDTO> bookDetailDTOs)
        {
            var categoryIds = bookDetailDTOs.SelectMany(dto => dto.Categories).Select(c => c.categoryId).ToList();
            loadLocal_categories(categoryIds);

            foreach (var dto in bookDetailDTOs)
                upsertCategories(dto);
        }

        private void upsertCategories(BookDetailDTO bookDetailDTO)
        {
            if (bookDetailDTO.Categories.Count == 0)
                return;

            if (bookDetailDTO.Categories.Count < 1 || bookDetailDTO.Categories.Count > 2)
                throw new Exception("expecting either 1 or 2 categories");

            for (var i = 0; i < bookDetailDTO.Categories.Count; i++)
            {
                var (categoryId, categoryName) = bookDetailDTO.Categories[i];

                Category parentCategory = null;
                if (i == 1)
                    parentCategory = context.Categories.Local.Single(c => c.AudibleCategoryId == bookDetailDTO.Categories[0].categoryId);

                var category
                    = context.Categories.Local.SingleOrDefault(c => c.AudibleCategoryId == categoryId)
                    ?? context.Categories.Add(new Category(new AudibleCategoryId(categoryId), categoryName)).Entity;
                category.UpdateParentCategory(parentCategory);
            }
        }

        private void upsertSeries(List<BookDetailDTO> bookDetailDTOs)
        {
            var requestedSeries = bookDetailDTOs
                .SelectMany(l => l.Series)
                .Select(s => (seriesId: s.SeriesId, seriesName: s.SeriesName))
                .ToList();

            upsertSeries(requestedSeries);
        }

        private void updateBooks(List<BookDetailDTO> bookDetailDTOs)
        {
            loadLocal_books(bookDetailDTOs.Select(dto => dto.ProductId).ToList());

            foreach (var dto in bookDetailDTOs)
                updateBook(dto);
        }

        private void updateBook(BookDetailDTO bookDetailDTO)
        {
            var book = context.Books.Local.Single(b => b.AudibleProductId == bookDetailDTO.ProductId);

            // nested logic is required so order of names is retained. else, contributors may appear in the order they were inserted into the db
            var narrators = bookDetailDTO
                .Narrators
                .Select(n => context.Contributors.Local.Single(c => n == c.Name))
                .ToList();
            // not all books have narrators. these will already be using author as narrator. don't undo this
            if (narrators.Any())
                book.ReplaceNarrators(narrators);

            var publisherName = bookDetailDTO.Publisher;
            if (!string.IsNullOrWhiteSpace(publisherName))
            {
                var publisher = context.Contributors.Local.Single(c => publisherName == c.Name);
                book.ReplacePublisher(publisher);
            }

            // these will upsert over library-scraped series, but will not leave orphans
            foreach (var seriesEntry in bookDetailDTO.Series)
            {
                var series = context.Series.Local.Single(s => seriesEntry.SeriesId == s.AudibleSeriesId);
                book.UpsertSeries(series, seriesEntry.Index);
            }

            // categories are laid out for a breadcrumb. category is 1st, subcategory is 2nd
            var category = context.Categories.Local.SingleOrDefault(c => c.AudibleCategoryId == bookDetailDTO.Categories.LastOrDefault().categoryId);
            if (category != null)
                book.UpdateCategory(category, context);

            book.UpdateBookDetails(bookDetailDTO.IsAbridged, bookDetailDTO.DatePublished);
        }
        #endregion

        #region load db existing => .Local
        private void loadLocal_contributors(List<string> contributorNames)
        {
            //// BAD: very inefficient
            // var x = context.Contributors.Local.Where(c => !contribNames.Contains(c.Name));

            // GOOD: Except() is efficient. Due to hashing, it's close to O(n)
            var localNames = context.Contributors.Local.Select(c => c.Name);
            var remainingContribNames = contributorNames
                .Distinct()
                .Except(localNames)
                .ToList();

            // load existing => local
            if (remainingContribNames.Any())
                context.Contributors.Where(c => remainingContribNames.Contains(c.Name)).ToList();
            // _________________________________^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            // i tried to extract this pattern, but this part prohibits doing so
            // wouldn't work anyway for Books.GetBooks()
        }

        private void loadLocal_series(List<string> seriesIds)
        {
            var localIds = context.Series.Local.Select(s => s.AudibleSeriesId);
            var remainingSeriesIds = seriesIds
                .Distinct()
                .Except(localIds)
                .ToList();

            if (remainingSeriesIds.Any())
                context.Series.Where(s => remainingSeriesIds.Contains(s.AudibleSeriesId)).ToList();
        }

        private void loadLocal_books(List<string> productIds)
        {
            var localProductIds = context.Books.Local.Select(b => b.AudibleProductId);
            var remainingProductIds = productIds
                .Distinct()
                .Except(localProductIds)
                .ToList();

            // GetBooks() eager loads Series, category, et al
            if (remainingProductIds.Any())
                context.Books.GetBooks(b => remainingProductIds.Contains(b.AudibleProductId)).ToList();
        }

        private void loadLocal_categories(List<string> categoryIds)
        {
            var localIds = context.Categories.Local.Select(c => c.AudibleCategoryId);
            var remainingCategoryIds = categoryIds
                .Distinct()
                .Except(localIds)
                .ToList();

            if (remainingCategoryIds.Any())
                context.Categories.Where(c => remainingCategoryIds.Contains(c.AudibleCategoryId)).ToList();
        }
        #endregion

        // only use after loading contributors => local
        private void upsertAuthors(List<(string authorName, string authorId)> requestedAuthors)
        {
            var distinctAuthors = requestedAuthors.Distinct().ToList();

            foreach (var (authorName, authorId) in distinctAuthors)
            {
                var author
                    = context.Contributors.Local.SingleOrDefault(c => c.Name == authorName)
                    ?? context.Contributors.Add(new Contributor(authorName)).Entity;
                author.UpdateAudibleAuthorId(authorId);
            }
        }

        // only use after loading contributors => local
        private void upsertNarrators(List<string> requestedNarratorNames)
        {
            var distinctNarrators = requestedNarratorNames.Distinct().ToList();

            foreach (var reqNarratorName in distinctNarrators)
                if (context.Contributors.Local.SingleOrDefault(c => c.Name == reqNarratorName) == null)
                    context.Contributors.Add(new Contributor(reqNarratorName));
        }

        // only use after loading contributors => local
        private void upsertPublishers(List<string> requestedPublisherNames)
        {
            var distinctPublishers = requestedPublisherNames.Distinct().ToList();

            foreach (var reqPublisherName in distinctPublishers)
                if (context.Contributors.Local.SingleOrDefault(c => c.Name == reqPublisherName) == null)
                    context.Contributors.Add(new Contributor(reqPublisherName));
        }

        private void upsertSeries(List<(string seriesId, string seriesName)> requestedSeries)
        {
            var distinctSeries = requestedSeries.Distinct().ToList();
            var requestedSeriesIds = distinctSeries
                .Select(r => r.seriesId)
                .Distinct()
                .ToList();

            loadLocal_series(requestedSeriesIds);

            foreach (var (seriesId, seriesName) in distinctSeries)
            {
                var series
                    = context.Series.Local.SingleOrDefault(c => c.AudibleSeriesId == seriesId)
                    ?? context.Series.Add(new Series(new AudibleSeriesId(seriesId))).Entity;
                series.UpdateName(seriesName);
            }
        }
    }
}
