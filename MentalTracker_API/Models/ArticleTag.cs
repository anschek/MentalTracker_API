using System;
using System.Collections.Generic;

namespace MentalTracker_API.Models
{
    public partial class ArticleTag
    {
        public ArticleTag()
        {
            Articles = new HashSet<Article>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Article> Articles { get; set; }
    }
}
