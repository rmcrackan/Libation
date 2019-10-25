using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core.Collections.Generic;

namespace DTOs
{
	public partial class LibraryApiV10
	{
		public IEnumerable<Person> AuthorsDistinct => Items.GetAuthorsDistinct();
		public IEnumerable<Person> NarratorsDistinct => Items.GetNarratorsDistinct();

		public override string ToString() => $"{Items.Length} {nameof(Items)}, {ResponseGroups.Length} {nameof(ResponseGroups)}";
	}
	public partial class Item
	{
		public string ProductId => Asin;
		public int LengthInMinutes => RuntimeLengthMin ?? 0;
		public string Description => PublisherSummary;
		public bool Episodes
			=> Relationships
				?.Where(r => r.RelationshipToProduct == RelationshipToProduct.Child && r.RelationshipType == RelationshipType.Episode)
				.Any()
			?? false;
		public string PictureId => ProductImages?.PictureId;
		public string SupplementUrls => PdfUrl.AbsoluteUri; // item.PdfUrl == item.PdfLink
		public DateTime DateAdded => PurchaseDate.UtcDateTime;

		public float Product_OverallStars => Convert.ToSingle(Rating?.OverallDistribution.DisplayStars ?? 0);
		public float Product_PerformanceStars => Convert.ToSingle(Rating?.PerformanceDistribution.DisplayStars ?? 0);
		public float Product_StoryStars => Convert.ToSingle(Rating?.StoryDistribution.DisplayStars ?? 0);

		public int MyUserRating_Overall => Convert.ToInt32(ProvidedReview?.Ratings.OverallRating ?? 0L);
		public int MyUserRating_Performance => Convert.ToInt32(ProvidedReview?.Ratings.PerformanceRating ?? 0L);
		public int MyUserRating_Story => Convert.ToInt32(ProvidedReview?.Ratings.StoryRating ?? 0L);

		public bool IsAbridged
			=> FormatType.HasValue
			? FormatType == DTOs.FormatType.Abridged
			: false;
		public DateTime? DatePublished => IssueDate?.UtcDateTime; // item.IssueDate == item.ReleaseDate
		public string Publisher => PublisherName;

		// future: need support for multiple categories
		public Ladder[] Categories => CategoryLadders?.FirstOrDefault()?.Ladder ?? new Ladder[0];

		// LibraryDTO.DownloadBookLink will be handled differently. see api.DownloadAaxWorkaroundAsync(asin)

		public IEnumerable<Person> AuthorsDistinct => Authors.DistinctBy(a => new { a.Name, a.Asin });
		public IEnumerable<Person> NarratorsDistinct => Narrators.DistinctBy(a => new { a.Name, a.Asin });

		public override string ToString() => $"[{ProductId}] {Title}";
	}
	public partial class Person
	{
		public override string ToString() => $"{Name}";
	}
	public partial class AvailableCodec
	{
		public override string ToString() => $"{Name} {Format} {EnhancedCodec}";
	}
	public partial class CategoryLadder
	{
		public override string ToString() => Ladder.Select(l => l.CategoryName).Aggregate((a, b) => $"{a} | {b}");
	}
	public partial class Ladder
	{
		public string CategoryId => Id;
		public string CategoryName => Name;

		public override string ToString() => $"[{CategoryId}] {CategoryName}";
	}
	public partial class ContentRating
	{
		public override string ToString() => $"{Steaminess}";
	}
	public partial class Review
	{
		public override string ToString() => $"{this.Title}";
	}
	public partial class GuidedResponse
	{
		//public override string ToString() => 
	}
	public partial class Ratings
	{
		public override string ToString() => $"{OverallRating:0.0}|{PerformanceRating:0.0}|{StoryRating:0.0}";
	}
	public partial class ReviewContentScores
	{
		public override string ToString() => $"Helpful={NumHelpfulVotes}, Unhelpful={NumUnhelpfulVotes}";
	}
	public partial class Plan
	{
		public override string ToString() => $"{PlanName}";
	}
	public partial class Price
	{
		public override string ToString() => $"List={ListPrice}, Lowest={LowestPrice}";
	}
	public partial class ListPriceClass
	{
		public override string ToString() => $"{Base}";
	}
	public partial class ProductImages
	{
		public string PictureId
			=> The500
				.AbsoluteUri // https://m.media-amazon.com/images/I/51T1NWIkR4L._SL500_.jpg?foo=bar
				?.Split('/').Last() // 51T1NWIkR4L._SL500_.jpg?foo=bar
				?.Split('.').First() // 51T1NWIkR4L
			;

		public override string ToString() => $"{The500}";
	}
	public partial class Rating
	{
		public override string ToString() => $"{OverallDistribution}|{PerformanceDistribution}|{StoryDistribution}";
	}
	public partial class Distribution
	{
		public override string ToString() => $"{DisplayStars:0.0}";
	}
	public partial class Relationship
	{
		public override string ToString() => $"{RelationshipToProduct} {RelationshipType}";
	}
	public partial class Series
	{
		public string SeriesName => Title;
		public string SeriesId => Asin;
		public float Index
			=> string.IsNullOrEmpty(Sequence)
			? 0
			// eg: a book containing volumes 5,6,7,8 has sequence "5-8"
			: float.Parse(Sequence.Split('-').First());

		public override string ToString() => $"[{SeriesId}] {SeriesName}";
	}
}
