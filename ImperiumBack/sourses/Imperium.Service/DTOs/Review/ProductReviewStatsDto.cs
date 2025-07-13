using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.Review
{
    public class ProductReviewStatsDto
    {
        public Guid ProductId { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public DateTime? LatestReviewDate { get; set; }
    }
}
