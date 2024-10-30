﻿using MentalTracker_API.Models;
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
        [HttpGet("{userId}/one-state")]
        public async Task<ActionResult<DailyStateDto>> GetUserDailyState([FromRoute] Guid userId, [FromHeader] string dateOfStateS)
        {
            if (!DateOnly.TryParse(dateOfStateS, out var dateOfState)) return BadRequest("Invalid date format");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound($"User with id={userId} not found");

            var dailyState = await _context.DailyStates.Include(state => state.MetricInDailyStates)
                .FirstOrDefaultAsync(state => state.User == user && state.NoteDate == dateOfState);
            if (dailyState == null) return NotFound($"State on date: {dateOfState} not found");

            return Ok(new DailyStateDto(dailyState));
        }
        /// <summary>
        ///  Getting full daily states for period.
        ///  The states are returned:
        ///  if period is null - all,
        ///  if lower limit is null - all states up to Period.End
        ///  if upper limit is null - all states from date Period.Beginning
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<ActionResult<ICollection<DailyStateDto>>> GetUserDailyStates([FromRoute] Guid userId, [FromHeader] string? beginningDateS = null, [FromHeader] string? endDateS = null)
        {
            DateOnly beginningDate, endDate;
            if (string.IsNullOrEmpty(beginningDateS)) beginningDate = DateOnly.MinValue;
            else
            {
                if (!DateOnly.TryParse(beginningDateS, out beginningDate)) return BadRequest($"Invalid date format: {beginningDateS}. Should be like: yyyy-MM-dd");
            }

            if (string.IsNullOrEmpty(endDateS)) endDate = DateOnly.MaxValue;
            else
            {
                if (!DateOnly.TryParse(endDateS, out endDate)) return BadRequest($"Invalid date format: {endDateS}. Should be like: yyyy-MM-dd");
            }

            if (endDate < beginningDate) return BadRequest("Start date of period must be lass than end date of period");
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound($"User with id={userId} not found");

            var states = await _context.DailyStates
                .Include(state => state.Mood)
                .Include(state => state.MetricInDailyStates)
                .ThenInclude(dailyMetric => dailyMetric.Metric.MetricType)
                .Where(state => state.User == user && state.NoteDate >= beginningDate && state.NoteDate <= endDate).ToListAsync();

            if (states == null || states.Count == 0) return NotFound();

            return Ok(states.Select(state => new DailyStateDto(state)).ToList());
        }

        /// <summary>
        /// same as api/DailyStates, but returns short format states
        /// </summary>
        [HttpGet("{userId}/short")]
        public async Task<ActionResult<ICollection<ShortDailyState>>> GetUserDailyStatesShort([FromRoute] Guid userId, [FromHeader] string? beginningDateS = null, [FromHeader] string? endDateS = null)
        {
            DateOnly beginningDate, endDate;
            if (string.IsNullOrEmpty(beginningDateS)) beginningDate = DateOnly.MinValue;
            else
            {
                if (!DateOnly.TryParse(beginningDateS, out beginningDate)) return BadRequest($"Invalid date format: {beginningDateS}. Should be like: yyyy-MM-dd");
            }

            if (string.IsNullOrEmpty(endDateS)) endDate = DateOnly.MaxValue;
            else
            {
                if (!DateOnly.TryParse(endDateS, out endDate)) return BadRequest($"Invalid date format: {endDateS}. Should be like: yyyy-MM-dd");
            }

            if (endDate < beginningDate) return BadRequest("Start date of period must be lass than end date of period");
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound($"User with id={userId} not found");

            var states = await _context.DailyStates
                .Include(state => state.Mood)
                .Include(state => state.MetricInDailyStates)
                .ThenInclude(dailyMetric => dailyMetric.Metric.MetricType)
                .Where(state => state.User == user && state.NoteDate >= beginningDate && state.NoteDate <= endDate).ToListAsync();

            if (states == null || states.Count == 0) return NotFound();

            return Ok(states.Select(state => new ShortDailyState(state)).ToList());
        }
        /// <summary>
        /// Saving user's state for day
        /// </summary>
        /// <param name="dailyStateDto">Should include: user id, mood, note date, metrics with assessments</param>
        [HttpPost]
        public async Task<IActionResult> CreateNewUserDailyState([FromBody] DailyStateDto dailyStateDto)
        {
            var user = await _context.Users.FindAsync(dailyStateDto.UserId);
            if (user == null) return NotFound($"User with id={dailyStateDto.UserId} not found");

            if (await _context.DailyStates.FirstOrDefaultAsync(state =>
            state.UserId == dailyStateDto.UserId && state.NoteDate == dailyStateDto.NoteDate) != null)
                return BadRequest($"State on date: {dailyStateDto.NoteDate} already exist");

            var dailyMood = await _context.Moods.FindAsync(dailyStateDto.MoodId);
            if (dailyMood == null) return NotFound($"Mood with id={dailyStateDto.MoodId} not found");

            DailyState dailyState = new DailyState
            {
                UserId = user.Id,
                User = user,
                NoteDate = dailyStateDto.NoteDate,
                GeneralMoodAssessment = dailyStateDto.GeneralMoodAssessment,
                MoodId = dailyMood.Id,
                Mood = dailyMood,
                Note = dailyStateDto.Note,
                MetricInDailyStates = dailyStateDto.MetricInDailyStates
                 .Select(state => new MetricInDailyState
                 {
                     MetricId = state.MetricId,
                     Assessment = state.Assessment
                 }).ToList()
            };

            await _context.DailyStates.AddAsync(dailyState);
            await _context.SaveChangesAsync();
            return Ok();
        }
        /// <summary>
        /// Update user's state for day
        /// </summary>
        /// <param name="dailyState">id must be equal to original id, state body is changed by it</param>
        [HttpPut]
        public async Task<IActionResult> UpdateUserDailyState([FromBody] DailyStateDto dailyState)
        {
            var updatedState = await _context.DailyStates.Include(state => state.MetricInDailyStates)
                .FirstOrDefaultAsync(state => state.Id == dailyState.Id);
            if (updatedState == null) return NotFound();

            updatedState.GeneralMoodAssessment = dailyState.GeneralMoodAssessment;
            updatedState.MoodId = dailyState.MoodId;
            updatedState.Mood = await _context.Moods.FindAsync(dailyState.MoodId);
            updatedState.Note = dailyState.Note;

            _context.MetricInDailyStates.RemoveRange(updatedState.MetricInDailyStates);
            updatedState.MetricInDailyStates = dailyState.MetricInDailyStates
                .Select(state => new MetricInDailyState
                {
                    MetricId = state.MetricId,
                    Assessment = state.Assessment
                }).ToList();

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
