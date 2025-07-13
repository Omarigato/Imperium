using Dapper;
using System;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Cart
{
    public class CartRepository : BaseRepository<Core.Models.Cart>, ICartRepository
    {
        public CartRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Carts")
        {
        }

        public async Task Insert(Core.Models.Cart cart)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `Carts` (
                    `Id`, `ClientId`, `ProductId`, `Quantity`, 
                    `SelectedColorId`, `SelectedSizeId`, `Notes`, `CreateDate`)
                VALUES (
                    @Id, @ClientId, @ProductId, @Quantity,
                    @SelectedColorId, @SelectedSizeId, @Notes, @CreateDate)", cart);
        }

        public async Task Update(Core.Models.Cart cart)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `Carts`
                SET 
                    `ClientId` = @ClientId,
                    `ProductId` = @ProductId,
                    `Quantity` = @Quantity,
                    `SelectedColorId` = @SelectedColorId,
                    `SelectedSizeId` = @SelectedSizeId,
                    `Notes` = @Notes,
                    `DeleteDate` = @DeleteDate
                WHERE `Id` = @Id", cart);
        }

        public async Task<IEnumerable<Core.Models.Cart>> GetByClientIdAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            return await connection.QueryAsync<Core.Models.Cart>(@"
                SELECT * FROM `Carts` 
                WHERE `ClientId` = @ClientId AND `DeleteDate` IS NULL
                ORDER BY `CreateDate` ASC", new { ClientId = clientId });
        }

        public async Task<Core.Models.Cart?> GetByClientAndProductAsync(Guid clientId, Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            return await connection.QuerySingleOrDefaultAsync<Core.Models.Cart>(@"
                SELECT * FROM `Carts` 
                WHERE `DeleteDate` IS NULL AND `ClientId` = @ClientId AND `ProductId` = @ProductId",
                new { ClientId = clientId, ProductId = productId });
        }

        public async Task<bool> ClearClientCartAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var rowsAffected = await connection.ExecuteAsync(@"
                UPDATE `Carts` SET `DeleteDate` = @DeleteDate 
                WHERE `ClientId` = @ClientId AND `DeleteDate` IS NULL",
                new { ClientId = clientId, DeleteDate = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Core.Models.Cart>> GetByClientIdWithDetailsAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            return await connection.QueryAsync<Core.Models.Cart>(@"
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
                WHERE c.`ClientId` = @ClientId AND c.`DeleteDate` IS NULL
                ORDER BY c.`CreateDate` ASC", new { ClientId = clientId });
        }

        public override async Task<IEnumerable<Core.Models.Cart>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Carts` WHERE `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.Cart>(sql);
        }
    }
}