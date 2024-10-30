using MentalTracker_API.Helpers;
using MentalTracker_API.Models;
using MentalTracker_API.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MentalTracker_API_Test
{
    [TestClass]
    public class DailyStatesControllerTests : ApiTests
    {
        public DailyStatesControllerTests() : base() { _baseUrl += "Dailystates/"; }

        public async Task<DailyState> CreateNewUserDailyState_PassFullObject_ReturnsOk(Guid userId, DateOnly noteDate)
        {
            // Arrange
            Random random = new Random();
            List<MetricInDailyStateDto> dailyMetrics = new List<MetricInDailyStateDto>();
            for (int i = 1; i <= await _context.Metrics.CountAsync(); ++i)
                dailyMetrics.Add(new MetricInDailyStateDto { MetricId = i, Assessment = random.Next(1, 6) });
            DailyStateDto dailyState = new DailyStateDto
            {
                UserId = userId,
                NoteDate = noteDate,
                GeneralMoodAssessment = random.Next(1, 6),
                MoodId = random.Next(1, await _context.Moods.CountAsync() + 1),
                Note = "note" + random.Next(),
                MetricInDailyStates = dailyMetrics
            };

            string jsonContent = JsonSerializer.Serialize(dailyState, _customJsonOptions);
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync(_baseUrl, content);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            return await _context.DailyStates.Include(state => state.MetricInDailyStates).FirstAsync(state => state.UserId == userId && state.NoteDate == noteDate);
        }

        public async Task<DailyStateDto> GetUserDailyState_PassCorrectUserAndDate_ReturnsExistingState(Guid userId, DateOnly noteDate)
        {
            // Arrange
            string route = $"{userId}/one-state/";
            _client.DefaultRequestHeaders.Add("dateOfStateS", noteDate.ToString("yyyy-MM-dd"));

            // Act
            HttpResponseMessage response = await _client.GetAsync(_baseUrl + route);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            _client.DefaultRequestHeaders.Clear();

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            DailyStateDto? dailyState = JsonSerializer.Deserialize<DailyStateDto>(stringContent, _customJsonOptions);
            Assert.IsNotNull(dailyState);
            Assert.IsNotNull(dailyState.MetricInDailyStates);
            CollectionAssert.AllItemsAreNotNull(dailyState.MetricInDailyStates);
            Assert.AreEqual(dailyState.UserId, userId);
            Assert.AreEqual(dailyState.NoteDate, noteDate);

            return dailyState;
        }
        public async Task<List<DailyStateDto>> GetUserDailyStates_PassCorrectUserIdAndPeriod_ReturnsMatchingStates(Guid userId, DateOnly beginningDate, DateOnly endDate)
        {
            // Arrange
            string route = $"{userId}/";
            _client.DefaultRequestHeaders.Add("beginningDateS", beginningDate.ToString("yyyy-MM-dd"));
            _client.DefaultRequestHeaders.Add("endDateS", endDate.ToString("yyyy-MM-dd"));

            // Act
            HttpResponseMessage response = await _client.GetAsync(_baseUrl + route);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            _client.DefaultRequestHeaders.Clear();

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            List<DailyStateDto>? dailyStates = JsonSerializer.Deserialize<List<DailyStateDto>>(stringContent, _customJsonOptions);

            Assert.IsNotNull(dailyStates);
            CollectionAssert.AllItemsAreNotNull(dailyStates);

            foreach (var dailyState in dailyStates)
            {
                Assert.IsTrue(dailyState.NoteDate >= beginningDate);
                Assert.IsTrue(dailyState.NoteDate <= endDate);
                Assert.IsNotNull(dailyState.MetricInDailyStates);
                CollectionAssert.AllItemsAreNotNull(dailyState.MetricInDailyStates);
            }

            return dailyStates;
        }

        public async Task<List<DailyStateDto>> GetUserDailyStates_PassCorrectUserIdAndOneDate_ReturnsMatchingStates(Guid userId, DateOnly beginningDate)
        {
            // Arrange
            string route = $"{userId}/";
            _client.DefaultRequestHeaders.Add("beginningDateS", beginningDate.ToString("yyyy-MM-dd"));

            // Act
            HttpResponseMessage response = await _client.GetAsync(_baseUrl + route);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            _client.DefaultRequestHeaders.Clear();

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            List<DailyStateDto>? dailyStates = JsonSerializer.Deserialize<List<DailyStateDto>>(stringContent, _customJsonOptions);

            Assert.IsNotNull(dailyStates);
            CollectionAssert.AllItemsAreNotNull(dailyStates);

            foreach (var dailyState in dailyStates)
            {
                Assert.IsTrue(dailyState.NoteDate >= beginningDate);
                Assert.IsNotNull(dailyState.MetricInDailyStates);
                CollectionAssert.AllItemsAreNotNull(dailyState.MetricInDailyStates);
            }

            return dailyStates;
        }

        public async Task<List<DailyStateDto>> GetUserDailyStates_PassCorrectUserIdWithoutPeriod_ReturnsAllStates(Guid userId)
        {
            // Arrange
            string route = $"{userId}/";

            // Act
            HttpResponseMessage response = await _client.GetAsync(_baseUrl + route);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            List<DailyStateDto>? dailyStates = JsonSerializer.Deserialize<List<DailyStateDto>>(stringContent, _customJsonOptions);

            Assert.IsNotNull(dailyStates);
            CollectionAssert.AllItemsAreNotNull(dailyStates);

            foreach (var dailyState in dailyStates)
            {
                Assert.IsNotNull(dailyState.MetricInDailyStates);
                CollectionAssert.AllItemsAreNotNull(dailyState.MetricInDailyStates);
            }

            return dailyStates;
        }

        public async Task<List<ShortDailyState>> GetUserDailyStatesShort_PassCorrectUserIdAndPeriod_ReturnsMatchingStates(Guid userId, DateOnly beginningDate, DateOnly endDate)
        {
            // Arrange
            string route = $"{userId}/short/";
            _client.DefaultRequestHeaders.Add("beginningDateS", beginningDate.ToString("yyyy-MM-dd"));
            _client.DefaultRequestHeaders.Add("endDateS", endDate.ToString("yyyy-MM-dd"));

            // Act
            HttpResponseMessage response = await _client.GetAsync(_baseUrl + route);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            _client.DefaultRequestHeaders.Clear();

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            List<ShortDailyState>? dailyStates = JsonSerializer.Deserialize<List<ShortDailyState>>(stringContent, _customJsonOptions);

            Assert.IsNotNull(dailyStates);
            CollectionAssert.AllItemsAreNotNull(dailyStates);

            foreach (var dailyState in dailyStates)
            {
                Assert.IsTrue(dailyState.NoteDate >= beginningDate);
                Assert.IsTrue(dailyState.NoteDate <= endDate);
            }

            return dailyStates;
        }

        public async Task UpdateUserDailyState_PassFullObject_ReturnsOk(DailyStateDto newDailyState)
        {
            // Arrange
            string jsonContent = JsonSerializer.Serialize(newDailyState, _customJsonOptions);
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PutAsync(_baseUrl, content);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            _context = new MentalTrackerContext();
            DailyState updatedDailyState = await _context.DailyStates.Include(state => state.MetricInDailyStates)
                .FirstAsync(state => state.Id == newDailyState.Id);
            Assert.AreEqual(updatedDailyState.GeneralMoodAssessment, newDailyState.GeneralMoodAssessment);
            Assert.AreEqual(updatedDailyState.Note, newDailyState.Note);
            Assert.AreEqual(updatedDailyState.MoodId, newDailyState.MoodId);

            List<MetricInDailyStateDto> newMetric = newDailyState.MetricInDailyStates.ToList();
            List<MetricInDailyState> updatedMetric = updatedDailyState.MetricInDailyStates.ToList();
            Assert.AreEqual(updatedMetric.Count, newMetric.Count );
            for (int i= 0; i < updatedMetric.Count; ++i)
            {
                Assert.AreEqual(updatedMetric[i].MetricId,newMetric[i].MetricId);
                Assert.AreEqual(updatedMetric[i].Assessment,newMetric[i].Assessment);
            }
        }

        public async Task DeleteUserDailyState_PassExistingStateId_ReturnsOk(int stateId)
        {
            //Arrange
            string route = $"{stateId}/";
            // Act
            HttpResponseMessage response = await _client.DeleteAsync(_baseUrl + route);
            string stringContent = await response.Content.ReadAsStringAsync();
            // Assert
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            _context = new MentalTrackerContext();
            DailyState? dailyState = await _context.DailyStates.FindAsync(stateId);
            Assert.IsNull(dailyState);
        }

        [TestMethod]
        public async Task FunctionIntegration()
        {
            // Arrange Post state
            User user = await _context.Users.FirstAsync(user => user.Mail == "test@mail.ru");
            List<DailyState> dailyStates = new List<DailyState>();
            int statesCount = 4;
            // Act Post state
            for (int i = 0; i < 4; ++i)
                dailyStates.Add(await CreateNewUserDailyState_PassFullObject_ReturnsOk(user.Id, new DateOnly(2024, 10, 20 + i)));

            for (int i = 0; i < 4; ++i)
            {
                // Act Get state
                DailyStateDto dailyState = await GetUserDailyState_PassCorrectUserAndDate_ReturnsExistingState(
                    dailyStates[i].UserId, dailyStates[i].NoteDate);
                // Assert Post/Get state
                Assert.AreEqual(dailyState.MoodId, dailyStates[i].MoodId);
                Assert.AreEqual(dailyState.Note, dailyStates[i].Note);
            }

            // Arrange Get states in period
            DateOnly beginning = new DateOnly(2024, 10, 21), end = new DateOnly(2024, 10, 22);
            // Act Get states in period
            List<DailyStateDto> dailyStatesInPeriod = await GetUserDailyStates_PassCorrectUserIdAndPeriod_ReturnsMatchingStates(
                user.Id, beginning, end);
            // Assert Post state/Get states in period
            Assert.AreEqual(dailyStatesInPeriod.Count, 2);
            for (int i = 0; i < 2; ++i)
                Assert.AreEqual(dailyStatesInPeriod[i].Id, dailyStates[i + 1].Id);

            // Act Get states from beginning
            dailyStatesInPeriod = await GetUserDailyStates_PassCorrectUserIdAndOneDate_ReturnsMatchingStates(
                user.Id, beginning);
            // Assert Post state/Get states from beginning
            Assert.AreEqual(dailyStatesInPeriod.Count, 3);
            for (int i = 0; i < 3; ++i)
                Assert.AreEqual(dailyStatesInPeriod[i].Id, dailyStates[i + 1].Id);

            // Act Get all states
            dailyStatesInPeriod = await GetUserDailyStates_PassCorrectUserIdWithoutPeriod_ReturnsAllStates(user.Id);
            // Assert Post state/Get all states
            Assert.AreEqual(dailyStatesInPeriod.Count, dailyStates.Count);
            for (int i = 0; i < 4; ++i)
                Assert.AreEqual(dailyStatesInPeriod[i].Id, dailyStates[i].Id);

            // Act Get short states
            List<ShortDailyState> shortDailyStates = await GetUserDailyStatesShort_PassCorrectUserIdAndPeriod_ReturnsMatchingStates(
                 user.Id, beginning, end);
            // Assert Post state/Get short states
            for (int i = 0; i < 2; ++i)
            {
                Assert.AreEqual(shortDailyStates[i].FullDailyStateId, dailyStates[i + 1].Id);
                Assert.AreEqual(shortDailyStates[i].Note, dailyStates[i + 1].Note);
                Assert.AreEqual(shortDailyStates[i].NoteDate, dailyStates[i + 1].NoteDate);
                Assert.AreEqual(shortDailyStates[i].Mood.Id, dailyStates[i + 1].MoodId);
            }

            // Arrange Put state
            int newAssessment = 1, newMoodId = 1;
            string newNote = "new note";
            List<MetricInDailyStateDto> newMetrics = new List<MetricInDailyStateDto>
            {
                new MetricInDailyStateDto{ MetricId = 1, Assessment=1},
                new MetricInDailyStateDto{ MetricId = 2, Assessment=1}
            };

            dailyStatesInPeriod[0].GeneralMoodAssessment = newAssessment;
            dailyStatesInPeriod[0].MoodId = newMoodId;
            dailyStatesInPeriod[0].Note = newNote;
            dailyStatesInPeriod[0].MetricInDailyStates = newMetrics;

            // Act Put state
            await UpdateUserDailyState_PassFullObject_ReturnsOk(dailyStatesInPeriod[0]);

            // Act Delete state
            for (int i = 0; i < 4; ++i)
                await DeleteUserDailyState_PassExistingStateId_ReturnsOk(dailyStates[i].Id);
        }
    }
}
