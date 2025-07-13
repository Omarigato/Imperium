using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.Review
{
    public class ClientReviewStatsDto
    {
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
    }
}
