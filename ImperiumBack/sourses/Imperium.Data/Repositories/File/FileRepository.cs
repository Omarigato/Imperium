using Dapper;
using System;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.File
{
    public class FileRepository : BaseRepository<Core.Models.File>, IFileRepository
    {
        public FileRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Files")
        {
        }

        public async Task Insert(Core.Models.File file)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `Files` (
                    `Id`, `Url`, `PublicId`, `FileName`, `Size`, `MimeType`,
                    `AuthorId`, `CreateDate`)
                VALUES (
                    @Id, @Url, @PublicId, @FileName, @Size, @MimeType,
                    @AuthorId, @CreateDate)", file);
        }

        public async Task Update(Core.Models.File file)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `Files`
                SET 
                    `Url` = @Url,
                    `PublicId` = @PublicId,
                    `FileName` = @FileName,
                    `Size` = @Size,
                    `MimeType` = @MimeType,
                    `DeleteDate` = @DeleteDate
                WHERE `Id` = @Id", file);
        }

        public async Task<Core.Models.File?> GetByPublicIdAsync(string publicId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Files` 
                WHERE `PublicId` = @PublicId AND `DeleteDate` IS NULL";

            return await connection.QuerySingleOrDefaultAsync<Core.Models.File>(sql, new { PublicId = publicId });
        }

        public async Task<IEnumerable<Core.Models.File>> GetByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT f.* 
                FROM `Files` f
                INNER JOIN `ProductFiles` pf ON f.`Id` = pf.`FileId`
                WHERE pf.`ProductId` = @ProductId AND f.`DeleteDate` IS NULL AND pf.`DeleteDate` IS NULL
                ORDER BY pf.`IsAddition`, f.`CreateDate`";

            return await connection.QueryAsync<Core.Models.File>(sql, new { ProductId = productId });
        }

        public async Task<IEnumerable<Core.Models.File>> GetByAuthorAsync(Guid authorId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Files` 
                WHERE `AuthorId` = @AuthorId AND `DeleteDate` IS NULL
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.File>(sql, new { AuthorId = authorId });
        }

        public async Task<IEnumerable<Core.Models.File>> GetByMimeTypeAsync(string mimeType)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Files` 
                WHERE `MimeType` = @MimeType AND `DeleteDate` IS NULL
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.File>(sql, new { MimeType = mimeType });
        }

        public async Task<bool> DeleteByPublicIdAsync(string publicId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                UPDATE `Files` SET `DeleteDate` = @DeleteDate
                WHERE `PublicId` = @PublicId";

            var rowsAffected = await connection.ExecuteAsync(sql, new { PublicId = publicId, DeleteDate = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public override async Task<IEnumerable<Core.Models.File>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Files` WHERE `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.File>(sql);
        }
    }
}