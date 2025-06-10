using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class ProductFileRepository : BaseRepository<ProductFile>, IProductFileRepository
    {
        public ProductFileRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "ProductFiles")
        {
        }

        public async Task<IEnumerable<ProductFile>> GetByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductFiles` 
                WHERE `ProductId` = @ProductId 
                ORDER BY `IsAddition`, `Id`";

            return await connection.QueryAsync<ProductFile>(sql, new { ProductId = productId });
        }

        public async Task<IEnumerable<ProductFile>> GetByFileIdAsync(Guid fileId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `ProductFiles` 
                WHERE `FileId` = @FileId";

            return await connection.QueryAsync<ProductFile>(sql, new { FileId = fileId });
        }

        public async Task<bool> DeleteByProductAndFileAsync(Guid productId, Guid fileId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                DELETE FROM `ProductFiles` 
                WHERE `ProductId` = @ProductId AND `FileId` = @FileId";

            var rowsAffected = await connection.ExecuteAsync(sql, new { ProductId = productId, FileId = fileId });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(Guid productId, Guid fileId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT COUNT(*) FROM `ProductFiles` 
                WHERE `ProductId` = @ProductId AND `FileId` = @FileId";

            var count = await connection.QuerySingleAsync<int>(sql, new { ProductId = productId, FileId = fileId });
            return count > 0;
        }
    }
}
