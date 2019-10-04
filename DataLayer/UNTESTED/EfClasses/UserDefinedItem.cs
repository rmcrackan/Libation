using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dinah.Core;

namespace DataLayer
{
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

        private string _tags = "";
        public string Tags
        {
            get => _tags;
            set => _tags = sanitize(value);
        }
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
        static Regex regex = new Regex(@"[^\w\d\s_]", RegexOptions.Compiled);
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

        public IEnumerable<string> TagsEnumerated => Tags == "" ? new string[0] : Tags.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);
        #endregion

        // owned: not an optional one-to-one
        /// <summary>The user's individual book rating</summary>
        public Rating Rating { get; private set; } = new Rating(0, 0, 0);

        public void UpdateRating(float overallRating, float performanceRating, float storyRating)
            => Rating.Update(overallRating, performanceRating, storyRating);
    }
}
