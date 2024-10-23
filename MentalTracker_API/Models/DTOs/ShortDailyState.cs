namespace MentalTracker_API.Models.DTOs
{
    public class ShortDailyState
    {
        public ShortDailyState(DailyState fullDailyState)
        {
            FullDailyStateId = fullDailyState.Id;
            NoteDate = fullDailyState.NoteDate;
            GeneralMoodAssessment = fullDailyState.GeneralMoodAssessment;
            Mood = fullDailyState.Mood;
            Note = fullDailyState.Note;

            MetricsQuality = new Dictionary<string, double>();
            MetricsQuality = fullDailyState.MetricInDailyStates.GroupBy(dailyMetric => dailyMetric.Metric.MetricType)
                .ToDictionary(
                group => group.Key.Name,
                group => group.Sum(dailyMetric => dailyMetric.Metric.IsPositive
                ? dailyMetric.Assessment
                : 6 - dailyMetric.Assessment)
                /5.0 
                / group.Count()
                );
        }

        public ShortDailyState() { }
        public int FullDailyStateId { get; set; }
        public DateOnly NoteDate { get; set; }
        public int GeneralMoodAssessment { get; set; }
        public virtual Mood Mood { get; set; } = null!;
        public string? Note {  get; set; }
        public Dictionary<string, double>? MetricsQuality { get; set; } = null;
    }
}
