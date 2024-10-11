using System;
using System.Collections.Generic;

namespace MentalTracker_API.Models
{
    public partial class User
    {
        public User()
        {
            DailyStates = new HashSet<DailyState>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateOnly DateOfBirth { get; set; }
        public string Mail { get; set; } = null!;
        public string Password { get; set; } = null!;

        public virtual ICollection<DailyState> DailyStates { get; set; }
    }
}
