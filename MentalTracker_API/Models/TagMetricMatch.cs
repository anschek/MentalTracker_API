using System;
using System.Collections.Generic;

namespace MentalTracker_API.Models
{
    public partial class TagMetricMatch
    {
        public int TagId { get; set; }
        public int MetricId { get; set; }
        public int? StartingWith { get; set; }
        public int? EndingWith { get; set; }

        public virtual Metric Metric { get; set; } = null!;
        public virtual ArticleTag Tag { get; set; } = null!;
    }
}
