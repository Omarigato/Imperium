using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories.Favorite
{
    public class FavoriteRepository : BaseRepository<Favorite>, IFavoriteRepository
    {
        public FavoriteRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Favorites")
        {
        }

        public async Task<IEnumerable<Favorite>> GetByUserIdAsync(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Favorites` 
                WHERE `UserId` = @UserId 
                ORDER BY `CreatedAt` DESC";

            return await connection.QueryAsync<Favorite>(sql, new { UserId = userId });
        }

        public async Task<Favorite?> GetByUserAndProductAsync(Guid userId, Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Favorites` 
                WHERE `UserId` = @UserId AND `ProductId` = @ProductId";

            return await connection.QuerySingleOrDefaultAsync<Favorite>(sql, new { UserId = userId, ProductId = productId });
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT COUNT(*) FROM `Favorites` 
                WHERE `UserId` = @UserId AND `ProductId` = @ProductId";

            var count = await connection.QuerySingleAsync<int>(sql, new { UserId = userId, ProductId = productId });
            return count > 0;
        }

        public async Task<bool> RemoveByUserAndProductAsync(Guid userId, Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                DELETE FROM `Favorites` 
                WHERE `UserId` = @UserId AND `ProductId` = @ProductId";

            var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId, ProductId = productId });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Favorite>> GetByUserIdWithDetailsAsync(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT f.*, 
                       p.`NameRu` as ProductNameRu, p.`NameKz` as ProductNameKz,
                       p.`Price` as ProductPrice, p.`Code` as ProductCode,
                       cat.`NameRu` as CategoryNameRu, cat.`NameKz` as CategoryNameKz
                FROM `Favorites` f
                INNER JOIN `Products` p ON f.`ProductId` = p.`Id`
                LEFT JOIN `Dictionaries` cat ON p.`CategoryId` = cat.`Id`
                WHERE f.`UserId` = @UserId
                ORDER BY f.`CreatedAt` DESC";

            return await connection.QueryAsync<Favorite>(sql, new { UserId = userId });
        }
    }
}
