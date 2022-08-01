using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dinah.Core;

namespace DataLayer
{
    /// <summary>
    /// Do not track in-process state. In-process state is determined by the presence of temp file.
    /// </summary>
    public enum LiberatedStatus
    {
        NotLiberated = 0,
        Liberated = 1,
        /// <summary>Error occurred during liberation. Don't retry</summary>
        Error = 2,

        /// <summary>Application-state only. Not a valid persistence state.</summary>
        PartialDownload = 0x1000
    }

    public class UserDefinedItem
    {
        internal int BookId { get; private set; }
        public Book Book { get; private set; }

        private UserDefinedItem() { }
        internal UserDefinedItem(Book book)
		{
			ArgumentValidator.EnsureNotNull(book, nameof(book));
            Book = book;
		}

        #region Tags
        private string _tags = "";
        public string Tags
        {
            get => _tags;
            set
            {
                var newTags = sanitize(value);
                if (_tags != newTags)
                {
                    _tags = newTags;
                    OnItemChanged(nameof(Tags));
                }
            }
		}

		public IEnumerable<string> TagsEnumerated => Tags == "" ? new string[0] : Tags.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);

		#region sanitize tags: space delimited. Inline/denormalized. Lower case. Alpha numeric and hyphen
		// only legal chars are letters numbers underscores and separating whitespace
		//
		// technically, the only char.s which aren't easily supported are  \  [  ]
		// however, whitelisting is far safer than blacklisting (eg: new lines, non-printable character)
		// it's easy to expand whitelist as needed
		// for lucene, ToLower() isn't needed because search is case-inspecific. for here, it prevents duplicates
		//
		// there are also other allowed but misleading characters. eg: the ^ operator defines a 'boost' score
		// full list of characters which must be escaped:
		//   + - && || ! ( ) { } [ ] ^ " ~ * ? : \
		static Regex regex { get; } = new Regex(@"[^\w\d\s_]", RegexOptions.Compiled);
        private static string sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            var str = input
                .Trim()
                .ToLowerInvariant()
                // assume a hyphen is supposed to be an underscore
                .Replace("-", "_");

            var unique = regex
                // turn illegal characters into a space. this will also take care of turning new lines into spaces
                .Replace(str, " ")
                // split and remove excess spaces
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                // de-dup
                .Distinct()
                // this will prevent order from being relevant
                .OrderBy(a => a);

            // currently, the string is the canonical set. if we later make the collection into the canonical set:
            //   var tags = new Hashset<string>(list); // de-dup, order doesn't matter but can seem random due to hashing algo
            //   var isEqual = tagsNew.SetEquals(tagsOld);

            return string.Join(" ", unique);
        }
        #endregion
        #endregion

        #region Rating
        // owned: not an optional one-to-one
        /// <summary>The user's individual book rating</summary>
        public Rating Rating { get; private set; } = new Rating(0, 0, 0);

        public void UpdateRating(float overallRating, float performanceRating, float storyRating)
            => Rating.Update(overallRating, performanceRating, storyRating);
        #endregion

        #region LiberatedStatuses
        /// <summary>
        /// Occurs when <see cref="Tags"/>, <see cref="BookStatus"/>, or <see cref="PdfStatus"/> values change.
        /// This signals the change of the in-memory value; it does not ensure that the new value has been persisted.
        /// </summary>
        public static event EventHandler<string> ItemChanged;

        private void OnItemChanged(string e)
        {
            // HACK
            // must not fire during initial import.
            //
            // these checks are necessary because current architecture attaches to this instead of attaching to an event *after* fully committed to db. the attached delegate/action sometimes calls commit:
            //
            // desired:
            // - importing new book
            // - update pdf status
            // - initial book commit
            //
            // actual without these checks [BAD]:
            // - importing new book
            // - update pdf status
            //   - invoke event
            //   - commit UserDefinedItem
            // - initial book commit
            if (BookId > 0 && Book is not null && Book.BookId > 0)
                ItemChanged?.Invoke(this, e);
        }

        private LiberatedStatus _bookStatus;
        private LiberatedStatus? _pdfStatus;
        public LiberatedStatus BookStatus
        {
            get => _bookStatus;
            set
            {
                // PartialDownload is a live/ephemeral status, not a persistent one. Do not store
                var displayStatus = value == LiberatedStatus.PartialDownload ? LiberatedStatus.NotLiberated : value;
                if (_bookStatus != displayStatus)
                {
                    _bookStatus = displayStatus;
                    OnItemChanged(nameof(BookStatus));
                }
            }
        }
        public LiberatedStatus? PdfStatus
        {
            get => _pdfStatus;
            set
            {
                if (_pdfStatus != value)
                {
                    _pdfStatus = value;
                    OnItemChanged(nameof(PdfStatus));
                }
            }
        }
		#endregion

        public override string ToString() => $"{Book} {Rating} {Tags}";
	}
}
