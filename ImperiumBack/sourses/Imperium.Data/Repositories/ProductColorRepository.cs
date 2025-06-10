using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class ProductColorRepository : BaseRepository<ProductColor>, IProductColorRepository
    {
        public ProductColorRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "ProductColors")
        {
        }

        public async Task<IEnumerable<ProductColor>> GetByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductColors` 
                WHERE `ProductId` = @ProductId";

            return await connection.QueryAsync<ProductColor>(sql, new { ProductId = productId });
        }

        public async Task<IEnumerable<ProductColor>> GetByColorIdAsync(Guid colorId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductColors` 
                WHERE `ColorId` = @ColorId";

            return await connection.QueryAsync<ProductColor>(sql, new { ColorId = colorId });
        }

        public async Task<bool> DeleteByProductAndColorAsync(Guid productId, Guid colorId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                DELETE FROM `ProductColors` 
                WHERE `ProductId` = @ProductId AND `ColorId` = @ColorId";

            var rowsAffected = await connection.ExecuteAsync(sql, new { ProductId = productId, ColorId = colorId });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(Guid productId, Guid colorId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT COUNT(*) FROM `ProductColors` 
                WHERE `ProductId` = @ProductId AND `ColorId` = @ColorId";

            var count = await connection.QuerySingleAsync<int>(sql, new { ProductId = productId, ColorId = colorId });
            return count > 0;
        }

        public async Task<IEnumerable<ProductColor>> GetAvailableByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductColors` 
                WHERE `ProductId` = @ProductId AND `IsAvailable` = 1";

            return await connection.QueryAsync<ProductColor>(sql, new { ProductId = productId });
        }
    }
}
