using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Review
{
    public interface IReviewRepository : IBaseRepository<Core.Models.Review>
    {
        Task Insert(Core.Models.Review review);
        Task Update(Core.Models.Review review);
        Task<IEnumerable<Core.Models.Review>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<Core.Models.Review>> GetByClientIdAsync(Guid clientId);
        Task<Core.Models.Review?> GetByClientAndProductAsync(Guid clientId, Guid productId);
        Task<double> GetAverageRatingByProductIdAsync(Guid productId);
        Task<IEnumerable<Core.Models.Review>> GetByProductIdWithDetailsAsync(Guid productId);
        Task<IEnumerable<Core.Models.Review>> GetByRatingAsync(int rating);
    }
}