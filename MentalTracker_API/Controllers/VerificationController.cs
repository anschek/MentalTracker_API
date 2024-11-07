using MentalTracker_API.Helpers;
using MentalTracker_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace MentalTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationController : Controller
    {
        private readonly MentalTrackerContext _context;
        private static ConcurrentDictionary<string, (string Code, DateTime Expiry)> _verificationCodes = new();
        private readonly EmailService _emailService;
        private static Random _random = new();

        public VerificationController(EmailService emailService, MentalTrackerContext context)
        {
            _emailService = emailService;
            _context = context;
        }
        /// <summary>
        /// Sending a verification code to entered email address (code expir: 60s)
        /// </summary>
        [HttpPost("request-code")]
        public async Task<IActionResult> RequestVerificationCode([FromHeader] string recipientEmail)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail)) return BadRequest("Email is required");

            var code = _random.Next(100000, 999999).ToString();
            var expiryTime = DateTime.UtcNow.AddSeconds(60);
            _verificationCodes[recipientEmail] = (code, expiryTime);

            await _emailService.SendEmail(recipientEmail, "Your verification code to access the MoodWave", code);

            return Ok();
        }
        /// <summary>
        /// Check if mail and code match, returns 200 if everything is correct, else - 400
        /// </summary>
        [HttpPost("verify")]
        public IActionResult VerifyCode([FromHeader] string recipientEmail, [FromHeader] string code)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail) || string.IsNullOrWhiteSpace(code)) return BadRequest("Email and code are required");

            if (_verificationCodes.TryGetValue(recipientEmail, out var expectedCode))
            {
                //don't remove code, because if queri is repeated, error should be about code expiry
                if(expectedCode.Expiry < DateTime.UtcNow)  return BadRequest("The verification code has expired");
                
                if (expectedCode.Code == code)
                {
                    _verificationCodes.TryRemove(recipientEmail, out _);
                    return Ok();
                }
                return BadRequest("Invalid verification code");
            }

            return BadRequest("Email not found");
        }        
        
        /// <summary>
        /// Change user password with mail verify
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromHeader] string recipientEmail, [FromHeader] string code, [FromHeader] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail) || string.IsNullOrWhiteSpace(code)) return BadRequest("Email and code are required");

            if (_verificationCodes.TryGetValue(recipientEmail, out var expectedCode))
            {
                //don't remove code, because if queri is repeated, error should be about code expiry
                if(expectedCode.Expiry < DateTime.UtcNow)  return BadRequest("The verification code has expired");
                
                if (expectedCode.Code == code)
                {
                    _verificationCodes.TryRemove(recipientEmail, out _);

                    var user = await _context.Users.FirstOrDefaultAsync(existingUser => existingUser.Mail == recipientEmail);
                    if (user == null) return NotFound($"User with mail={recipientEmail} not found");

                    user.Password = UsersController.GetPasswordHash(newPassword);
                    await _context.SaveChangesAsync();

                    return Ok();
                }
                return BadRequest("Invalid verification code");
            }

            return BadRequest("Email not found");
        }
    }
}
