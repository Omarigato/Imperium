using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public interface IReviewRepository : IBaseRepository<Review>
    {
        Task<IEnumerable<Review>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<Review>> GetByUserIdAsync(Guid userId);
        Task<Review?> GetByUserAndProductAsync(Guid userId, Guid productId);
        Task<double> GetAverageRatingByProductIdAsync(Guid productId);
        Task<IEnumerable<Review>> GetVerifiedReviewsAsync();
        Task<IEnumerable<Review>> GetByProductIdWithDetailsAsync(Guid productId);
    }
}