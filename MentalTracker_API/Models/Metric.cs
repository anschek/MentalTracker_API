using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MentalTracker_API.Models
{
    public partial class Metric
    {
        public Metric()
        {
            MetricInDailyStates = new HashSet<MetricInDailyState>();
            TagMetricMatches = new HashSet<TagMetricMatch>();
        }

        public int Id { get; set; }
        [JsonIgnore]
        public int? MetricTypeId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsPositive { get; set; }
        [JsonIgnore]
        public virtual MetricType? MetricType { get; set; }
        [JsonIgnore]
        public virtual ICollection<MetricInDailyState> MetricInDailyStates { get; set; }
        [JsonIgnore]
        public virtual ICollection<TagMetricMatch> TagMetricMatches { get; set; }
    }
}
