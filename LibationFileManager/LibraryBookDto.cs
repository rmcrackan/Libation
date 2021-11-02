using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationFileManager
{
    public class BookDto
    {
        public string AudibleProductId { get; set; }
        public string Title { get; set; }
        public string Locale { get; set; }

        public IEnumerable<string> Authors { get; set; }
        public string AuthorNames => string.Join(", ", Authors);
        public string FirstAuthor => Authors.FirstOrDefault();

        public IEnumerable<string> Narrators { get; set; }
        public string NarratorNames => string.Join(", ", Narrators);
        public string FirstNarrator => Narrators.FirstOrDefault();

        public string SeriesName { get; set; }
        public string SeriesNumber { get; set; }
    }

    public class LibraryBookDto : BookDto
    {
        public string Account { get; set; }
    }
}
