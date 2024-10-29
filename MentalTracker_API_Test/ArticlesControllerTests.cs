using MentalTracker_API.Models;
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
    public class ArticlesControllerTests : ApiTests
    {
        public ArticlesControllerTests() : base() { _baseUrl += "Articles/"; }

        public async Task<List<Article>> GetArticles_WithoutArguments_ReturnsAllArticles()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync(_baseUrl);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            List<Article> articles = JsonSerializer.Deserialize<List<Article>>(stringContent, _customJsonOptions);
            Assert.IsNotNull(articles);
            Assert.IsTrue(articles.Any());
            CollectionAssert.AllItemsAreNotNull(articles);

            return articles;
        }

        public async Task<(int, List<Article>)> GetArticles_Pagination_ReturnsAllMatchingArticles(int pageNumber, int pageSize )
        {
            // Arrange
            string queryParams = $"?pageSize={pageSize}";

            // Act
            HttpResponseMessage response = await _client.GetAsync(_baseUrl + $"page/{pageNumber}/" + queryParams);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            List<Article> articles = JsonSerializer.Deserialize<List<Article>>(stringContent, _customJsonOptions);
            Assert.IsNotNull(articles);
            Assert.IsTrue(articles.Count <= pageSize);
            CollectionAssert.AllItemsAreNotNull(articles);

            string totalArticlesCount = response.Headers.GetValues("X-Total-Count").First();
            return (int.Parse(totalArticlesCount), articles);
        }

        public async Task<List<Article>> GetArticles_Sorting_ReturnsSortedArticles(string orderBy, bool ascending)
        {
            // Arrange
            string queryParams = $"?orderBy={orderBy}&ascending={ascending}";

            // Act
            HttpResponseMessage response = await _client.GetAsync(_baseUrl + $"page/{1}/" + queryParams);
            string stringContent = await response.Content.ReadAsStringAsync();

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(response.StatusCode + ": " + stringContent);
                Assert.Fail();
            }

            List<Article> articles = JsonSerializer.Deserialize<List<Article>>(stringContent, _customJsonOptions);
            Assert.IsNotNull(articles);
            CollectionAssert.AllItemsAreNotNull(articles);
            
            return articles;
        }

        [TestMethod]
        public async Task FunctionIntegration()
        {
            // Act Get all articles
            List<Article> allArticles = await GetArticles_WithoutArguments_ReturnsAllArticles();

            // Arrange Get 
            int pageNumber = 1, pageSize=2;
            // Act page of articles
            (int articleCount1, List<Article> articles1) = await GetArticles_Pagination_ReturnsAllMatchingArticles(pageNumber, pageSize);
            (int articleCount2, List<Article> articles2) = await GetArticles_Pagination_ReturnsAllMatchingArticles(++pageNumber, pageSize);

            // Assert Get all articles/ page of article
            Assert.AreEqual(allArticles.Count, articleCount1);
            Assert.AreEqual(articleCount1, articleCount2);
            for(int i = 0; i < pageSize; ++i) 
                Assert.AreNotEqual(articles1[i], articles2[i]);

            // Arrange Get with sorting
            string orderBy = "posting_date";
            bool asc = true;
            // Act Get with sorting
            articles1 = await GetArticles_Sorting_ReturnsSortedArticles(orderBy, asc);
            articles2 = articles1.OrderBy(article => article.PostingDate).ToList(); // copy object references, but list is different
            // Assert Get with sorting
            CollectionAssert.AreEqual(articles1, articles2 );

            // Arrange Get with sorting
            orderBy = "author";
            asc = false;
            // Act Get with sorting
            articles1 = await GetArticles_Sorting_ReturnsSortedArticles(orderBy, asc);
            articles2 = articles1.OrderByDescending(article => article.Author).ToList(); // copy object references, but list is different
            // Arrange Get with sorting
            CollectionAssert.AreEqual(articles1, articles2 );
        }
    }

}
