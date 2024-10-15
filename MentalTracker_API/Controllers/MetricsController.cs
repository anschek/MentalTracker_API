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

        [HttpGet]
        public async Task<ActionResult<ICollection<MetricType>>> GetMetricsWithTypes()
        {
            var metricTypes = await _context.MetricTypes.Include(metricType => metricType.Metrics).ToListAsync();
            if(metricTypes == null || metricTypes.Count == 0) return NotFound();
            return metricTypes;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewMetric([FromQuery] int metricTypeId, [FromQuery] string newMetricName, [FromQuery] bool newMetricIsPositive)
        {
            var mentalType = await _context.MetricTypes.FindAsync(metricTypeId);
            if (mentalType == null) return NotFound($"Metric type with id = {metricTypeId} not found");
            var newMetric = new Metric { Id=0, Name = newMetricName, MetricTypeId = metricTypeId, IsPositive = newMetricIsPositive };
            await _context.Metrics.AddAsync(newMetric);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
