using MentalTracker_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace MentalTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly MentalTrackerContext _context;
        public UsersController(MentalTrackerContext context)
        {
            _context = context;
        }
        private string GetPasswordHash(string password)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashedBytes = sha512.ComputeHash(bytes);
                return string.Join(" ", hashedBytes);
            }
        }
        /// <summary>
        /// Getting user object by mail and password
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<User>> GetUser(string mail, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Mail == mail
                && user.Password == GetPasswordHash(password));

            if (user == null) return NotFound();
            return user;
        }
        /// <summary>
        /// Creating a new user
        /// </summary>
        /// <param name="user">Includes: mail, password, date of birth, name </param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<User>> CreateNewUser(User user)
        {
            user.Password = GetPasswordHash(user.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return _context.Users.First(user => user.Mail == user.Mail);
        }
        /// <summary>
        /// User data update
        /// </summary>
        [HttpPut("change-personal-data")]
        public async Task<IActionResult> UpdateUserData(User updatedUser)
        {
            User? existingUser = await _context.Users.FindAsync(updatedUser.Id);
            if (existingUser == null) return NotFound("User not found.");

            existingUser.Name = updatedUser.Name;
            existingUser.Mail = updatedUser.Mail;
            existingUser.DateOfBirth = updatedUser.DateOfBirth;

            _context.Entry(updatedUser).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }
        /// <summary>
        /// User password update
        /// </summary>
        [HttpPut("change-password")]
        public async Task<IActionResult> UpdateUserPassword(Guid updatedUserId, string oldPassword, string newPassword)
        {
            User? existingUser = await _context.Users.FindAsync(updatedUserId);
            if (existingUser == null) return NotFound("User not found.");

            if (existingUser.Password != GetPasswordHash(oldPassword)) return BadRequest("Wrong old password");

            existingUser.Password = GetPasswordHash(newPassword);
            _context.Entry(existingUser).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}


