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

        [HttpGet("{userId}/{date}")]
        public async Task<ActionResult<DailyState>> GetUserDailyState(Guid userId, DateOnly dateOfState)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound($"User with id={userId} nor found");

            var dailyState = await _context.DailyStates.Include(state => state.MetricInDailyStates)
                .FirstOrDefaultAsync(state => state.User==user && state.NoteDate == dateOfState);
            if (dailyState == null) return NotFound($"State on date: {dateOfState} not found");

            return dailyState;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ICollection<DailyState>>> GetUserDailyStates(Guid userId, [FromHeader] Period? period)
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

            return states;
        }

        
        [HttpGet("{userId}/short")]
        public async Task<ActionResult<ICollection<ShortDailyState>>> GetUserDailyStatesShort(Guid userId, [FromHeader] Period? period)
        {
            var result = await GetUserDailyStates(userId, period);
            if(result.Result != null)
            {            
                if (result.Result is NotFoundResult) return NotFound(result.Value);
                return BadRequest(result.Value);
            }
            var shortStates = result.Value.Select(dailyState => new ShortDailyState(dailyState)).ToList();
            return shortStates;
        }

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

        [HttpDelete("dailyStateId")]
        public async Task<IActionResult> DeleteUserDailyState(int dailyStateId)
        {
            var dailyState = await _context.DailyStates.FindAsync(dailyStateId);
            if (dailyState == null) return NotFound($"State with id={dailyStateId} not found");
            
            _context.DailyStates.Remove(dailyState);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
