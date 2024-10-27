using Microsoft.AspNetCore.Mvc.Testing;
using MentalTracker_API;
using System.Diagnostics;

namespace MentalTracker_API_Test
{
    [TestClass]
    public class ApiTests
    {
        private static HttpClient _client;
        private static WebApplicationFactory<Program> _factory;

        [ClassInitialize]
        public static void Initialixe(TestContext context)
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [TestMethod]
        public async Task CreateNewUser_PassExistingMailWithCorrectPassword_ReturnsExistingUser()
        {
            string url = "https://localhost:7254/api/Users/";
            string queryParams = "?mail=anschek@yandex.ru&password=admin";
            url += queryParams;

            HttpResponseMessage response = await _client.GetAsync(url);
            Assert.IsTrue(response.IsSuccessStatusCode);
            string content = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(content);
            Debug.WriteLine(content);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}