using System;
using System.Collections.Generic;

namespace MentalTracker_API.Models
{
    public partial class TagMoodMatch
    {
        public int TagId { get; set; }
        public int MoodId { get; set; }

        public virtual Mood Mood { get; set; } = null!;
        public virtual ArticleTag Tag { get; set; } = null!;
    }
}
