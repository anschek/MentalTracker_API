using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        [MaxLength(200)]
        public string? Author { get; set; }
        public DateOnly? PostingDate { get; set; }
        [MaxLength(255)]
        public string Title { get; set; } = null!;
        [MaxLength(4000)]
        public string Content { get; set; } = null!;
        [JsonIgnore]
        public virtual ArticleType Type { get; set; } = null!;
        public virtual ICollection<ArticleTag> Tags { get; set; }
    }
}
