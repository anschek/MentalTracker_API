namespace MentalTracker_API.Models.DTOs
{
    public class DailyStateDto
    {
        public DailyStateDto()
        {
            MetricInDailyStates = new List<MetricInDailyStateDto>();
        }
        public DailyStateDto(DailyState dailyState)
        {
            Id = dailyState.Id;
            UserId = dailyState.UserId;
            NoteDate = dailyState.NoteDate;
            GeneralMoodAssessment = dailyState.GeneralMoodAssessment;
            MoodId = dailyState.MoodId;
            Note = dailyState.Note;
            MetricInDailyStates = new List<MetricInDailyStateDto>();
            if(dailyState.MetricInDailyStates != null && dailyState.MetricInDailyStates.Count > 0) 
            MetricInDailyStates = dailyState.MetricInDailyStates.Select(state => new MetricInDailyStateDto
            {
                 Assessment = state.Assessment,
                 MetricId = state.MetricId,
                 Metric = state.Metric
            }).ToList();
        }
        public int? Id { get; set; }
        public Guid UserId { get; set; }
        public DateOnly NoteDate { get; set; }
        public int GeneralMoodAssessment { get; set; }
        public int MoodId { get; set; }
        public string? Note { get; set; }
        public List<MetricInDailyStateDto> MetricInDailyStates { get; set; }
    }
}
