using System;
using System.Collections.Generic;

namespace MentalTracker_API.Models
{
    public partial class MetricType
    {
        public MetricType()
        {
            Metrics = new HashSet<Metric>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Metric> Metrics { get; set; }
    }
}
