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
        /// 0-2 - Dict&lt;string date,int assessment&gt;
        /// 1 -  Dict&lt;string date,double quality percentage&gt;
        /// 3 -  Dict&lt;int mood basis id, int count&gt;
        /// 4 - Dict&lt;int mood id, int count&gt;
        /// </summary>
        /// <param name="analyticTypeId"> Number from 0 to 4 according to api/Analytics/types </param>
        /// <param name="beginningDateS"> 
        ///  if all period is null - all dates,
        ///  if lower limit is null - all dates up to Period.End </param>        
        /// <param name="endDateS"> 
        ///  if all period is null - all dates,
        ///  if upper limit is null - all dates from date Period.Beginning </param>
        /// <param name="argumentId">
        /// (1) TimeFuncOfMetricsTypes - metric type id
        /// (2) TimeFuncOfOneMetric - metric id
        /// (4) MoodsInMoodBasesSum - mood base id
        /// </param>
        [HttpGet("{userId}")]
        public async Task<ActionResult<object>> GetUserAnalyticData(
            [FromRoute] Guid userId,
            [FromQuery] int analyticTypeId,
            [FromHeader] string? beginningDateS = null,
            [FromHeader] string? endDateS = null,
            [FromHeader] int? argumentId = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound($"User with id={userId} not found");

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

            var states = await _context.DailyStates
                .Include(state => state.Mood.MoodBase)
                .Include(state => state.MetricInDailyStates)
                .ThenInclude(dailyMetric => dailyMetric.Metric.MetricType)
                .Where(state => state.UserId == user.Id && state.NoteDate >= beginningDate && state.NoteDate <= endDate).ToListAsync();

            if (states == null || states.Count == 0) return BadRequest("Analytics can not be defined, no daily states for period");

            switch ((AnalyticTypes)analyticTypeId)
            {
                case AnalyticTypes.TimeFuncOfGeneralAssessment:

                    Dictionary<string, int> analytics0 = states.ToDictionary(
                        state => state.NoteDate.ToString("yyyy-MM-dd"),
                        state => state.GeneralMoodAssessment);
                    return Ok(analytics0);

                case AnalyticTypes.TimeFuncOfMetricsTypes:

                    var metricType = await _context.MetricTypes.FindAsync(argumentId);
                    if (metricType == null) return BadRequest($"Metric type with id={argumentId} not found");
                    Dictionary<string, double> analytics1 = new Dictionary<string, double>();

                    foreach (var state in states)
                    {
                        var metricsByType = state.MetricInDailyStates.Where(dailyMetric => dailyMetric.Metric.MetricTypeId == metricType.Id);

                        analytics1.Add(state.NoteDate.ToString("yyyy-MM-dd"), metricsByType
                            .Sum(dailyMetric => dailyMetric.Metric.IsPositive ? dailyMetric.Assessment : 6 - dailyMetric.Assessment) // sum (+) 1:1,2:2..5:5 (-) 1:5,2:4..5:1
                            / 5.0 / metricsByType.Count() // /5.0 - from 5-point system to 1-point
                        );
                    }
                    return Ok(analytics1);

                case AnalyticTypes.TimeFuncOfOneMetric:

                    var metric = await _context.Metrics.FindAsync(argumentId);
                    if (metric == null) return NotFound($"Metric with id={argumentId} not found");

                    Dictionary<string, int> analytics2 = states.ToDictionary(
                        state => state.NoteDate.ToString("yyyy-MM-dd"),
                        state => state.MetricInDailyStates.First(dailyMetric =>
                        dailyMetric.MetricId == metric.Id).Assessment); // dailyState -> dailyMetrics -> (!) cur metric -> assessment

                    return Ok(analytics2);

                case AnalyticTypes.MoodBasesSum:
                    Dictionary<int, int> analytics3 = states
                        .Select(state => state.Mood.MoodBase)
                        .GroupBy(moodBasis => moodBasis.Id)
                        .ToDictionary(group => group.Key, group => group.Count());
                    return Ok(analytics3);

                case AnalyticTypes.MoodsInMoodBasesSum:

                    var moodBase = await _context.MoodBases.FindAsync(argumentId);
                    if (moodBase == null) return NotFound($"Mood base with id={argumentId} not found");

                    Dictionary<int, int> analytics4 = states
                        .Select(state => state.Mood)
                        .Where(mood => mood.MoodBaseId == moodBase.Id)
                        .GroupBy(mood => mood.Id)
                        .ToDictionary(group => group.Key, group=> group.Count());
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
        public async Task<ActionResult<ICollection<Article>>> GetUserRecommendations([FromRoute] Guid userId, [FromQuery] int lastStates = 7)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound($"User with id={userId} not found");

            var articles = await _context.Articles
                .Include(article => article.Tags).ThenInclude(tag => tag.TagMetricMatches) //Metric id 
                .Include(article => article.Tags).ThenInclude(tag => tag.Moods).ToListAsync();

            var userStates = (await _context.DailyStates
                .Where(state => state.UserId == userId)
                .Include(state => state.MetricInDailyStates)
                .ThenInclude(dailyMetric => dailyMetric.Metric)
                .ToListAsync())
                .TakeLast(lastStates)
                .ToList();

            var lastMoodsIds = userStates.Select(state => state.MoodId).ToList();
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
                        recommendedArticles[article] = recommendedArticles.GetValueOrDefault(article) + 1;
                        break;
                    }
                }

                foreach (var metricMatch in articleMetricsMatches)
                {
                    var metric = lastMetrics.Find(dailyMetric => dailyMetric.MetricId == metricMatch.MetricId);
                    if (metric != null && metric.Assessment <= metricMatch.EndingWith && metric.Assessment >= metricMatch.StartingWith)
                        recommendedArticles[article] = recommendedArticles.GetValueOrDefault(article) + 1;
                }
            }

            recommendedArticles = recommendedArticles.OrderByDescending(article => article.Value).ToDictionary(article => article.Key, article => article.Value);
            return Ok(recommendedArticles.Select(article => article.Key).ToList());
        }

    }
}
