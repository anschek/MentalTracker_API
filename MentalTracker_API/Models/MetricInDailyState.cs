using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MentalTracker_API.Models
{
    public partial class MetricInDailyState
    {
         [JsonIgnore]
        public int MetricId { get; set; }
        public int Assessment { get; set; }
        public int DailyStateId { get; set; }

        [JsonIgnore]
        public virtual DailyState DailyState { get; set; } = null!;
        public virtual Metric Metric { get; set; } = null!;
    }
}
