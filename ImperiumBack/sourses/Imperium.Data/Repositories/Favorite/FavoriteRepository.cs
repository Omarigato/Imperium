using Dapper;
using System;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Favorite
{
    public class FavoriteRepository : BaseRepository<Core.Models.Favorite>, IFavoriteRepository
    {
        public FavoriteRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Favorites")
        {
        }

        public async Task Insert(Core.Models.Favorite favorite)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `Favorites` (
                    `Id`, `ClientId`, `ProductId`, `CreateDate`)
                VALUES (
                    @Id, @ClientId, @ProductId, @CreateDate)", favorite);
        }

        public async Task Update(Core.Models.Favorite favorite)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `Favorites`
                SET 
                    `ClientId` = @ClientId,
                    `ProductId` = @ProductId
                WHERE `Id` = @Id", favorite);
        }

        public async Task<IEnumerable<Core.Models.Favorite>> GetByClientIdAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Favorites` 
                WHERE `ClientId` = @ClientId 
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Favorite>(sql, new { ClientId = clientId });
        }

        public async Task<Core.Models.Favorite?> GetByClientAndProductAsync(Guid clientId, Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Favorites` 
                WHERE `ClientId` = @ClientId AND `ProductId` = @ProductId";

            return await connection.QuerySingleOrDefaultAsync<Core.Models.Favorite>(sql, new { ClientId = clientId, ProductId = productId });
        }

        public async Task<bool> ExistsAsync(Guid clientId, Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT COUNT(*) FROM `Favorites` 
                WHERE `ClientId` = @ClientId AND `ProductId` = @ProductId";

            var count = await connection.QuerySingleAsync<int>(sql, new { ClientId = clientId, ProductId = productId });
            return count > 0;
        }

        public async Task<bool> RemoveByClientAndProductAsync(Guid clientId, Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                DELETE FROM `Favorites` 
                WHERE `ClientId` = @ClientId AND `ProductId` = @ProductId";

            var rowsAffected = await connection.ExecuteAsync(sql, new { ClientId = clientId, ProductId = productId });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Core.Models.Favorite>> GetByClientIdWithDetailsAsync(Guid clientId)
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
                WHERE f.`ClientId` = @ClientId
                ORDER BY f.`CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Favorite>(sql, new { ClientId = clientId });
        }
    }
}