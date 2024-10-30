using System.Text.Json.Serialization;

namespace MentalTracker_API.Models.DTOs
{
    public class MetricInDailyStateDto
    {
        public int MetricId { get; set; }
        public int Assessment { get; set; }
        public Metric? Metric { get; set; }
    }
}
