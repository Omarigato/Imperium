using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class ProductSizeRepository : BaseRepository<ProductSize>, IProductSizeRepository
    {
        public ProductSizeRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "ProductSizes")
        {
        }

        public async Task<IEnumerable<ProductSize>> GetByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductSizes` 
                WHERE `ProductId` = @ProductId";

            return await connection.QueryAsync<ProductSize>(sql, new { ProductId = productId });
        }

        public async Task<IEnumerable<ProductSize>> GetBySizeIdAsync(Guid sizeId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductSizes` 
                WHERE `SizeId` = @SizeId";

            return await connection.QueryAsync<ProductSize>(sql, new { SizeId = sizeId });
        }

        public async Task<bool> DeleteByProductAndSizeAsync(Guid productId, Guid sizeId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                DELETE FROM `ProductSizes` 
                WHERE `ProductId` = @ProductId AND `SizeId` = @SizeId";

            var rowsAffected = await connection.ExecuteAsync(sql, new { ProductId = productId, SizeId = sizeId });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(Guid productId, Guid sizeId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT COUNT(*) FROM `ProductSizes` 
                WHERE `ProductId` = @ProductId AND `SizeId` = @SizeId";

            var count = await connection.QuerySingleAsync<int>(sql, new { ProductId = productId, SizeId = sizeId });
            return count > 0;
        }

        public async Task<IEnumerable<ProductSize>> GetAvailableByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductSizes` 
                WHERE `ProductId` = @ProductId AND `IsAvailable` = true";

            return await connection.QueryAsync<ProductSize>(sql, new { ProductId = productId });
        }
    }
}
