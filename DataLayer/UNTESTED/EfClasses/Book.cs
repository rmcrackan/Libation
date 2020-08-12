using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public class AudibleProductId
    {
        public string Id { get; }
        public AudibleProductId(string id)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(id, nameof(id));
            Id = id;
        }
    }
    public class Book
    {
        // implementation detail. set by db only. only used by data layer
        internal int BookId { get; private set; }

        // immutable
        public string AudibleProductId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public int LengthInMinutes { get; private set; }

        // immutable-ish. should be immutable. mutability is necessary for v3 => v4 upgrades
        public string Locale { get; private set; }

        // mutable
        public string PictureId { get; set; }

        // book details
        public bool IsAbridged { get; private set; }
        public DateTime? DatePublished { get; private set; }

        // non-null. use "empty pattern"
        internal int CategoryId { get; private set; }
        public Category Category { get; private set; }
        public string[] CategoriesNames
            => Category == null ? new string[0]
            : Category.ParentCategory == null ? new[] { Category.Name }
            : new[] { Category.ParentCategory.Name, Category.Name };
        public string[] CategoriesIds
            => Category == null ? null
            : Category.ParentCategory == null ? new[] { Category.AudibleCategoryId }
            : new[] { Category.ParentCategory.AudibleCategoryId, Category.AudibleCategoryId };

        // is owned, not optional 1:1
        public UserDefinedItem UserDefinedItem { get; private set; }

        // is owned, not optional 1:1
        /// <summary>The product's aggregate community rating</summary>
        public Rating Rating { get; private set; } = new Rating(0, 0, 0);

        // ef-ctor
        private Book() { }
        // non-ef ctor
        /// <param name="audibleProductId">special id class b/c it's too easy to get string order mixed up</param>
        public Book(
            AudibleProductId audibleProductId,
            string title,
            string description,
            int lengthInMinutes,
			IEnumerable<Contributor> authors,
			IEnumerable<Contributor> narrators,
			Category category)
        {
            // validate
            ArgumentValidator.EnsureNotNull(audibleProductId, nameof(audibleProductId));
            var productId = audibleProductId.Id;
            ArgumentValidator.EnsureNotNullOrWhiteSpace(productId, nameof(productId));

			// assign as soon as possible. stuff below relies on this
            AudibleProductId = productId;

            ArgumentValidator.EnsureNotNullOrWhiteSpace(title, nameof(title));

            // non-ef-ctor init.s
            UserDefinedItem = new UserDefinedItem(this);
            _contributorsLink = new HashSet<BookContributor>();
            _seriesLink = new HashSet<SeriesBook>();
            _supplements = new HashSet<Supplement>();

			Category = category;

            // simple assigns
            Title = title;
            Description = description;
            LengthInMinutes = lengthInMinutes;

            // assigns with biz logic
            ReplaceAuthors(authors);
            ReplaceNarrators(narrators);
		}

        #region contributors, authors, narrators
        // use uninitialised backing fields - this means we can detect if the collection was loaded
        private HashSet<BookContributor> _contributorsLink;
        // i'd like this to be internal but migration throws this exception when i try:
        //   Value cannot be null.
        //   Parameter name: property
        public IEnumerable<BookContributor> ContributorsLink
            => _contributorsLink?
                .OrderBy(bc => bc.Order)
                .ToList();

        public IEnumerable<Contributor> Authors => getContributions(Role.Author).Select(bc => bc.Contributor).ToList();
        public string AuthorNames => string.Join(", ", Authors.Select(a => a.Name));

        public IEnumerable<Contributor> Narrators => getContributions(Role.Narrator).Select(bc => bc.Contributor).ToList();
        public string NarratorNames => string.Join(", ", Narrators.Select(n => n.Name));

        public string Publisher => getContributions(Role.Publisher).SingleOrDefault()?.Contributor.Name;

        public void ReplaceAuthors(IEnumerable<Contributor> authors, DbContext context = null)
            => replaceContributors(authors, Role.Author, context);
        public void ReplaceNarrators(IEnumerable<Contributor> narrators, DbContext context = null)
            => replaceContributors(narrators, Role.Narrator, context);
        public void ReplacePublisher(Contributor publisher, DbContext context = null)
            => replaceContributors(new List<Contributor> { publisher }, Role.Publisher, context);
        private void replaceContributors(IEnumerable<Contributor> newContributors, Role role, DbContext context = null)
        {
            ArgumentValidator.EnsureEnumerableNotNullOrEmpty(newContributors, nameof(newContributors));

            // the edge cases of doing local-loaded vs remote-only got weird. just load it
            if (_contributorsLink is null)
				getEntry(context).Collection(s => s.ContributorsLink).Load();

			var roleContributions = getContributions(role);
            var isIdentical = roleContributions.Select(c => c.Contributor).SequenceEqual(newContributors);
            if (isIdentical)
                return;

            _contributorsLink.RemoveWhere(bc => bc.Role == role);
            addNewContributors(newContributors, role);
        }

		private void addNewContributors(IEnumerable<Contributor> newContributors, Role role)
        {
            byte order = 0;
            var newContributionsEnum = newContributors.Select(c => new BookContributor(this, c, role, order++));
            var newContributions = new HashSet<BookContributor>(newContributionsEnum);
            _contributorsLink.UnionWith(newContributions);
        }

        private List<BookContributor> getContributions(Role role)
            => ContributorsLink
                .Where(a => a.Role == role)
                .OrderBy(a => a.Order)
                .ToList();
        #endregion

		private Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Book> getEntry(DbContext context)
		{
			ArgumentValidator.EnsureNotNull(context, nameof(context));

			var entry = context.Entry(this);

			if (!entry.IsKeySet)
				throw new InvalidOperationException("Could not load a valid Book from database");

			return entry;
		}

        #region series
        private HashSet<SeriesBook> _seriesLink;
        public IEnumerable<SeriesBook> SeriesLink => _seriesLink?.ToList();
        public string SeriesNames
        {
            get
            {
                // first: alphabetical by name
                var withNames = _seriesLink
                    .Where(s => !string.IsNullOrWhiteSpace(s.Series.Name))
                    .Select(s => s.Series.Name)
                    .OrderBy(a => a)
                    .ToList();
                // then un-named are alpha by series id
                var nullNames = _seriesLink
                    .Where(s => string.IsNullOrWhiteSpace(s.Series.Name))
                    .Select(s => s.Series.AudibleSeriesId)
                    .OrderBy(a => a)
                    .ToList();

                var all = withNames.Union(nullNames).ToList();
                return string.Join(", ", all);
            }
        }

        public void UpsertSeries(Series series, float? index = null, DbContext context = null)
        {
            ArgumentValidator.EnsureNotNull(series, nameof(series));

            // our add() is conditional upon what's already included in the collection.
            // therefore if not loaded, a trip is required. might as well just load it
            if (_seriesLink is null)
				getEntry(context).Collection(s => s.SeriesLink).Load();

			var singleSeriesBook = _seriesLink.SingleOrDefault(sb => sb.Series == series);
            if (singleSeriesBook == null)
                _seriesLink.Add(new SeriesBook(series, this, index));
            else
                singleSeriesBook.UpdateIndex(index);
        }
        #endregion

        #region supplements
        private HashSet<Supplement> _supplements;
        public IEnumerable<Supplement> Supplements => _supplements?.ToList();
        public bool HasPdf => Supplements.Any();

        public void AddSupplementDownloadUrl(string url)
        {
            // supplements are owned by Book, so no need to Load():
            //  Are automatically loaded, and can only be tracked by a DbContext alongside their owner.

            ArgumentValidator.EnsureNotNullOrWhiteSpace(url, nameof(url));

            if (!_supplements.Any(s => url.EqualsInsensitive(url)))
                _supplements.Add(new Supplement(this, url));
        }
        #endregion

        public void UpdateProductRating(float overallRating, float performanceRating, float storyRating)
            => Rating.Update(overallRating, performanceRating, storyRating);

        public void UpdateBookDetails(bool isAbridged, DateTime? datePublished)
        {
            // don't overwrite with default values
            IsAbridged |= isAbridged;
            DatePublished = datePublished ?? DatePublished;
        }

        public void UpdateCategory(Category category, DbContext context = null)
		{
			// since category is never null, nullity means it hasn't been loaded
			if (Category is null)
				getEntry(context).Reference(s => s.Category).Load();

			Category = category;
        }

		public override string ToString() => $"[{AudibleProductId}] {Title}";
	}
}
