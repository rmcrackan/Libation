using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    // enum will be easier than bool to extend later. 
    public enum ContentType 
    { 
        Unknown = 0,
        Product = 1,
        Episode = 2,
        Parent = 4,
    }


    public class Book
    {
        // implementation detail. set by db only. only used by data layer
        internal int BookId { get; private set; }

        // immutable
        public string AudibleProductId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        private string _titleWithSubtitle;
        public string TitleWithSubtitle => _titleWithSubtitle ??= string.IsNullOrEmpty(Subtitle) ? Title : $"{Title}: {Subtitle}";
        public string Description { get; private set; }
        public int LengthInMinutes { get; private set; }
        public ContentType ContentType { get; private set; }
        public string Locale { get; private set; }

		//This field is now unused, however, there is little sense in adding a
		//database migration to remove an unused field. Leave it for compatibility.
#pragma warning disable CS0649 // Field 'Book._audioFormat' is never assigned to, and will always have its default value 0
		internal long _audioFormat;
#pragma warning restore CS0649

        // mutable
        public string PictureId { get; set; }
        public string PictureLarge { get; set; }

        // book details
        public bool IsAbridged { get; private set; }
        public DateTime? DatePublished { get; private set; }
        public string Language { get; private set; }

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
            string subtitle,
            string description,
            int lengthInMinutes,
            ContentType contentType,
            IEnumerable<Contributor> authors,
            IEnumerable<Contributor> narrators,
            string localeName)
        {
            // validate
            ArgumentValidator.EnsureNotNull(audibleProductId, nameof(audibleProductId));
            var productId = audibleProductId.Id;
            ArgumentValidator.EnsureNotNullOrWhiteSpace(productId, nameof(productId));

            // assign as soon as possible. stuff below relies on this
            AudibleProductId = productId;
            Locale = localeName;

            ArgumentValidator.EnsureNotNullOrWhiteSpace(title, nameof(title));

            // non-ef-ctor init.s
            UserDefinedItem = new UserDefinedItem(this);
            ContributorsLink = new HashSet<BookContributor>();
            CategoriesLink = new HashSet<BookCategory>();
            _seriesLink = new HashSet<SeriesBook>();
            _supplements = new HashSet<Supplement>();

            // simple assigns
            UpdateTitle(title, subtitle);
            Description = description?.Trim() ?? "";
            LengthInMinutes = lengthInMinutes;
            ContentType = contentType;

            // assigns with biz logic
            ReplaceAuthors(authors);
            ReplaceNarrators(narrators);
        }

        public void UpdateTitle(string title, string subtitle)
        {
            Title = title?.Trim() ?? "";
            Subtitle = subtitle?.Trim() ?? "";
            _titleWithSubtitle = null;
        }

        public void UpdateLengthInMinutes(int lengthInMinutes)
            => LengthInMinutes = lengthInMinutes;

		#region contributors, authors, narrators
		internal HashSet<BookContributor> ContributorsLink { get; private set; }

        public IEnumerable<Contributor> Authors => ContributorsLink.ByRole(Role.Author).Select(bc => bc.Contributor).ToList();
        public IEnumerable<Contributor> Narrators => ContributorsLink.ByRole(Role.Narrator).Select(bc => bc.Contributor).ToList();
        public string Publisher => ContributorsLink.ByRole(Role.Publisher).SingleOrDefault()?.Contributor.Name;

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
            if (ContributorsLink is null)
                getEntry(context).Collection(s => s.ContributorsLink).Load();

            var isIdentical
                = ContributorsLink
                .ByRole(role)
                .Select(c => c.Contributor)
                .SequenceEqual(newContributors);

            if (isIdentical)
                return;

            ContributorsLink.RemoveWhere(bc => bc.Role == role);
            addNewContributors(newContributors, role);
        }

        private void addNewContributors(IEnumerable<Contributor> newContributors, Role role)
        {
            byte order = 0;
            var newContributionsEnum = newContributors.Select(c => new BookContributor(this, c, role, order++));
            var newContributions = new HashSet<BookContributor>(newContributionsEnum);
            ContributorsLink.UnionWith(newContributions);
        }

        #endregion

        private Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Book> getEntry(DbContext context)
        {
            ArgumentValidator.EnsureNotNull(context, nameof(context));

            var entry = context.Entry(this);

            if (!entry.IsKeySet)
                throw new InvalidOperationException("Could not load a valid Book from database");

            return entry;
        }

        #region categories
        internal HashSet<BookCategory> CategoriesLink { get; private set; }

        private ReadOnlyCollection<BookCategory> _categoriesReadOnly;
        public ReadOnlyCollection<BookCategory> Categories
        {
            get
            {
                if (_categoriesReadOnly?.SequenceEqual(CategoriesLink) is not true)
                    _categoriesReadOnly = CategoriesLink.ToList().AsReadOnly();
                return _categoriesReadOnly;
            }
        }
		public void SetCategoryLadders(IEnumerable<CategoryLadder> ladders)
		{
			ArgumentValidator.EnsureNotNull(ladders, nameof(ladders));

			//Replace all existing category ladders.
            //Some books make have duplicate ladders
			CategoriesLink.Clear();
            CategoriesLink.UnionWith(ladders.Distinct().Select(l => new BookCategory(this, l)));
		}
		#endregion

		#region series
		private HashSet<SeriesBook> _seriesLink;
        public IEnumerable<SeriesBook> SeriesLink => _seriesLink?.ToList();

        public void UpsertSeries(Series series, string order, DbContext context = null)
        {
            ArgumentValidator.EnsureNotNull(series, nameof(series));

            // our add() is conditional upon what's already included in the collection.
            // therefore if not loaded, a trip is required. might as well just load it
            if (_seriesLink is null)
				getEntry(context).Collection(s => s.SeriesLink).Load();

			var singleSeriesBook = _seriesLink.SingleOrDefault(sb => sb.Series == series);
            if (singleSeriesBook is null)
                _seriesLink.Add(new SeriesBook(series, this, order));
            else
                singleSeriesBook.UpdateOrder(order);
        }
        #endregion

        #region supplements
        private HashSet<Supplement> _supplements;
        public IEnumerable<Supplement> Supplements => _supplements?.ToList();

        public void AddSupplementDownloadUrl(string url)
        {
            // supplements are owned by Book, so no need to Load():
            //  Are automatically loaded, and can only be tracked by a DbContext alongside their owner.

            ArgumentValidator.EnsureNotNullOrWhiteSpace(url, nameof(url));

            if (_supplements.Any(s => url.EqualsInsensitive(url)))
                return;

            _supplements.Add(new Supplement(this, url));
            UserDefinedItem.PdfStatus ??= LiberatedStatus.NotLiberated;
        }
        #endregion

        public void UpdateProductRating(float overallRating, float performanceRating, float storyRating)
            => Rating.Update(overallRating, performanceRating, storyRating);

        public void UpdateBookDetails(bool isAbridged, DateTime? datePublished, string language)
        {
            // don't overwrite with default values
            IsAbridged |= isAbridged;
            DatePublished = datePublished ?? DatePublished;
            Language = language?.FirstCharToUpper() ?? Language;
        }

        public override string ToString() => $"[{AudibleProductId}] {TitleWithSubtitle}";
	}
}
