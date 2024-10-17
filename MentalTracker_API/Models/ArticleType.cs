using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MentalTracker_API.Models
{
    public partial class ArticleType
    {
        public ArticleType()
        {
            Articles = new HashSet<Article>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        [JsonIgnore]
        public virtual ICollection<Article> Articles { get; set; }
    }
}
