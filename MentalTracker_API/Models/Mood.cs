using System;
using System.Collections.Generic;

namespace MentalTracker_API.Models
{
    public partial class Mood
    {
        public Mood()
        {
            DailyStates = new HashSet<DailyState>();
        }

        public int Id { get; set; }
        public int? MoodBaseId { get; set; }
        public string Name { get; set; } = null!;

        public virtual MoodBasis? MoodBase { get; set; }
        public virtual ICollection<DailyState> DailyStates { get; set; }
    }
}
