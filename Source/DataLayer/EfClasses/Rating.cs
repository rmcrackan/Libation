using System;
using System.Collections.Generic;
using Dinah.Core;

namespace DataLayer
{
    /// <summary>Parameterless ctor and setters should be used by EF only. Everything else should treat it as immutable</summary>
    public class Rating : ValueObject_Static<Rating>
    {
        public float OverallRating { get; private set; }
        public float PerformanceRating { get; private set; }
        public float StoryRating { get; private set; }

        private Rating() { }
        internal Rating(float overallRating, float performanceRating, float storyRating)
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

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return OverallRating;
            yield return PerformanceRating;
            yield return StoryRating;
        }

		public override string ToString() => $"Overall={OverallRating} Perf={PerformanceRating} Story={StoryRating}";
	}
}
