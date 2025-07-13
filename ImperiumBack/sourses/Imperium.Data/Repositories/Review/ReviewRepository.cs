using Dapper;
using System;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Review
{
    public class ReviewRepository : BaseRepository<Core.Models.Review>, IReviewRepository
    {
        public ReviewRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Reviews")
        {
        }

        public async Task Insert(Core.Models.Review review)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `Reviews` (
                    `Id`, `ClientId`, `ProductId`, `Rating`, `Comment`, `CreateDate`)
                VALUES (
                    @Id, @ClientId, @ProductId, @Rating, @Comment, @CreateDate)", review);
        }

        public async Task Update(Core.Models.Review review)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `Reviews`
                SET 
                    `ClientId` = @ClientId,
                    `ProductId` = @ProductId,
                    `Rating` = @Rating,
                    `Comment` = @Comment
                WHERE `Id` = @Id", review);
        }

        public async Task<IEnumerable<Core.Models.Review>> GetByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Reviews` 
                WHERE `ProductId` = @ProductId 
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Review>(sql, new { ProductId = productId });
        }

        public async Task<IEnumerable<Core.Models.Review>> GetByClientIdAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Reviews` 
                WHERE `ClientId` = @ClientId 
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Review>(sql, new { ClientId = clientId });
        }

        public async Task<Core.Models.Review?> GetByClientAndProductAsync(Guid clientId, Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Reviews` 
                WHERE `ClientId` = @ClientId AND `ProductId` = @ProductId";

            return await connection.QuerySingleOrDefaultAsync<Core.Models.Review>(sql, new { ClientId = clientId, ProductId = productId });
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

        public async Task<IEnumerable<Core.Models.Review>> GetByRatingAsync(int rating)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Reviews` 
                WHERE `Rating` = @Rating 
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Review>(sql, new { Rating = rating });
        }

        public async Task<IEnumerable<Core.Models.Review>> GetByProductIdWithDetailsAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT r.*, 
                       c.`FullName` AS ClientFullName
                FROM `Reviews` r
                LEFT JOIN `Clients` c ON r.`ClientId` = c.`Id`
                WHERE r.`ProductId` = @ProductId
                ORDER BY r.`CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Review>(sql, new { ProductId = productId });
        }
    }
}