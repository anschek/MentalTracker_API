using System;
using System.Collections.Generic;

namespace MentalTracker_API.Models
{
    public partial class ArticlesTag
    {
        public int? ArticleId { get; set; }
        public int? TagId { get; set; }

        public virtual Article? Article { get; set; }
        public virtual ArticleTag? Tag { get; set; }
    }
}
