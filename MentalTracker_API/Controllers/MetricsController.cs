using MentalTracker_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MentalTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetricsController : Controller
    {
        private readonly MentalTrackerContext _context;
        public MetricsController(MentalTrackerContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Getting metrics within their types
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ICollection<MetricType>>> GetMetricsWithTypes()
        {
            var metricTypes = await _context.MetricTypes.Include(metricType => metricType.Metrics).ToListAsync();
            if(metricTypes == null || metricTypes.Count == 0) return NotFound();
            return Ok(metricTypes);
        }
        /// <summary>
        /// Creating a new metric
        /// </summary>
        /// <param name="metricTypeId"> Metric type (usually mental(1) or physical(2)) </param>
        /// <param name="newMetricName"> Metric itself </param>
        /// <param name="newMetricIsPositive"> New metric is positive - true, negative - false</param>
        [HttpPost]
        public async Task<IActionResult> CreateNewMetric([FromQuery] int metricTypeId, [FromQuery] string newMetricName, [FromQuery] bool newMetricIsPositive)
        {
            var mentalType = await _context.MetricTypes.FindAsync(metricTypeId);
            if (mentalType == null) return NotFound($"Metric type with id={metricTypeId} not found");

            var newMetric = new Metric { Id=0, Name = newMetricName, MetricTypeId = metricTypeId, IsPositive = newMetricIsPositive };
            await _context.Metrics.AddAsync(newMetric);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
