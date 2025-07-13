using Dapper;
using System;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.ProductColor
{
    public class ProductColorRepository : BaseRepository<Core.Models.ProductColor>, IProductColorRepository
    {
        public ProductColorRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "ProductColors")
        {
        }

        public async Task Insert(Core.Models.ProductColor productColor)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `ProductColors` (
                    `Id`, `ProductId`, `ColorId`, `IsAvailable`, `AuthorId`, `CreateDate`)
                VALUES (
                    @Id, @ProductId, @ColorId, @IsAvailable, @AuthorId, @CreateDate)", productColor);
        }

        public async Task Update(Core.Models.ProductColor productColor)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `ProductColors`
                SET 
                    `ProductId` = @ProductId,
                    `ColorId` = @ColorId,
                    `IsAvailable` = @IsAvailable,
                    `DeleteDate` = @DeleteDate
                WHERE `Id` = @Id", productColor);
        }

        public async Task<IEnumerable<Core.Models.ProductColor>> GetByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductColors` 
                WHERE `ProductId` = @ProductId AND `DeleteDate` IS NULL";

            return await connection.QueryAsync<Core.Models.ProductColor>(sql, new { ProductId = productId });
        }

        public async Task<IEnumerable<Core.Models.ProductColor>> GetByColorIdAsync(Guid colorId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductColors` 
                WHERE `ColorId` = @ColorId AND `DeleteDate` IS NULL";

            return await connection.QueryAsync<Core.Models.ProductColor>(sql, new { ColorId = colorId });
        }

        public async Task<bool> DeleteByProductAndColorAsync(Guid productId, Guid colorId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                UPDATE `ProductColors` 
                SET `DeleteDate` = @DeleteDate
                WHERE `ProductId` = @ProductId AND `ColorId` = @ColorId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                ProductId = productId,
                ColorId = colorId,
                DeleteDate = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(Guid productId, Guid colorId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT COUNT(*) FROM `ProductColors` 
                WHERE `ProductId` = @ProductId AND `ColorId` = @ColorId AND `DeleteDate` IS NULL";

            var count = await connection.QuerySingleAsync<int>(sql, new { ProductId = productId, ColorId = colorId });
            return count > 0;
        }

        public async Task<IEnumerable<Core.Models.ProductColor>> GetAvailableByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductColors` 
                WHERE `ProductId` = @ProductId AND `IsAvailable` = 1 AND `DeleteDate` IS NULL";

            return await connection.QueryAsync<Core.Models.ProductColor>(sql, new { ProductId = productId });
        }

        public override async Task<IEnumerable<Core.Models.ProductColor>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `ProductColors` WHERE `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.ProductColor>(sql);
        }
    }
}
