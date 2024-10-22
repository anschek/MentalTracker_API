using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MentalTracker_API.Models
{
    public partial class DailyState
    {
        public DailyState()
        {
            MetricInDailyStates = new HashSet<MetricInDailyState>();
        }

        public int Id { get; set; }
        public Guid UserId { get; set; }
        public DateOnly NoteDate { get; set; }
        public int GeneralMoodAssessment { get; set; }
        public int MoodId { get; set; }
        public string? Note { get; set; }

        [JsonIgnore]
        public virtual Mood Mood { get; set; } = null!;
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
        public virtual ICollection<MetricInDailyState> MetricInDailyStates { get; set; }
    }
}
