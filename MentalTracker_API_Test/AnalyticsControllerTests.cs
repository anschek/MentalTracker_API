using MentalTracker_API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MentalTracker_API_Test
{
    [TestClass]
    public class AnalyticsControllerTests : ApiTests
    {
        public AnalyticsControllerTests() : base() { _baseUrl += "Analytics/"; }
        
        private static User user;
        private static DailyStatesControllerTests dsMethods;
        private static List<DailyState> dailyStates = new List<DailyState>();
        private static int statesCount = 7;
        private static DateOnly startDate = new DateOnly(2024, 10, 1);

        public async Task GetUserAnalyticData_PassCorrectDataWithoutPeriod_ReturnsCorrectAnalytics()
        {
            //0 - TimeFuncOfGeneralAssessment

            // Arrange
            int analyticTypeId = 0;
            string route = $"{user.Id}/";
            string queryParams = $"?analyticTypeId={analyticTypeId}";

            // Act
            HttpResponseMessage response = await _client.GetAsync(_baseUrl + route + queryParams);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            Dictionary<string, int>? returnedAnalytics0 = JsonSerializer.Deserialize<Dictionary<string, int>>(stringContent, _customJsonOptions);
            Assert.IsNotNull(returnedAnalytics0);
            CollectionAssert.AllItemsAreNotNull(returnedAnalytics0);
            Assert.AreEqual(returnedAnalytics0.Count, statesCount);
            for(int i=0; i< statesCount; ++i)
            {
                if (returnedAnalytics0.TryGetValue(dailyStates[i].NoteDate.ToString("yyyy-MM-dd"), out int assessment))
                    Assert.AreEqual(assessment, dailyStates[i].GeneralMoodAssessment);
                
                else Assert.Fail();
            }

            // 1 - TimeFuncOfMetricsTypes, argument - metric type id

            // Arrange
            analyticTypeId = 1;
            queryParams = $"?analyticTypeId={analyticTypeId}";
            int argumentId = 1; // mental metrics
            _client.DefaultRequestHeaders.Add("argumentId", argumentId.ToString());

            // Act
            response = await _client.GetAsync(_baseUrl + route + queryParams);
            stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            _client.DefaultRequestHeaders.Clear();

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            Dictionary<string, double>? returnedAnalytics1 = JsonSerializer.Deserialize<Dictionary<string, double>>(stringContent, _customJsonOptions);
            Assert.IsNotNull(returnedAnalytics1);
            CollectionAssert.AllItemsAreNotNull(returnedAnalytics1);
            Assert.AreEqual(returnedAnalytics1.Count, statesCount);

            // 2 - TimeFuncOfOneMetric, argument - metric id

            // Arrange
            analyticTypeId = 2;
            queryParams = $"?analyticTypeId={analyticTypeId}";
            argumentId = 1; // stress
            _client.DefaultRequestHeaders.Add("argumentId", argumentId.ToString());

            // Act
            response = await _client.GetAsync(_baseUrl + route + queryParams);
            stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            _client.DefaultRequestHeaders.Clear();

            Dictionary<string, int>? returnedAnalytics2 = JsonSerializer.Deserialize<Dictionary<string, int>>(stringContent, _customJsonOptions);
            Assert.IsNotNull(returnedAnalytics2);
            CollectionAssert.AllItemsAreNotNull(returnedAnalytics2);
            Assert.AreEqual(returnedAnalytics2.Count, statesCount);
            for (int i = 0; i < statesCount; ++i)
            {
                if (returnedAnalytics2.TryGetValue(dailyStates[i].NoteDate.ToString("yyyy-MM-dd"), out int metricAssessment))
                {
                    int expectedAssessment = dailyStates[i].MetricInDailyStates.First(dailyMetric => dailyMetric.MetricId == argumentId)
                        .Assessment;
                    Assert.AreEqual(metricAssessment, expectedAssessment);
                }
                else Assert.Fail();
            }
        }

        public async Task GetUserAnalyticData_PassCorrectDataWithFullPeriod_ReturnsCorrectAnalytics(DateOnly beginningDate, DateOnly endDate)
        {
            // 3 - MoodBasesSum

            // Arrange
            int analyticTypeId = 3;
            string route = $"{user.Id}/";
            string queryParams = $"?analyticTypeId={analyticTypeId}";
            _client.DefaultRequestHeaders.Add("beginningDateS", beginningDate.ToString("yyyy-MM-dd"));
            _client.DefaultRequestHeaders.Add("endDateS", endDate.ToString("yyyy-MM-dd"));

            // Act
            HttpResponseMessage response = await _client.GetAsync(_baseUrl + route + queryParams);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            _client.DefaultRequestHeaders.Clear();

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            Dictionary<int, int>? returnedAnalytics3 = JsonSerializer.Deserialize<Dictionary<int, int>>(stringContent, _customJsonOptions);
            Assert.IsNotNull(returnedAnalytics3);
            CollectionAssert.AllItemsAreNotNull(returnedAnalytics3);
            Assert.AreEqual(endDate.DayNumber - beginningDate.DayNumber + 1, returnedAnalytics3.Select(analytic => analytic.Value).Sum());
        }

        public async Task GetUserAnalyticData_PassCorrectDataWithOneDate_ReturnsCorrectAnalytics(DateOnly beginningDate)
        {
            // 4 - MoodsInMoodBasesSum, argument - mood basis id

            // Arrange
            int analyticTypeId = 4;
            string route = $"{user.Id}/";
            string queryParams = $"?analyticTypeId={analyticTypeId}";
            _client.DefaultRequestHeaders.Add("beginningDateS", beginningDate.ToString("yyyy-MM-dd"));
            int argumentId = 1; // sadness
            _client.DefaultRequestHeaders.Add("argumentId", argumentId.ToString());

            // Act
            HttpResponseMessage response = await _client.GetAsync(_baseUrl + route + queryParams);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            _client.DefaultRequestHeaders.Clear();

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            Dictionary<int, int>? returnedAnalytics4 = JsonSerializer.Deserialize<Dictionary<int, int>>(stringContent, _customJsonOptions);
            Assert.IsNotNull(returnedAnalytics4);
            CollectionAssert.AllItemsAreNotNull(returnedAnalytics4);
        }

        public async Task GetUserRecommendations_PassCorrectUserId_ReturnsMatchingRecommendations()
        {
            // Arrange
            string route = $"recommendations/{user.Id}/";

            // Act
            HttpResponseMessage response = await _client.GetAsync(_baseUrl + route);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            List<Article>? recommendations = JsonSerializer.Deserialize<List<Article>>(stringContent, _customJsonOptions);
            Assert.IsNotNull(recommendations);
            CollectionAssert.AllItemsAreNotNull(recommendations);
        }

        [TestMethod]
        public async Task FunctionIntegration()
        {
            // Arrange
            user = await _context.Users.FirstAsync(user => user.Mail == "test2@mail.ru");
            dsMethods = new DailyStatesControllerTests();

            for (int i = 0; i < statesCount; ++i)
                dailyStates.Add(await dsMethods.CreateNewUserDailyState_PassFullObject_ReturnsOk(user.Id, startDate.AddDays(i)));

            // Act Get analytics 0-2
            await GetUserAnalyticData_PassCorrectDataWithoutPeriod_ReturnsCorrectAnalytics();

            // Act Get analytic 3
            await GetUserAnalyticData_PassCorrectDataWithFullPeriod_ReturnsCorrectAnalytics(startDate.AddDays(1), startDate.AddDays(5));

            // Act Get analytic 4
            await GetUserAnalyticData_PassCorrectDataWithOneDate_ReturnsCorrectAnalytics(startDate.AddDays(3));

            // Act Get recommendations
            await GetUserRecommendations_PassCorrectUserId_ReturnsMatchingRecommendations();

            // Cleanup
            for (int i = 0; i < statesCount; ++i)
                await dsMethods.DeleteUserDailyState_PassExistingStateId_ReturnsOk(dailyStates[i].Id);

        }
    }

}
