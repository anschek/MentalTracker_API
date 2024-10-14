using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MentalTracker_API.Models
{
    public partial class User
    {
        public User()
        {
            DailyStates = new HashSet<DailyState>();
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        [EmailAddress]
        public string Mail { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<DailyState> DailyStates { get; set; }
    }
}
