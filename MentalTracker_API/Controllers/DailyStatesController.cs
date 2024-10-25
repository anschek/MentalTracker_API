using MentalTracker_API.Models;
using MentalTracker_API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MentalTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailyStatesController : Controller
    {
        private readonly MentalTrackerContext _context;
        public DailyStatesController(MentalTrackerContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Getting full daily state with mood, note and metrics with assessments
        /// </summary>
        [HttpGet("{userId}/{dateOfState}")]
        public async Task<ActionResult<DailyState>> GetUserDailyState([FromRoute] Guid userId, DateOnly dateOfState)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound($"User with id={userId} not found");

            var dailyState = await _context.DailyStates.Include(state => state.MetricInDailyStates)
                .FirstOrDefaultAsync(state => state.User==user && state.NoteDate == dateOfState);
            if (dailyState == null) return NotFound($"State on date: {dateOfState} not found");

            return Ok(dailyState);
        }
        /// <summary>
        ///  Getting full daily states for period.
        ///  The states are returned:
        ///  if period is null - all,
        ///  if lower limit is null - all states up to Period.End
        ///  if upper limit is null - all states from date Period.Beginning
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<ActionResult<ICollection<DailyState>>> GetUserDailyStates([FromQuery] Guid userId, [FromHeader] Period? period)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound($"User with id={userId} not found");

            period              ??= new Period();
            period.Beginning    ??= DateOnly.MinValue;
            period.End          ??= DateOnly.MaxValue;
            if (period.End < period.Beginning) return BadRequest("Start date of period must be lass than end date of period");

            var states = await _context.DailyStates
                .Include(state => state.Mood)
                .Include(state => state.MetricInDailyStates)
                .ThenInclude(dailyMetric => dailyMetric.Metric.MetricType)
                .Where(state => state.User == user && state.NoteDate >= period.Beginning && state.NoteDate <= period.End).ToListAsync();

            if (states == null || states.Count == 0) return NotFound();

            return Ok(states);
        }
        /// <summary>
        /// same as api/DailyStates, but returns short format states
        /// </summary>
        [HttpGet("{userId}/short")]
        public async Task<ActionResult<ICollection<ShortDailyState>>> GetUserDailyStatesShort([FromQuery] Guid userId, [FromHeader] Period? period)
        {
            var result = await GetUserDailyStates(userId, period);
            if(result.Result != null)
            {            
                if (result.Result is NotFoundResult) return NotFound(result.Value);
                return BadRequest(result.Value);
            }
            var shortStates = result.Value.Select(dailyState => new ShortDailyState(dailyState)).ToList();
            return Ok(shortStates);
        }
        /// <summary>
        /// Saving user's state for day
        /// </summary>
        /// <param name="dailyState">Should include: user id, mood, note date, metrics with assessments</param>
        [HttpPost]
        public async Task<IActionResult> CreateNewUserDailyState(DailyState dailyState)
        {
            var user = await _context.Users.FindAsync(dailyState.UserId);
            if (user == null) return NotFound($"User with id={dailyState.UserId} not found");

            if (await _context.DailyStates.FirstOrDefaultAsync(state => state.NoteDate == dailyState.NoteDate) != null)
                return BadRequest($"State on date: {dailyState.NoteDate} already exist");

            var dailyMood = await _context.Moods.FindAsync(dailyState.MoodId);
            if (dailyMood == null) return NotFound($"Mood with id={dailyState.MoodId} nor found");

            dailyState.Id = 0;
            dailyState.User = user;
            dailyState.Mood = dailyMood;

            await _context.DailyStates.AddAsync(dailyState);
            await _context.SaveChangesAsync();
            return Ok();
        }
        /// <summary>
        /// Update user's state for day
        /// </summary>
        /// <param name="dailyState">id must be equal to original id, state body is changed by it</param>
        [HttpPut]
        public async Task<IActionResult> UpdateUserDailyState(DailyState dailyState)
        {
            var updatedState = await _context.DailyStates.FindAsync(dailyState.Id);
            if(updatedState == null) return NotFound();

            updatedState = dailyState;

            _context.Entry(updatedState).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }
        /// <summary>
        /// Deleting daily state by its Id
        /// </summary>
        [HttpDelete("{dailyStateId}")]
        public async Task<IActionResult> DeleteUserDailyState([FromRoute] int dailyStateId)
        {
            var dailyState = await _context.DailyStates.FindAsync(dailyStateId);
            if (dailyState == null) return NotFound($"State with id={dailyStateId} not found");
            
            _context.DailyStates.Remove(dailyState);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
