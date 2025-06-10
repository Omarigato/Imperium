using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class OrderItemRepository : BaseRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "OrderItems")
        {
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `OrderItems` 
                WHERE `OrderId` = @OrderId";

            return await connection.QueryAsync<OrderItem>(sql, new { OrderId = orderId });
        }

        public async Task<IEnumerable<OrderItem>> GetByProductIdAsync(Guid productId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `OrderItems` 
                WHERE `ProductId` = @ProductId";

            return await connection.QueryAsync<OrderItem>(sql, new { ProductId = productId });
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

        public async Task<IEnumerable<OrderItem>> GetByOrderIdWithDetailsAsync(Guid orderId)
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

            return await connection.QueryAsync<OrderItem>(sql, new { OrderId = orderId });
        }
    }
}
