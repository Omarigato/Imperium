using Dapper;
using Imperium.Core.Enums;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class DictionaryRepository : BaseRepository<Dictionary>, IDictionaryRepository
    {
        public DictionaryRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Dictionaries")
        {
        }

        public async Task<IEnumerable<Dictionary>> GetByTypeAsync(string type)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM ""Dictionaries"" 
                WHERE ""Type"" = @Type AND ""IsActive"" = true 
                ORDER BY ""NameRu""";

            return await connection.QueryAsync<Dictionary>(sql, new { Type = type });
        }

        public async Task<Dictionary?> GetByCodeAsync(string code)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM ""Dictionaries"" 
                WHERE ""Code"" = @Code AND ""IsActive"" = true";

            return await connection.QuerySingleOrDefaultAsync<Dictionary>(sql, new { Code = code });
        }

        public async Task<IEnumerable<Dictionary>> GetCategoriesAsync()
        {
            return await GetByTypeAsync(DictionaryType.Categories);
        }

        public async Task<IEnumerable<Dictionary>> GetColorsAsync()
        {
            return await GetByTypeAsync(DictionaryType.Colors);
        }

        public async Task<IEnumerable<Dictionary>> GetSizesAsync()
        {
            return await GetByTypeAsync(DictionaryType.Sizes);
        }

        public async Task<IEnumerable<Dictionary>> GetMaterialsAsync()
        {
            return await GetByTypeAsync(DictionaryType.Materials);
        }

        public async Task<IEnumerable<Dictionary>> GetCategoriesWithChildrenAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                WITH RECURSIVE category_tree AS (
                    -- Корневые категории
                    SELECT ""Id"", ""Type"", ""NameRu"", ""NameKz"", ""Code"", ""Value"", 
                           ""DescriptionRu"", ""DescriptionKz"", ""ParentId"", ""IsActive"", ""CreatedAt"", 0 as level
                    FROM ""Dictionaries""
                    WHERE ""Type"" = @Type AND ""ParentId"" IS NULL AND ""IsActive"" = true
                    
                    UNION ALL
                    
                    -- Дочерние категории
                    SELECT d.""Id"", d.""Type"", d.""NameRu"", d.""NameKz"", d.""Code"", d.""Value"",
                           d.""DescriptionRu"", d.""DescriptionKz"", d.""ParentId"", d.""IsActive"", d.""CreatedAt"", ct.level + 1
                    FROM ""Dictionaries"" d
                    INNER JOIN category_tree ct ON d.""ParentId"" = ct.""Id""
                    WHERE d.""IsActive"" = true
                )
                SELECT * FROM category_tree
                ORDER BY level, ""NameRu""";

            return await connection.QueryAsync<Dictionary>(sql, new { Type = DictionaryType.Categories });
        }
    }
}