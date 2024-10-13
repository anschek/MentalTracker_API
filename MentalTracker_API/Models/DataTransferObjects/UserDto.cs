using MentalTracker_API.Models.DataTransferObjects;
using MentalTracker_API.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace MentalTracker_API.Models.DataTransferObjects
{
    public class UserDto
    {
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
    }


}
namespace MentalTracker_API.Models
{
    public partial class User
    {
        public User(UserDto userDto)
        {
            DailyStates = new HashSet<DailyState>();
            Id = Guid.NewGuid();
            Name = userDto.Name;
            DateOfBirth = userDto.DateOfBirth;
            Mail = userDto.Mail;
            Password = userDto.Password;
        }
    }
}