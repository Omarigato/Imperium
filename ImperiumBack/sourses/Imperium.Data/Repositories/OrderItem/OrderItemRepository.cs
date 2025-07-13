using Dapper;
using System;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.OrderItem
{
    public class OrderItemRepository : BaseRepository<Core.Models.OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "OrderItems")
        {
        }

        public async Task Insert(Core.Models.OrderItem orderItem)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `OrderItems` (
                    `Id`, `OrderId`, `ProductId`, `Quantity`, `UnitPrice`, `TotalPrice`,
                    `SelectedColorId`, `SelectedSizeId`, `ItemNotes`)
                VALUES (
                    @Id, @OrderId, @ProductId, @Quantity, @UnitPrice, @TotalPrice,
                    @SelectedColorId, @SelectedSizeId, @ItemNotes)", orderItem);
        }

        public async Task Update(Core.Models.OrderItem orderItem)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `OrderItems`
                SET 
                    `OrderId` = @OrderId,
                    `ProductId` = @ProductId,
                    `Quantity` = @Quantity,
                    `UnitPrice` = @UnitPrice,
                    `TotalPrice` = @TotalPrice,
                    `SelectedColorId` = @SelectedColorId,
                    `SelectedSizeId` = @SelectedSizeId,
                    `ItemNotes` = @ItemNotes
                WHERE `Id` = @Id", orderItem);
        }

        public async Task<IEnumerable<Core.Models.OrderItem>> GetByOrderIdAsync(Guid orderId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `OrderItems` 
                WHERE `OrderId` = @OrderId";

            return await connection.QueryAsync<Core.Models.OrderItem>(sql, new { OrderId = orderId });
        }

        public async Task<IEnumerable<Core.Models.OrderItem>> GetByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `OrderItems` 
                WHERE `ProductId` = @ProductId";

            return await connection.QueryAsync<Core.Models.OrderItem>(sql, new { ProductId = productId });
        }

        public async Task<bool> DeleteByOrderIdAsync(Guid orderId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                DELETE FROM `OrderItems` 
                WHERE `OrderId` = @OrderId";

            var rowsAffected = await connection.ExecuteAsync(sql, new { OrderId = orderId });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Core.Models.OrderItem>> GetByOrderIdWithDetailsAsync(Guid orderId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT oi.*,
                       p.`NameRu` as ProductNameRu, p.`NameKz` as ProductNameKz, p.`Code` as ProductCode,
                       col.`NameRu` as ColorNameRu, col.`NameKz` as ColorNameKz,
                       siz.`NameRu` as SizeNameRu, siz.`NameKz` as SizeNameKz
                FROM `OrderItems` oi
                LEFT JOIN `Products` p ON oi.`ProductId` = p.`Id`
                LEFT JOIN `Dictionaries` col ON oi.`SelectedColorId` = col.`Id`
                LEFT JOIN `Dictionaries` siz ON oi.`SelectedSizeId` = siz.`Id`
                WHERE oi.`OrderId` = @OrderId";

            return await connection.QueryAsync<Core.Models.OrderItem>(sql, new { OrderId = orderId });
        }
    }
}