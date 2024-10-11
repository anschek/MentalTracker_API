using System;
using System.Collections.Generic;

namespace MentalTracker_API.Models
{
    public partial class Metric
    {
        public Metric()
        {
            MetricInDailyStates = new HashSet<MetricInDailyState>();
        }

        public int Id { get; set; }
        public int? MetricTypeId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsPositive { get; set; }

        public virtual MetricType? MetricType { get; set; }
        public virtual ICollection<MetricInDailyState> MetricInDailyStates { get; set; }
    }
}
