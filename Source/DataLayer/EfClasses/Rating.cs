using System;

#nullable enable
namespace DataLayer
{
    /// <summary>Parameterless ctor and setters should be used by EF only. Everything else should treat it as immutable</summary>
    public record Rating : IComparable<Rating>, IComparable
    {
        public float OverallRating { get; private set; }
        public float PerformanceRating { get; private set; }
        public float StoryRating { get; private set; }

        private Rating() { }
        public Rating(float overallRating, float performanceRating, float storyRating)
        {
            OverallRating = overallRating;
            PerformanceRating = performanceRating;
            StoryRating = storyRating;
        }

        // EF magically tracks this owned object. by replacing it with a new() immutable object, stuff gets weird. update instead
        internal void Update(float overallRating, float performanceRating, float storyRating)
        {
            // don't overwrite with all 0
            if (overallRating + performanceRating + storyRating == 0)
                return;

            OverallRating = overallRating;
            PerformanceRating = performanceRating;
            StoryRating = storyRating;
        }

        public override string ToString() => $"Overall={OverallRating} Perf={PerformanceRating} Story={StoryRating}";

        public int CompareTo(Rating? other)
        {
            if (other is null) return 1;
			var compare = OverallRating.CompareTo(other.OverallRating);
            if (compare != 0) return compare;
            compare = PerformanceRating.CompareTo(other.PerformanceRating);
            if (compare != 0) return compare;
            return StoryRating.CompareTo(other.StoryRating);
        }
        public int CompareTo(object? obj) => obj is Rating second ? CompareTo(second) : 1;
    }
}
