using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AaxDecrypter
{
    public class Chapter
    {
        public Chapter(double startTime, double endTime, string title)
        {
            StartTime = startTime;
            EndTime = endTime;
            Title = title;
        }
        /// <summary>
        /// Chapter start time, in seconds.
        /// </summary>
        public double StartTime { get; private set; }
        /// <summary>
        /// Chapter end time, in seconds.
        /// </summary>
        public double EndTime { get; private set; }
        public string Title { get; private set; }
    }
}
