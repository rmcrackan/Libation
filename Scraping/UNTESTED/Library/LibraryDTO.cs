using System;
using System.Collections.Generic;

namespace Scraping.Library
{
    public class LibraryDTO
    {
        //
        // must initialize optional collections
        //

        public string ProductId { get; set; }
        public string Title { get; set; }

        /// <summary>Whether this product is episodic. These will not have a book download link or personal library ratings</summary>
        public bool IsEpisodes { get; set; }

        // order matters. do not use a hashtable/dictionary
        public List<(string authorName, string authorId)> Authors { get; set; } = new List<(string name, string id)>();
        public string[] Narrators { get; set; } = new string[0];

        public int LengthInMinutes { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        /// <summary>Key: Id. Value: Name</summary>
        public Dictionary<string, string> Series { get; } = new Dictionary<string, string>();

        // aggregate community ratings for this product
        public float Product_OverallStars { get; set; }
        public float Product_PerformanceStars { get; set; }
        public float Product_StoryStars { get; set; }

        // my personal user ratings for this product (only products i own. ie: in library)
        public int MyUserRating_Overall { get; set; }
        public int MyUserRating_Performance { get; set; }
        public int MyUserRating_Story { get; set; }

        public List<string> SupplementUrls { get; set; } = new List<string>();

        public DateTime DateAdded { get; set; }

        public string DownloadBookLink { get; set; }

        public override string ToString() => $"[{ProductId}] {Title}";
    }
}
