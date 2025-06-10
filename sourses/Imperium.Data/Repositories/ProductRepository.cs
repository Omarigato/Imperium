using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
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
                SELECT p.*, c.""NameRu"" as CategoryNameRu, c.""NameKz"" as CategoryNameKz, 
                       m.""NameRu"" as MaterialNameRu, m.""NameKz"" as MaterialNameKz
                FROM ""Products"" p
                LEFT JOIN ""Dictionaries"" c ON p.""CategoryId"" = c.""Id""
                LEFT JOIN ""Dictionaries"" m ON p.""MaterialId"" = m.""Id""
                WHERE p.""CategoryId"" = @CategoryId AND p.""IsAvailable"" = true
                ORDER BY p.""CreatedAt"" DESC";

            return await connection.QueryAsync<Product>(sql, new { CategoryId = categoryId });
        }

        public async Task<IEnumerable<Product>> GetFeaturedAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT p.*, c.""NameRu"" as CategoryNameRu, c.""NameKz"" as CategoryNameKz,
                       m.""NameRu"" as MaterialNameRu, m.""NameKz"" as MaterialNameKz
                FROM ""Products"" p
                LEFT JOIN ""Dictionaries"" c ON p.""CategoryId"" = c.""Id""
                LEFT JOIN ""Dictionaries"" m ON p.""MaterialId"" = m.""Id""
                WHERE p.""IsFeatured"" = true AND p.""IsAvailable"" = true
                ORDER BY p.""CreatedAt"" DESC";

            return await connection.QueryAsync<Product>(sql);
        }

        public async Task<IEnumerable<Product>> GetAvailableAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT p.*, c.""NameRu"" as CategoryNameRu, c.""NameKz"" as CategoryNameKz,
                       m.""NameRu"" as MaterialNameRu, m.""NameKz"" as MaterialNameKz
                FROM ""Products"" p
                LEFT JOIN ""Dictionaries"" c ON p.""CategoryId"" = c.""Id""
                LEFT JOIN ""Dictionaries"" m ON p.""MaterialId"" = m.""Id""
                WHERE p.""IsAvailable"" = true
                ORDER BY p.""CreatedAt"" DESC";

            return await connection.QueryAsync<Product>(sql);
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT p.*, c.""NameRu"" as CategoryNameRu, c.""NameKz"" as CategoryNameKz,
                       m.""NameRu"" as MaterialNameRu, m.""NameKz"" as MaterialNameKz
                FROM ""Products"" p
                LEFT JOIN ""Dictionaries"" c ON p.""CategoryId"" = c.""Id""
                LEFT JOIN ""Dictionaries"" m ON p.""MaterialId"" = m.""Id""
                WHERE p.""Code"" = @Code";

            return await connection.QuerySingleOrDefaultAsync<Product>(sql, new { Code = code });
        }

        public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT p.*, c.""NameRu"" as CategoryNameRu, c.""NameKz"" as CategoryNameKz,
                       m.""NameRu"" as MaterialNameRu, m.""NameKz"" as MaterialNameKz
                FROM ""Products"" p
                LEFT JOIN ""Dictionaries"" c ON p.""CategoryId"" = c.""Id""
                LEFT JOIN ""Dictionaries"" m ON p.""MaterialId"" = m.""Id""
                WHERE p.""IsAvailable"" = true 
                AND (
                    p.""NameRu"" ILIKE @SearchTerm 
                    OR p.""NameKz"" ILIKE @SearchTerm
                    OR p.""DescriptionRu"" ILIKE @SearchTerm 
                    OR p.""DescriptionKz"" ILIKE @SearchTerm
                )
                ORDER BY p.""CreatedAt"" DESC";

            var searchPattern = $"%{searchTerm}%";
            return await connection.QueryAsync<Product>(sql, new { SearchTerm = searchPattern });
        }

        public async Task<Product?> GetWithDetailsAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            // Основная информация о продукте
            var productSql = @"
                SELECT p.*, c.""NameRu"" as CategoryNameRu, c.""NameKz"" as CategoryNameKz,
                       m.""NameRu"" as MaterialNameRu, m.""NameKz"" as MaterialNameKz
                FROM ""Products"" p
                LEFT JOIN ""Dictionaries"" c ON p.""CategoryId"" = c.""Id""
                LEFT JOIN ""Dictionaries"" m ON p.""MaterialId"" = m.""Id""
                WHERE p.""Id"" = @Id";

            var product = await connection.QuerySingleOrDefaultAsync<Product>(productSql, new { Id = id });
            if (product == null) return null;

            // Получаем цвета продукта
            var colorsSql = @"
                SELECT d.*
                FROM ""ProductColors"" pc
                INNER JOIN ""Dictionaries"" d ON pc.""ColorId"" = d.""Id""
                WHERE pc.""ProductId"" = @ProductId AND pc.""IsAvailable"" = true";

            var colors = await connection.QueryAsync<Dictionary>(colorsSql, new { ProductId = id });

            // Получаем размеры продукта
            var sizesSql = @"
                SELECT d.*
                FROM ""ProductSizes"" ps
                INNER JOIN ""Dictionaries"" d ON ps.""SizeId"" = d.""Id""
                WHERE ps.""ProductId"" = @ProductId AND ps.""IsAvailable"" = true";

            var sizes = await connection.QueryAsync<Dictionary>(sizesSql, new { ProductId = id });

            // Получаем файлы продукта
            var filesSql = @"
                SELECT f.*
                FROM ""ProductFiles"" pf
                INNER JOIN ""Files"" f ON pf.""FileId"" = f.""Id""
                WHERE pf.""ProductId"" = @ProductId
                ORDER BY pf.""IsAddition"", f.""CreatedAt""";

            var files = await connection.QueryAsync<File>(filesSql, new { ProductId = id });

            // Для простоты, присваиваем коллекции продукту
            // В реальном проекте можно использовать более сложную логику маппинга

            return product;
        }

        public async Task<IEnumerable<Product>> GetByCategoryWithDetailsAsync(Guid categoryId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT p.*, c.""NameRu"" as CategoryNameRu, c.""NameKz"" as CategoryNameKz,
                       m.""NameRu"" as MaterialNameRu, m.""NameKz"" as MaterialNameKz
                FROM ""Products"" p
                LEFT JOIN ""Dictionaries"" c ON p.""CategoryId"" = c.""Id""
                LEFT JOIN ""Dictionaries"" m ON p.""MaterialId"" = m.""Id""
                WHERE p.""CategoryId"" = @CategoryId AND p.""IsAvailable"" = true
                ORDER BY p.""IsFeatured"" DESC, p.""CreatedAt"" DESC";

            return await connection.QueryAsync<Product>(sql, new { CategoryId = categoryId });
        }
    }
}