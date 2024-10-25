using MentalTracker_API.Models;
using MentalTracker_API.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MentalTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly MentalTrackerContext _context;
        public AnalyticsController(MentalTrackerContext context)
        {
            _context = context;
        }

        enum AnalyticTypes
        {
            TimeFuncOfGeneralAssessment,
            TimeFuncOfMetricsTypes,
            TimeFuncOfOneMetric,
            MoodBasesSum,
            MoodsInMoodBasesSum
        }
        /// <summary>
        /// Getting all types of possible giagrams/ function graphs
        /// </summary>
        [HttpGet("types")]
        public Dictionary<int, string> GetAnalyticTypes() =>
            Enum.GetValues(typeof(AnalyticTypes)).Cast<AnalyticTypes>()
            .ToDictionary(
                type => (int)type,
                type => type.ToString()
            );
        /// <summary>
        /// Getting a set of analyzed data, for types:
        /// 0-2 - Dict&lt;DateOnly,int&gt;
        /// 1 -  Dict&lt;DateOnly,double&gt;
        /// 3 - MoodBasis[]
        /// 4 - Mood[]
        /// </summary>
        /// <param name="analyticTypeId"> Number from 0 to 4 according to api/Analytics/types </param>
        /// <param name="period"> 
        ///  if period is null - all dates,
        ///  if lower limit is null - all dates up to Period.End
        ///  if upper limit is null - all dates from date Period.Beginning </param>
        /// <param name="argumentId">
        /// (1) TimeFuncOfMetricsTypes - metric type id
        /// (2) TimeFuncOfOneMetric - metric id
        /// (4) MoodsInMoodBasesSum - mood base id
        /// </param>
        [HttpGet("{userId}/{analyticTypeId}")]
        public async Task<ActionResult<object>> GetUserAnalyticData([FromRoute] Guid userId, [FromRoute] int analyticTypeId, [FromHeader] Period? period, int? argumentId=null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound($"User with id={userId} not found");

            period ??= new Period();
            period.Beginning ??= DateOnly.MinValue;
            period.End ??= DateOnly.MaxValue;
            if (period.End < period.Beginning) return BadRequest("Start date of period must be lass than end date of period");

            var states = await _context.DailyStates
                .Include(state => state.Mood.MoodBase)
                .Include(state => state.MetricInDailyStates)
                .ThenInclude(dailyMetric => dailyMetric.Metric.MetricType)
                .Where(state => state.User == user && state.NoteDate >= period.Beginning && state.NoteDate <= period.End).ToListAsync();

            if (states == null || states.Count == 0) return BadRequest("Analytics can not be defined, no daily states for period");

            switch ((AnalyticTypes)analyticTypeId)
            {
                case AnalyticTypes.TimeFuncOfGeneralAssessment:

                    Dictionary<DateOnly, int> analytics0 = states.ToDictionary(
                        state => state.NoteDate,
                        state => state.GeneralMoodAssessment);
                    return Ok(analytics0);
                
                case AnalyticTypes.TimeFuncOfMetricsTypes:
                    
                    var metricType = await _context.MetricTypes.FindAsync(argumentId);
                    if (metricType == null) return BadRequest($"Metric type with id={argumentId} not found");


                    Dictionary<DateOnly, double> analytics1 = states.ToDictionary(
                        state => state.NoteDate,
                        state => state.MetricInDailyStates.Where(dailyMetric => dailyMetric.Metric.MetricType == metricType)
                        .Sum(dailyMetric => dailyMetric.Metric.IsPositive ? dailyMetric.Assessment : 6 - dailyMetric.Assessment)
                        / 5.0 / state.MetricInDailyStates.Where(dailyMetric => dailyMetric.Metric.MetricType == metricType).Count()
                        );
                    return Ok(analytics1);
                
                case AnalyticTypes.TimeFuncOfOneMetric:

                    var metric = await _context.Metrics.FindAsync(argumentId);
                    if (metric == null) return NotFound($"Metric with id={argumentId} not found");

                    Dictionary<DateOnly, int> analytics2 = states.ToDictionary(
                        state => state.NoteDate,
                        state => state.MetricInDailyStates.First(dailyMetric =>
                        dailyMetric.Metric == metric).Assessment);              
                        
                    return Ok(analytics2);
                    
                case AnalyticTypes.MoodBasesSum:

                    List<MoodBasis> analytics3 = states.Select(state => state.Mood.MoodBase).ToList();
                    return Ok(analytics3);
                    
                case AnalyticTypes.MoodsInMoodBasesSum:
                    
                    var moodBase = await _context.MoodBases.FindAsync(argumentId);
                    if (moodBase == null) return NotFound($"Mood base with id={argumentId} not found");

                    List<Mood> analytics4 = states.Select(state => state.Mood).Where(mood => mood.MoodBase == moodBase).ToList();
                    return Ok(analytics4);

                default: return BadRequest($"Analytic type with id={analyticTypeId} not found");

            }
        }
        /// <summary>
        /// etting articles recommended to user (based on their latest states)
        /// </summary>
        /// <param name="lastStates"> Number of last notes for which recommendations are determined </param>
        /// <returns></returns>
        [HttpGet("recommendations/{userId}")]
        public async Task<ActionResult<IDictionary<Article, int>>> GetUserRecommendations([FromRoute] Guid userId, [FromQuery] int lastStates = 7)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound($"User with id={userId} not found");

            var articles = await _context.Articles
                .Include(article => article.Tags).ThenInclude(tag => tag.TagMetricMatches) //Metric id 
                .Include(article => article.Tags).ThenInclude(tag => tag.Moods).ToListAsync();

            var userStates = await _context.DailyStates.Include(state => state.MetricInDailyStates)
                .TakeLast(lastStates).ToListAsync();

            var lastMoodsIds = userStates.Select(state => state.Mood.Id).ToList();
            var lastMetrics = userStates.SelectMany(state => state.MetricInDailyStates).ToList();

            var recommendedArticles = new Dictionary<Article, int>();

            foreach (var article in articles)
            {
                var articleMoods = article.Tags.SelectMany(tag => tag.Moods).ToList();
                var articleMetricsMatches = article.Tags.SelectMany(tag => tag.TagMetricMatches).ToList();

                foreach (var mood in articleMoods)
                {
                    if (lastMoodsIds.Contains(mood.Id))
                    {
                        ++recommendedArticles[article];
                        break;
                    }
                }

                foreach (var metricMatch in articleMetricsMatches)
                {
                    var metric = lastMetrics.Find(dailyMetric => dailyMetric.Metric == metricMatch.Metric);
                    if(metric.Assessment <= metricMatch.EndingWith && metric.Assessment >= metricMatch.StartingWith) ++recommendedArticles[article] ;
                }
            }

            return Ok(recommendedArticles.OrderByDescending(article => article.Value).ToDictionary(article => article.Key, article => article.Value));
        }

    }
}
