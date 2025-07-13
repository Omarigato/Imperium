using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.Review
{
    public class ReviewStatisticsDto
    {
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsThisMonth { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public List<ClientReviewStatsDto> MostActiveReviewers { get; set; } = new();
    }
}
