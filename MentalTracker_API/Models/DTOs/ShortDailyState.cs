namespace MentalTracker_API.Models.DTOs
{
    public class MetricQuality
    {
        public string MetricTypeName { get; set; }
        public double Assessment { get; set; }
    }

    public class ShortDailyState
    {
        public ShortDailyState(DailyState fullDailyState)
        {
            FullDailyStateId = fullDailyState.Id;
            NoteDate = fullDailyState.NoteDate;
            GeneralMoodAssessment = fullDailyState.GeneralMoodAssessment;
            Mood = fullDailyState.Mood;
            Note = fullDailyState.Note;

            MetricsQuality = new List<MetricQuality>();
            MetricsQuality = fullDailyState.MetricInDailyStates.GroupBy(dailyMetric => dailyMetric.Metric.MetricType)
                .Select( group =>
                new MetricQuality
                {
                    MetricTypeName =  group.Key.Name,
                    Assessment = group.Sum(dailyMetric => dailyMetric.Metric.IsPositive
                ? dailyMetric.Assessment
                : 6 - dailyMetric.Assessment)
                /5.0 
                / group.Count()
                }).ToList();
        }

        public ShortDailyState() { }
        public int FullDailyStateId { get; set; }
        public DateOnly NoteDate { get; set; }
        public int GeneralMoodAssessment { get; set; }
        public virtual Mood Mood { get; set; } = null!;
        public string? Note {  get; set; }
        public List<MetricQuality>? MetricsQuality { get; set; } = null;
    }
}
