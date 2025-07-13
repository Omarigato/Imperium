using Dapper;
using System;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.ProductSize
{
    public class ProductSizeRepository : BaseRepository<Core.Models.ProductSize>, IProductSizeRepository
    {
        public ProductSizeRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "ProductSizes")
        {
        }

        public async Task Insert(Core.Models.ProductSize productSize)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `ProductSizes` (
                    `Id`, `ProductId`, `SizeId`, `IsAvailable`, `AuthorId`, `CreateDate`)
                VALUES (
                    @Id, @ProductId, @SizeId, @IsAvailable, @AuthorId, @CreateDate)", productSize);
        }

        public async Task Update(Core.Models.ProductSize productSize)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `ProductSizes`
                SET 
                    `ProductId` = @ProductId,
                    `SizeId` = @SizeId,
                    `IsAvailable` = @IsAvailable,
                    `DeleteDate` = @DeleteDate
                WHERE `Id` = @Id", productSize);
        }

        public async Task<IEnumerable<Core.Models.ProductSize>> GetByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductSizes` 
                WHERE `ProductId` = @ProductId AND `DeleteDate` IS NULL";

            return await connection.QueryAsync<Core.Models.ProductSize>(sql, new { ProductId = productId });
        }

        public async Task<IEnumerable<Core.Models.ProductSize>> GetBySizeIdAsync(Guid sizeId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductSizes` 
                WHERE `SizeId` = @SizeId AND `DeleteDate` IS NULL";

            return await connection.QueryAsync<Core.Models.ProductSize>(sql, new { SizeId = sizeId });
        }

        public async Task<bool> DeleteByProductAndSizeAsync(Guid productId, Guid sizeId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                UPDATE `ProductSizes` 
                SET `DeleteDate` = @DeleteDate
                WHERE `ProductId` = @ProductId AND `SizeId` = @SizeId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                ProductId = productId,
                SizeId = sizeId,
                DeleteDate = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(Guid productId, Guid sizeId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT COUNT(*) FROM `ProductSizes` 
                WHERE `ProductId` = @ProductId AND `SizeId` = @SizeId AND `DeleteDate` IS NULL";

            var count = await connection.QuerySingleAsync<int>(sql, new { ProductId = productId, SizeId = sizeId });
            return count > 0;
        }

        public async Task<IEnumerable<Core.Models.ProductSize>> GetAvailableByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductSizes` 
                WHERE `ProductId` = @ProductId AND `IsAvailable` = true AND `DeleteDate` IS NULL";

            return await connection.QueryAsync<Core.Models.ProductSize>(sql, new { ProductId = productId });
        }

        public override async Task<IEnumerable<Core.Models.ProductSize>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `ProductSizes` WHERE `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.ProductSize>(sql);
        }
    }
}