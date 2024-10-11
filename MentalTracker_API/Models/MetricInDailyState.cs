using System;
using System.Collections.Generic;

namespace MentalTracker_API.Models
{
    public partial class MetricInDailyState
    {
        public int MetricId { get; set; }
        public int Assessment { get; set; }
        public int DailyStateId { get; set; }

        public virtual DailyState DailyState { get; set; } = null!;
        public virtual Metric Metric { get; set; } = null!;
    }
}
