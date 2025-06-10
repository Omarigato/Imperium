using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        public ReviewRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Reviews")
        {
        }

        public async Task<IEnumerable<Review>> GetByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Reviews` 
                WHERE `ProductId` = @ProductId 
                ORDER BY `CreatedAt` DESC";

            return await connection.QueryAsync<Review>(sql, new { ProductId = productId });
        }

        public async Task<IEnumerable<Review>> GetByUserIdAsync(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Reviews` 
                WHERE `UserId` = @UserId 
                ORDER BY `CreatedAt` DESC";

            return await connection.QueryAsync<Review>(sql, new { UserId = userId });
        }

        public async Task<Review?> GetByUserAndProductAsync(Guid userId, Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Reviews` 
                WHERE `UserId` = @UserId AND `ProductId` = @ProductId";

            return await connection.QuerySingleOrDefaultAsync<Review>(sql, new { UserId = userId, ProductId = productId });
        }

        public async Task<double> GetAverageRatingByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT COALESCE(AVG(CAST(`Rating` AS DECIMAL(10,2))), 0) 
                FROM `Reviews` 
                WHERE `ProductId` = @ProductId";

            return await connection.QuerySingleAsync<double>(sql, new { ProductId = productId });
        }

        public async Task<IEnumerable<Review>> GetVerifiedReviewsAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Reviews` 
                WHERE `IsVerified` = true 
                ORDER BY `CreatedAt` DESC";

            return await connection.QueryAsync<Review>(sql);
        }

        public async Task<IEnumerable<Review>> GetByProductIdWithDetailsAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT r.*, 
                       u.`FullName` AS UserFullName
                FROM `Reviews` r
                LEFT JOIN `Users` u ON r.`UserId` = u.`Id`
                WHERE r.`ProductId` = @ProductId
                ORDER BY r.`CreatedAt` DESC";

            return await connection.QueryAsync<Review>(sql, new { ProductId = productId });
        }
    }
}
