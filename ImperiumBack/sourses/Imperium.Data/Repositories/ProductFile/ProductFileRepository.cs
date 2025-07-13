using Dapper;
using System;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.ProductFile
{
    public class ProductFileRepository : BaseRepository<Core.Models.ProductFile>, IProductFileRepository
    {
        public ProductFileRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "ProductFiles")
        {
        }

        public async Task Insert(Core.Models.ProductFile productFile)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `ProductFiles` (
                    `Id`, `ProductId`, `FileId`, `IsAddition`, `AuthorId`, `CreateDate`)
                VALUES (
                    @Id, @ProductId, @FileId, @IsAddition, @AuthorId, @CreateDate)", productFile);
        }

        public async Task Update(Core.Models.ProductFile productFile)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `ProductFiles`
                SET 
                    `ProductId` = @ProductId,
                    `FileId` = @FileId,
                    `IsAddition` = @IsAddition,
                    `DeleteDate` = @DeleteDate
                WHERE `Id` = @Id", productFile);
        }

        public async Task<IEnumerable<Core.Models.ProductFile>> GetByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductFiles` 
                WHERE `ProductId` = @ProductId AND `DeleteDate` IS NULL
                ORDER BY `IsAddition`, `Id`";

            return await connection.QueryAsync<Core.Models.ProductFile>(sql, new { ProductId = productId });
        }

        public async Task<IEnumerable<Core.Models.ProductFile>> GetByFileIdAsync(Guid fileId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductFiles` 
                WHERE `FileId` = @FileId AND `DeleteDate` IS NULL";

            return await connection.QueryAsync<Core.Models.ProductFile>(sql, new { FileId = fileId });
        }

        public async Task<IEnumerable<Core.Models.ProductFile>> GetMainImagesByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductFiles` 
                WHERE `ProductId` = @ProductId AND `IsAddition` = false AND `DeleteDate` IS NULL
                ORDER BY `Id`";

            return await connection.QueryAsync<Core.Models.ProductFile>(sql, new { ProductId = productId });
        }

        public async Task<IEnumerable<Core.Models.ProductFile>> GetAdditionalImagesByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductFiles` 
                WHERE `ProductId` = @ProductId AND `IsAddition` = true AND `DeleteDate` IS NULL
                ORDER BY `Id`";

            return await connection.QueryAsync<Core.Models.ProductFile>(sql, new { ProductId = productId });
        }

        public async Task<bool> DeleteByProductAndFileAsync(Guid productId, Guid fileId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                UPDATE `ProductFiles` 
                SET `DeleteDate` = @DeleteDate
                WHERE `ProductId` = @ProductId AND `FileId` = @FileId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                ProductId = productId,
                FileId = fileId,
                DeleteDate = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(Guid productId, Guid fileId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT COUNT(*) FROM `ProductFiles` 
                WHERE `ProductId` = @ProductId AND `FileId` = @FileId AND `DeleteDate` IS NULL";

            var count = await connection.QuerySingleAsync<int>(sql, new { ProductId = productId, FileId = fileId });
            return count > 0;
        }

        public override async Task<IEnumerable<Core.Models.ProductFile>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `ProductFiles` WHERE `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.ProductFile>(sql);
        }
    }
}