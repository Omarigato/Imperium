using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class FileRepository : BaseRepository<File>, IFileRepository
    {
        public FileRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Files")
        {
        }

        public async Task<File?> GetByPublicIdAsync(string publicId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Files` 
                WHERE `PublicId` = @PublicId";

            return await connection.QuerySingleOrDefaultAsync<File>(sql, new { PublicId = publicId });
        }

        public async Task<IEnumerable<File>> GetByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT f.* 
                FROM `Files` f
                INNER JOIN `ProductFiles` pf ON f.`Id` = pf.`FileId`
                WHERE pf.`ProductId` = @ProductId
                ORDER BY pf.`IsAddition`, f.`CreatedAt`";

            return await connection.QueryAsync<File>(sql, new { ProductId = productId });
        }

        public async Task<bool> DeleteByPublicIdAsync(string publicId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                DELETE FROM `Files` 
                WHERE `PublicId` = @PublicId";

            var rowsAffected = await connection.ExecuteAsync(sql, new { PublicId = publicId });
            return rowsAffected > 0;
        }
    }
}
