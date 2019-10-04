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

        public float FirstScore
            => OverallRating > 0 ? OverallRating
            : PerformanceRating > 0 ? PerformanceRating
            : StoryRating;

        /// <summary>character: ★</summary>
        const char STAR = '\u2605';
        /// <summary>character: ½</summary>
        const char HALF = '\u00BD';
        string getStars(float score)
        {
            var fullStars = (int)Math.Floor(score);

            var starString = "".PadLeft(fullStars, STAR);

            if (score - fullStars == 0.5f)
                starString += HALF;

            return starString;
        }

        public string ToStarString()
        {
            var items = new List<string>();

            if (OverallRating > 0)
                items.Add($"Overall:  {getStars(OverallRating)}");
            if (PerformanceRating > 0)
                items.Add($"Perform: {getStars(PerformanceRating)}");
            if (StoryRating > 0)
                items.Add($"Story:     {getStars(StoryRating)}");

            return string.Join("\r\n", items);
        }
    }
}
