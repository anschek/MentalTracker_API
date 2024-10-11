using System;
using System.Collections.Generic;

namespace MentalTracker_API.Models
{
    public partial class Article
    {
        public Article()
        {
            Tags = new HashSet<ArticleTag>();
        }

        public int Id { get; set; }
        public int TypeId { get; set; }
        public string? Author { get; set; }
        public DateOnly? PostingDate { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;

        public virtual ArticleType Type { get; set; } = null!;

        public virtual ICollection<ArticleTag> Tags { get; set; }
    }
}
