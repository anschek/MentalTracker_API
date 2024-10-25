using MentalTracker_API.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace MentalTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationController : Controller
    {
        //private static ConcurrentDictionary<string, string> _verificationCodes = new();
        private static ConcurrentDictionary<string, (string Code, DateTime Expiry)> _verificationCodes = new();
        private readonly EmailService _emailService;
        private static Random _random = new();

        public VerificationController(EmailService emailService)
        {
            _emailService = emailService;
        }
        /// <summary>
        /// Sending a verification code to entered email address (code expir: 60s)
        /// </summary>
        [HttpPost("request-code")]
        public async Task<IActionResult> RequestVerificationCode([FromQuery] string recipientEmail)
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
        public IActionResult VerifyCode([FromQuery] string recipientEmail, [FromQuery] string code)
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
    }
}
