using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MentalTracker_API.Models
{
    public partial class ArticleTag
    {
        public ArticleTag()
        {
            TagMetricMatches = new HashSet<TagMetricMatch>();
            Articles = new HashSet<Article>();
            Moods = new HashSet<Mood>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<TagMetricMatch> TagMetricMatches { get; set; }
        [JsonIgnore]
        public virtual ICollection<Article> Articles { get; set; }
        [JsonIgnore]
        public virtual ICollection<Mood> Moods { get; set; }
    }
}
