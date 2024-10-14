using MentalTracker_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MentalTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoodsController : Controller
    {
        private readonly MentalTrackerContext _context;
        public MoodsController(MentalTrackerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MoodBasis>>> GetMoodsWithBases()
        {
            try
            {
                var moodBases = await _context.MoodBases.Include(moodBase => moodBase.Moods).ToListAsync();
                if (moodBases == null || moodBases.Count == 0) return NotFound();
                return moodBases;
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateNewMood([FromQuery] int moodBaseId, [FromQuery] string newMoodName)
        {
            try
            {
                var moodBase = await _context.MoodBases.FirstOrDefaultAsync(moodBase => moodBase.Id == moodBaseId);
                if (moodBase == null) return NotFound($"Mood base with id = {moodBaseId} not found");
                var newMood = new Mood { Id = 0, Name = newMoodName, MoodBaseId = moodBaseId };
                await _context.Moods.AddAsync(newMood);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }
    }
}
