using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationFileManager
{
    public class BookDto
    {
        public string AudibleProductId { get; set; }
        public string Title { get; set; }
    public string Subtitle { get; set; }
    public string TitleWithSubtitle
    {
      get
      {
        string text = Title?.Trim();
        string text2 = Subtitle?.Trim();
        if (string.IsNullOrWhiteSpace(text2))
        {
          return text;
        }

        return text + ": " + text2;
      }
    }
    public string Locale { get; set; }
        public int? YearPublished { get; set; }

        public IEnumerable<string> Authors { get; set; }
        public string AuthorNames => string.Join(", ", Authors);
        public string FirstAuthor => Authors.FirstOrDefault();

        public IEnumerable<string> Narrators { get; set; }
        public string NarratorNames => string.Join(", ", Narrators);
        public string FirstNarrator => Narrators.FirstOrDefault();

        public string SeriesName { get; set; }
        public float? SeriesNumber { get; set; }
        public bool IsSeries => !string.IsNullOrEmpty(SeriesName);
        public bool IsPodcastParent { get; set; }
        public bool IsPodcast { get; set; }

        public int BitRate { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }
		public DateTime FileDate { get; set; } = DateTime.Now;
        public DateTime? DatePublished { get; set; }
        public string Language { get; set; }
	}

    public class LibraryBookDto : BookDto
	{
		public DateTime? DateAdded { get; set; }
		public string Account { get; set; }
		public string AccountNickname { get; set; }
    }
}
