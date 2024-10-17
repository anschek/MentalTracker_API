using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MentalTracker_API.Models
{
    public partial class Mood
    {
        public Mood()
        {
            DailyStates = new HashSet<DailyState>();
            Tags = new HashSet<ArticleTag>();
        }

        public int Id { get; set; }
        public int? MoodBaseId { get; set; }
        public string Name { get; set; } = null!;
        [JsonIgnore]
        public virtual MoodBasis? MoodBase { get; set; }
        [JsonIgnore]
        public virtual ICollection<DailyState> DailyStates { get; set; }

        public virtual ICollection<ArticleTag> Tags { get; set; }
    }
}
