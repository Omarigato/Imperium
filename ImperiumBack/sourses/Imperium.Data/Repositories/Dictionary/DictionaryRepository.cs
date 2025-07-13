using Dapper;
using System;
using Imperium.Core.Enums;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Dictionary
{
    public class DictionaryRepository : BaseRepository<Core.Models.Dictionary>, IDictionaryRepository
    {
        public DictionaryRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Dictionaries")
        {
        }

        public async Task Insert(Core.Models.Dictionary dictionary)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `Dictionaries` (
                    `Id`, `Type`, `NameRu`, `NameKz`, `Code`, `Data`,
                    `DescriptionRu`, `DescriptionKz`, `ParentId`, `IsActive`,
                    `AuthorId`, `CreateDate`)
                VALUES (
                    @Id, @Type, @NameRu, @NameKz, @Code, @Data,
                    @DescriptionRu, @DescriptionKz, @ParentId, @IsActive,
                    @AuthorId, @CreateDate)", dictionary);
        }

        public async Task Update(Core.Models.Dictionary dictionary)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `Dictionaries`
                SET 
                    `Type` = @Type,
                    `NameRu` = @NameRu,
                    `NameKz` = @NameKz,
                    `Code` = @Code,
                    `Data` = @Data,
                    `DescriptionRu` = @DescriptionRu,
                    `DescriptionKz` = @DescriptionKz,
                    `ParentId` = @ParentId,
                    `IsActive` = @IsActive
                WHERE `Id` = @Id", dictionary);
        }

        public async Task<IEnumerable<Core.Models.Dictionary>> GetByTypeAsync(string type)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Dictionaries` 
                WHERE `Type` = @Type AND `IsActive` = true 
                ORDER BY `NameRu`";

            return await connection.QueryAsync<Core.Models.Dictionary>(sql, new { Type = type });
        }

        public async Task<Core.Models.Dictionary?> GetByCodeAsync(string code)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Dictionaries` 
                WHERE `Code` = @Code AND `IsActive` = true";

            return await connection.QuerySingleOrDefaultAsync<Core.Models.Dictionary>(sql, new { Code = code });
        }

        public async Task<IEnumerable<Core.Models.Dictionary>> GetChildrenByParentIdAsync(Guid parentId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Dictionaries` 
                WHERE `ParentId` = @ParentId AND `IsActive` = true 
                ORDER BY `NameRu`";

            return await connection.QueryAsync<Core.Models.Dictionary>(sql, new { ParentId = parentId });
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "UPDATE `Dictionaries` SET `IsActive` = false WHERE `Id` = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Core.Models.Dictionary>> GetCategoriesWithChildrenAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                WITH RECURSIVE CategoryHierarchy AS (
                    SELECT *, 0 as `Level`
                    FROM `Dictionaries`
                    WHERE `Type` = @Type AND `ParentId` IS NULL AND `IsActive` = true

                    UNION ALL

                    SELECT d.*, ch.`Level` + 1
                    FROM `Dictionaries` d
                    INNER JOIN CategoryHierarchy ch ON d.`ParentId` = ch.`Id`
                    WHERE d.`IsActive` = true
                )
                SELECT * FROM CategoryHierarchy
                ORDER BY `Level`, `NameRu`";

            return await connection.QueryAsync<Core.Models.Dictionary>(sql, new { Type = DictionaryType.Categories });
        }
    }
}
