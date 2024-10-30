using System.Diagnostics;
using MentalTracker_API.Models;
using System.Text.Json;
using System.Text;

namespace MentalTracker_API_Test
{
    [TestClass]
    public class UsersControllerTests: ApiTests
    {
        public UsersControllerTests() : base() { _baseUrl += "Users/";  }

        public async Task<User> CreateNewUser_PassCorrectData_ReturnsNewUser(User user)
        {
            // Arrange
            string jsonContent = JsonSerializer.Serialize(user, _customJsonOptions);
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

            User returnedUser = JsonSerializer.Deserialize<User>(stringContent, _customJsonOptions);

            Assert.IsNotNull(returnedUser);
            Assert.AreEqual(user.Name, returnedUser.Name);
            Assert.AreEqual(user.Mail, returnedUser.Mail);
            Assert.AreNotEqual(user.Password, returnedUser.Password); //hash
            
            return returnedUser;
        }

        public async Task<User> GetUser_PassCorrectMailAndPassword_ReturnsExistingUser(string mail, string password)
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("mail", mail);
            _client.DefaultRequestHeaders.Add("password", password);

            // Act
            HttpResponseMessage response = await _client.GetAsync(_baseUrl);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            _client.DefaultRequestHeaders.Clear();

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            User user = JsonSerializer.Deserialize<User>(stringContent, _customJsonOptions);
            Assert.IsNotNull(user);

            return user;
        }

        public async Task UpdateUserData_PassCorrectData_ReturnsOk(User updatedUser)
        {
            // Arrange
            string jsonContent = JsonSerializer.Serialize(updatedUser, _customJsonOptions);
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PutAsync(_baseUrl + "change-personal-data", content);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }
        }

        public async Task UpdateUserPassword_PassCorrectMailAndOldPassword_ReturnsOk(Guid updatedUserId, string oldPassword, string newPassword)
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("updatedUserId", updatedUserId.ToString());
            _client.DefaultRequestHeaders.Add("oldPassword", oldPassword);

            string jsonContent = JsonSerializer.Serialize(newPassword, _customJsonOptions);
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PutAsync(_baseUrl + "change-password", content);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            _client.DefaultRequestHeaders.Clear();

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task FunctionIntegration()
        {
            // Arrange Post
            string mail = "user@example.ru", name = "Test", password = "Test";
            DateOnly birthDate = new DateOnly(2001, 1, 1);
            User mainUser = new User { Id = Guid.NewGuid(), Mail = mail, DateOfBirth = birthDate, Name = name, Password = password };
            // Act Post
            mainUser =   await CreateNewUser_PassCorrectData_ReturnsNewUser(mainUser);
            // Act Get
            User otherUser = await GetUser_PassCorrectMailAndPassword_ReturnsExistingUser(mail, password);
            // Assert Create/Get
            Assert.AreEqual(mainUser.Id, otherUser.Id);
            Assert.AreEqual(mainUser.Name, otherUser.Name);
            Assert.AreEqual(mainUser.Mail, otherUser.Mail);
            Assert.AreEqual(mainUser.DateOfBirth, otherUser.DateOfBirth);
            Assert.AreEqual(mainUser.Password, otherUser.Password);

            // Arrange Put
            mail = "mainUser@example.ru"; name = "NewTest"; birthDate = new DateOnly(2002, 5, 5);
            mainUser.Mail = mail;
            mainUser.Name = name;
            mainUser.DateOfBirth = birthDate;
            // Act Put
            await UpdateUserData_PassCorrectData_ReturnsOk(mainUser);
            // Assert Put
            Assert.AreEqual(mainUser.Mail, mail);
            Assert.AreEqual(mainUser.Name, name);
            Assert.AreEqual(mainUser.DateOfBirth, birthDate);

            // Arrange Put Password
            string newPassword = "NewTest";
            // Act Put Password
            await UpdateUserPassword_PassCorrectMailAndOldPassword_ReturnsOk(mainUser.Id, password, newPassword);
            otherUser = await GetUser_PassCorrectMailAndPassword_ReturnsExistingUser(mail, newPassword);
            // Assert Put password
            Assert.AreNotEqual(newPassword, otherUser.Password);//hash

            //Clenup
            mainUser = await _context.Users.FindAsync(mainUser.Id);
            _context.Users.Remove(mainUser);
            await _context.SaveChangesAsync();
        }
    }
}
