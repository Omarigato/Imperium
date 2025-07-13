using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories.Product
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Products")
        {
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Products` 
                WHERE `CategoryId` = @CategoryId AND `IsAvailable` = true
                ORDER BY `CreatedAt` DESC";

            return await connection.QueryAsync<Product>(sql, new { CategoryId = categoryId });
        }

        public async Task<IEnumerable<Product>> GetFeaturedAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Products` 
                WHERE `IsFeatured` = true AND `IsAvailable` = true
                ORDER BY `CreatedAt` DESC";

            return await connection.QueryAsync<Product>(sql);
        }

        public async Task<IEnumerable<Product>> GetAvailableAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Products` 
                WHERE `IsAvailable` = true
                ORDER BY `CreatedAt` DESC";

            return await connection.QueryAsync<Product>(sql);
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Products` 
                WHERE `Code` = @Code";

            return await connection.QuerySingleOrDefaultAsync<Product>(sql, new { Code = code });
        }

        public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Products` 
                WHERE `IsAvailable` = true 
                AND (
                    LOWER(`NameRu`) LIKE @SearchTerm OR 
                    LOWER(`NameKz`) LIKE @SearchTerm OR
                    LOWER(`DescriptionRu`) LIKE @SearchTerm OR
                    LOWER(`DescriptionKz`) LIKE @SearchTerm OR
                    LOWER(`Code`) LIKE @SearchTerm
                )
                ORDER BY `CreatedAt` DESC";

            var searchPattern = $"%{searchTerm.ToLower()}%";
            return await connection.QueryAsync<Product>(sql, new { SearchTerm = searchPattern });
        }

        public async Task<Product?> GetWithDetailsAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var productSql = @"
                SELECT p.*, 
                       cat.`Id` as CategoryId, cat.`NameRu` as CategoryNameRu, cat.`NameKz` as CategoryNameKz, cat.`Code` as CategoryCode,
                       mat.`Id` as MaterialId, mat.`NameRu` as MaterialNameRu, mat.`NameKz` as MaterialNameKz, mat.`Code` as MaterialCode
                FROM `Products` p
                LEFT JOIN `Dictionaries` cat ON p.`CategoryId` = cat.`Id`
                LEFT JOIN `Dictionaries` mat ON p.`MaterialId` = mat.`Id`
                WHERE p.`Id` = @Id";

            var product = await connection.QuerySingleOrDefaultAsync<Product>(productSql, new { Id = id });
            if (product == null) return null;

            var colorsSql = @"
                SELECT pc.*, c.`NameRu`, c.`NameKz`, c.`Code`, c.`Value`
                FROM `ProductColors` pc
                INNER JOIN `Dictionaries` c ON pc.`ColorId` = c.`Id`
                WHERE pc.`ProductId` = @ProductId AND pc.`IsAvailable` = true";

            var colors = await connection.QueryAsync(colorsSql, new { ProductId = id });

            var sizesSql = @"
                SELECT ps.*, s.`NameRu`, s.`NameKz`, s.`Code`, s.`Value`
                FROM `ProductSizes` ps
                INNER JOIN `Dictionaries` s ON ps.`SizeId` = s.`Id`
                WHERE ps.`ProductId` = @ProductId AND ps.`IsAvailable` = true";

            var sizes = await connection.QueryAsync(sizesSql, new { ProductId = id });

            var filesSql = @"   
                SELECT f.*
                FROM `ProductFiles` pf
                INNER JOIN `Files` f ON pf.`FileId` = f.`Id`
                WHERE pf.`ProductId` = @ProductId
                ORDER BY pf.`IsAddition`, f.`CreatedAt`";

            var files = await connection.QueryAsync<File>(filesSql, new { ProductId = id });

            return product;
        }

        public async Task<IEnumerable<Product>> GetByCategoryWithDetailsAsync(Guid categoryId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT p.*, 
                       cat.`NameRu` as CategoryNameRu, cat.`NameKz` as CategoryNameKz,
                       mat.`NameRu` as MaterialNameRu, mat.`NameKz` as MaterialNameKz
                FROM `Products` p
                LEFT JOIN `Dictionaries` cat ON p.`CategoryId` = cat.`Id`
                LEFT JOIN `Dictionaries` mat ON p.`MaterialId` = mat.`Id`
                WHERE p.`CategoryId` = @CategoryId AND p.`IsAvailable` = true
                ORDER BY p.`CreatedAt` DESC";

            return await connection.QueryAsync<Product>(sql, new { CategoryId = categoryId });
        }
    }
}
