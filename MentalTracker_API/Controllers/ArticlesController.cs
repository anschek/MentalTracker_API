﻿using MentalTracker_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MentalTracker_API.Controllers
{
    /// <summary>
    /// описание контроллера
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1")]
    public class ArticlesController : Controller
    {
        private readonly MentalTrackerContext _context;
        public ArticlesController(MentalTrackerContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Getting all article tags
        /// </summary>
        [HttpGet("tags")]
        public async Task<ActionResult<ICollection<ArticleTag>>> GetArticleTags()
        {
            var tags = await _context.ArticleTags.ToListAsync();
            if (tags == null || tags.Count == 0) return NotFound();
            return tags;
        }
        /// <summary>
        /// Getting all article types
        /// </summary>
        [HttpGet("types")]
        public async Task<ActionResult<ICollection<ArticleType>>> GetArticleTypes()
        {
            var types = await _context.ArticleTypes.ToListAsync();
            if (types == null || types.Count == 0) return NotFound();
            return types;
        }
        /// <summary>
        /// Getting all articles
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ICollection<Article>>> GetArticles()
        {
            var articles = await _context.Articles.Include(article => article.Type).Include(article => article.Tags).ToListAsync();
            if (articles == null || articles.Count == 0) return NotFound();
            return articles;
        }
        /// <summary>
        /// Getting a fixed length page of articles
        /// </summary>
        /// <param name="pageNumber"> Index of one of pages into which articles were split (already sorted) </param>        
        /// <param name="pageSize"> Amount of articles in one page </param>        
        /// <param name="orderBy"> Attribute by which articles are sorted: "author", "posting_date" or "title".
        /// If null is passed to method, default sorting is used</param>
        /// <param name="ascending"> If you need to sort in ascending order - true, descending - false </param>
        /// <returns> Body of response contains a sorted list of articles of length no longer than "pageSize", header contains
        /// total count of articles in database</returns>
        [HttpGet("page/{pageNumber}")]
        public async Task<ActionResult<ICollection<Article>>> GetArticles(
            [FromRoute] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool ascending = true
        )
        {
            if (pageNumber < 1 || pageSize < 1) return BadRequest("attributes \"page\" and \"page size\" must be positive integers");

            int totalArticleCount = await _context.Articles.CountAsync();
            Response.Headers.Add("X-Total-Count", totalArticleCount.ToString());

            var articles = await _context.Articles.Include(article => article.Type).Include(article => article.Tags).ToListAsync();

            if (!string.IsNullOrEmpty(orderBy))
            {
                if (ascending) _ = orderBy.ToLower() switch
                {
                    "author" => articles.OrderBy(article => article.Author),
                    "posting_date" => articles.OrderBy(article => article.PostingDate),
                    "title" => articles.OrderBy(article => article.Title),
                    _ => articles.OrderBy(article => article.Id) // default
                };

                else _ = orderBy.ToLower() switch
                {
                    "author" => articles.OrderByDescending(article => article.Author),
                    "posting_date" => articles.OrderByDescending(article => article.PostingDate),
                    "title" => articles.OrderByDescending(article => article.Title),
                    _ => articles.OrderByDescending(article => article.Id) // default
                };
            }

            articles = articles.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            if (articles == null || articles.Count == 0) return NotFound();

            return articles;
        }
        /// <summary>
        /// Getting all articles of same type
        /// </summary>
        [HttpGet("{articleTypeId}")]
        public async Task<ActionResult<ICollection<Article>>> GetArticlesByType([FromRoute] int articleTypeId)
        {
            var articleType = await _context.ArticleTypes.FirstOrDefaultAsync(type => type.Id == articleTypeId);
            if (articleType == null) return NotFound($"Article type with id={articleTypeId} not found");
            var articles = await _context.Articles.Include(article => article.Type).Include(article => article.Tags)
                .Where(articles => articles.Type == articleType).ToListAsync();
            if (articles == null || articles.Count == 0) return NotFound();
            return articles;
        }
        /// <summary>
        /// Adding a new article to db
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateNewArticle([FromBody] Article article)
        {
            article.Id = 0;

            var type = await _context.ArticleTypes.FirstOrDefaultAsync(type => type.Id == article.TypeId);
            if (type == null) return NotFound($"Article type with Id={article.TypeId} not found");
            article.Type = type;

            if (article.PostingDate == null) article.PostingDate = DateOnly.FromDateTime(DateTime.Now);
            else if (article.PostingDate > DateOnly.FromDateTime(DateTime.Now)) return BadRequest("Posting day must be no later than today");

            ICollection<ArticleTag> articleTags = new HashSet<ArticleTag>();
            foreach (var tag in article.Tags)
            {
                var existingTag = await _context.ArticleTags.FirstOrDefaultAsync(articleTag =>
                articleTag.Id == tag.Id && articleTag.Name == articleTag.Name);

                if (existingTag == null) return NotFound($"Article tag with Id=={tag.Id} and Name={tag.Name} not found");
                articleTags.Add(existingTag);
            }
            article.Tags = articleTags;

            await _context.Articles.AddAsync(article);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
