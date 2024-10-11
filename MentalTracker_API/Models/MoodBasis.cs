using System;
using System.Collections.Generic;

namespace MentalTracker_API.Models
{
    public partial class MoodBasis
    {
        public MoodBasis()
        {
            Moods = new HashSet<Mood>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Mood> Moods { get; set; }
    }
}
