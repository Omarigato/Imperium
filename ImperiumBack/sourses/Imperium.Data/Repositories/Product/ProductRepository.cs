using Dapper;
using System;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Product
{
    public class ProductRepository : BaseRepository<Core.Models.Product>, IProductRepository
    {
        public ProductRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Products")
        {
        }

        public async Task Insert(Core.Models.Product product)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `Products` (
                    `Id`, `CategoryId`, `MaterialId`, `NameRu`, `NameKz`, `Code`,
                    `DescriptionRu`, `DescriptionKz`, `Price`, `IsAvailable`,
                    `AuthorId`, `CreateDate`, `UpdatedAt`)
                VALUES (
                    @Id, @CategoryId, @MaterialId, @NameRu, @NameKz, @Code,
                    @DescriptionRu, @DescriptionKz, @Price, @IsAvailable,
                    @AuthorId, @CreateDate, @UpdatedAt)", product);
        }

        public async Task Update(Core.Models.Product product)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `Products`
                SET 
                    `CategoryId` = @CategoryId,
                    `MaterialId` = @MaterialId,
                    `NameRu` = @NameRu,
                    `NameKz` = @NameKz,
                    `Code` = @Code,
                    `DescriptionRu` = @DescriptionRu,
                    `DescriptionKz` = @DescriptionKz,
                    `Price` = @Price,
                    `IsAvailable` = @IsAvailable,
                    `UpdatedAt` = @UpdatedAt,
                    `DeleteDate` = @DeleteDate
                WHERE `Id` = @Id", product);
        }

        public async Task<IEnumerable<Core.Models.Product>> GetByCategoryAsync(Guid categoryId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Products` 
                WHERE `CategoryId` = @CategoryId AND `IsAvailable` = true AND `DeleteDate` IS NULL
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Product>(sql, new { CategoryId = categoryId });
        }

        public async Task<IEnumerable<Core.Models.Product>> GetAvailableAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Products` 
                WHERE `IsAvailable` = true AND `DeleteDate` IS NULL
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Product>(sql);
        }

        public async Task<Core.Models.Product?> GetByCodeAsync(string code)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Products` 
                WHERE `Code` = @Code AND `DeleteDate` IS NULL";

            return await connection.QuerySingleOrDefaultAsync<Core.Models.Product>(sql, new { Code = code });
        }

        public async Task<IEnumerable<Core.Models.Product>> SearchAsync(string searchTerm)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Products` 
                WHERE `IsAvailable` = true AND `DeleteDate` IS NULL
                AND (
                    LOWER(`NameRu`) LIKE @SearchTerm OR 
                    LOWER(`NameKz`) LIKE @SearchTerm OR
                    LOWER(`DescriptionRu`) LIKE @SearchTerm OR
                    LOWER(`DescriptionKz`) LIKE @SearchTerm OR
                    LOWER(`Code`) LIKE @SearchTerm
                )
                ORDER BY `CreateDate` DESC";

            var searchPattern = $"%{searchTerm.ToLower()}%";
            return await connection.QueryAsync<Core.Models.Product>(sql, new { SearchTerm = searchPattern });
        }

        public async Task<IEnumerable<Core.Models.Product>> GetByMaterialAsync(Guid materialId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Products` 
                WHERE `MaterialId` = @MaterialId AND `IsAvailable` = true AND `DeleteDate` IS NULL
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Product>(sql, new { MaterialId = materialId });
        }

        public async Task<IEnumerable<Core.Models.Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Products` 
                WHERE `Price` BETWEEN @MinPrice AND @MaxPrice 
                AND `IsAvailable` = true AND `DeleteDate` IS NULL
                ORDER BY `Price` ASC";

            return await connection.QueryAsync<Core.Models.Product>(sql, new { MinPrice = minPrice, MaxPrice = maxPrice });
        }

        public async Task<IEnumerable<Core.Models.Product>> GetByAuthorAsync(Guid authorId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Products` 
                WHERE `AuthorId` = @AuthorId AND `DeleteDate` IS NULL
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Product>(sql, new { AuthorId = authorId });
        }

        public async Task<Core.Models.Product?> GetWithDetailsAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var productSql = @"
                SELECT p.*, 
                       cat.`Id` as CategoryId, cat.`NameRu` as CategoryNameRu, cat.`NameKz` as CategoryNameKz, cat.`Code` as CategoryCode,
                       mat.`Id` as MaterialId, mat.`NameRu` as MaterialNameRu, mat.`NameKz` as MaterialNameKz, mat.`Code` as MaterialCode
                FROM `Products` p
                LEFT JOIN `Dictionaries` cat ON p.`CategoryId` = cat.`Id`
                LEFT JOIN `Dictionaries` mat ON p.`MaterialId` = mat.`Id`
                WHERE p.`Id` = @Id AND p.`DeleteDate` IS NULL";

            return await connection.QuerySingleOrDefaultAsync<Core.Models.Product>(productSql, new { Id = id });
        }

        public async Task<IEnumerable<Core.Models.Product>> GetByCategoryWithDetailsAsync(Guid categoryId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT p.*, 
                       cat.`NameRu` as CategoryNameRu, cat.`NameKz` as CategoryNameKz,
                       mat.`NameRu` as MaterialNameRu, mat.`NameKz` as MaterialNameKz
                FROM `Products` p
                LEFT JOIN `Dictionaries` cat ON p.`CategoryId` = cat.`Id`
                LEFT JOIN `Dictionaries` mat ON p.`MaterialId` = mat.`Id`
                WHERE p.`CategoryId` = @CategoryId AND p.`IsAvailable` = true AND p.`DeleteDate` IS NULL
                ORDER BY p.`CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Product>(sql, new { CategoryId = categoryId });
        }

        public override async Task<IEnumerable<Core.Models.Product>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Products` WHERE `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.Product>(sql);
        }
    }
}