using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class CartRepository : BaseRepository<Cart>, ICartRepository
    {
        public CartRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Carts")
        {
        }

        public async Task<IEnumerable<Cart>> GetByUserIdAsync(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Carts` 
                WHERE `UserId` = @UserId 
                ORDER BY `CreatedAt` ASC";

            return await connection.QueryAsync<Cart>(sql, new { UserId = userId });
        }

        public async Task<Cart?> GetByUserAndProductAsync(Guid userId, Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Carts` 
                WHERE `UserId` = @UserId AND `ProductId` = @ProductId";

            return await connection.QuerySingleOrDefaultAsync<Cart>(sql, new { UserId = userId, ProductId = productId });
        }

        public async Task<bool> ClearUserCartAsync(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"DELETE FROM `Carts` WHERE `UserId` = @UserId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Cart>> GetByUserIdWithDetailsAsync(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT c.*, 
                       p.`NameRu` as ProductNameRu, p.`NameKz` as ProductNameKz,
                       p.`Price` as ProductPrice, p.`Code` as ProductCode,
                       cat.`NameRu` as CategoryNameRu, cat.`NameKz` as CategoryNameKz,
                       col.`NameRu` as ColorNameRu, col.`NameKz` as ColorNameKz,
                       siz.`NameRu` as SizeNameRu, siz.`NameKz` as SizeNameKz
                FROM `Carts` c
                INNER JOIN `Products` p ON c.`ProductId` = p.`Id`
                LEFT JOIN `Dictionaries` cat ON p.`CategoryId` = cat.`Id`
                LEFT JOIN `Dictionaries` col ON c.`SelectedColorId` = col.`Id`
                LEFT JOIN `Dictionaries` siz ON c.`SelectedSizeId` = siz.`Id`
                WHERE c.`UserId` = @UserId
                ORDER BY c.`CreatedAt` ASC";

            return await connection.QueryAsync<Cart>(sql, new { UserId = userId });
        }
    }
}
