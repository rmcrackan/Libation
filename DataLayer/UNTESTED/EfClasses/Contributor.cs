using System.Collections.Generic;
using System.Linq;
using Dinah.Core;

namespace DataLayer
{
    public class Contributor
    {
        // contributors search links are just name with url-encoding. space can be + or %20
        //   author search link:   /search?searchAuthor=Robert+Bevan
        //   narrator search link: /search?searchNarrator=Robert+Bevan
        // can also search multiples. concat with comma before url encode

        // id.s
        // ----
        // https://www.audible.com/author/Neil-Gaiman/B000AQ01G2 == https://www.audible.com/author/B000AQ01G2
        //     goes to summary page
        //     at bottom "See all titles by Neil Gaiman" goes to https://www.audible.com/search?searchAuthor=Neil+Gaiman
        // some authors have no id. simply goes to https://www.audible.com/search?searchAuthor=Rufus+Fears
        // all narrators have no id: https://www.audible.com/search?searchNarrator=Neil+Gaiman

        internal int ContributorId { get; private set; }
        public string Name { get; private set; }

        private HashSet<BookContributor> _booksLink;
        public IEnumerable<BookContributor> BooksLink => _booksLink?.ToList();

        private Contributor() { }
        public Contributor(string name)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(name, nameof(name));

            _booksLink = new HashSet<BookContributor>();

            Name = name;
        }

        public string AudibleAuthorId { get; private set; }
        public void UpdateAudibleAuthorId(string authorId)
        {
            // don't overwrite with null or whitespace but not an error
            if (!string.IsNullOrWhiteSpace(authorId))
                AudibleAuthorId = authorId;
        }

        #region // AudibleAuthorId refactor: separate author-specific info. overkill for a single optional string
        ///// <summary>Most authors in Audible have a unique id</summary>
        //public AudibleAuthorProperty AudibleAuthorProperty { get; private set; }
        //public void UpdateAuthorId(string authorId, LibationContext context = null)
        //{
        //    if (authorId == null)
        //        return;
        //    if (AudibleAuthorProperty != null)
        //    {
        //        AudibleAuthorProperty.UpdateAudibleAuthorId(authorId);
        //        return;
        //    }
        //    if (context == null)
        //        throw new ArgumentNullException(nameof(context), "You must provide a context");
        //    if (context.Contributors.Find(ContributorId) == null)
        //        throw new InvalidOperationException("Could not update audible author id.");
        //    var audibleAuthorProperty = new AudibleAuthorProperty();
        //    audibleAuthorProperty.UpdateAudibleAuthorId(authorId);
        //    context.AuthorProperties.Add(audibleAuthorProperty);
        //}
        //public class AudibleAuthorProperty
        //{
        //    public int ContributorId { get; private set; }
        //    public Contributor Contributor { get; set; }

        //    public string AudibleAuthorId { get; private set; }

        //    public void UpdateAudibleAuthorId(string authorId)
        //    {
        //        if (!string.IsNullOrWhiteSpace(authorId))
        //            AudibleAuthorId = authorId;
        //    }
        //}
        //// ...and create EF table config
        #endregion
    }
}
