using MentalTracker_API.Helpers;
using MentalTracker_API.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using MentalTracker_API;

namespace MentalTracker_API_Test
{
    public class ApiTests
    {
        protected static WebApplicationFactory<Program> _factory;
        protected static HttpClient _client;
        protected static MentalTrackerContext _context;
        protected static string _baseUrl = "https://localhost:7254/api/";
        protected static JsonSerializerOptions _customJsonOptions;

        public ApiTests()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
            _context = new MentalTrackerContext();
            _customJsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new DateOnlyConverter() }
            };
        }

        [ClassCleanup]
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose();
            _context.Dispose();
        }
    }
}
