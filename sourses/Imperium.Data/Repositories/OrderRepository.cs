using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Orders")
        {
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM ""Orders"" 
                WHERE ""UserId"" = @UserId 
                ORDER BY ""CreatedAt"" DESC";

            return await connection.QueryAsync<Order>(sql, new { UserId = userId });
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM ""Orders"" 
                WHERE ""OrderNumber"" = @OrderNumber";

            return await connection.QuerySingleOrDefaultAsync<Order>(sql, new { OrderNumber = orderNumber });
        }

        public async Task<Order?> GetWithDetailsAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            // Получаем основную информацию о заказе
            var orderSql = @"
                SELECT o.*, 
                       u.""FullName"" as UserFullName, u.""Email"" as UserEmail, u.""Phone"" as UserPhone,
                       a.""Title"" as AddressTitle, a.""City"" as AddressCity, a.""Street"" as AddressStreet,
                       a.""HouseNumber"" as AddressHouseNumber, a.""Apartment"" as AddressApartment
                FROM ""Orders"" o
                LEFT JOIN ""Users"" u ON o.""UserId"" = u.""Id""
                LEFT JOIN ""Addresses"" a ON o.""DeliveryAddressId"" = a.""Id""
                WHERE o.""Id"" = @Id";

            var order = await connection.QuerySingleOrDefaultAsync<Order>(orderSql, new { Id = id });
            if (order == null) return null;

            // Получаем элементы заказа
            var itemsSql = @"
                SELECT oi.*,
                       p.""NameRu"" as ProductNameRu, p.""NameKz"" as ProductNameKz, p.""Code"" as ProductCode,
                       col.""NameRu"" as ColorNameRu, col.""NameKz"" as ColorNameKz,
                       siz.""NameRu"" as SizeNameRu, siz.""NameKz"" as SizeNameKz
                FROM ""OrderItems"" oi
                LEFT JOIN ""Products"" p ON oi.""ProductId"" = p.""Id""
                LEFT JOIN ""Dictionaries"" col ON oi.""SelectedColorId"" = col.""Id""
                LEFT JOIN ""Dictionaries"" siz ON oi.""SelectedSizeId"" = siz.""Id""
                WHERE oi.""OrderId"" = @OrderId";

            var items = await connection.QueryAsync<OrderItem>(itemsSql, new { OrderId = id });

            return order;
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var sql = @"
                SELECT ""OrderNumber"" 
                FROM ""Orders"" 
                WHERE ""OrderNumber"" LIKE @Pattern 
                ORDER BY ""OrderNumber"" DESC 
                LIMIT 1";

            var pattern = $"IMP{today}%";
            var lastOrderNumber = await connection.QuerySingleOrDefaultAsync<string>(sql, new { Pattern = pattern });

            if (string.IsNullOrEmpty(lastOrderNumber))
            {
                return $"IMP{today}001";
            }

            var lastNumber = int.Parse(lastOrderNumber.Substring(11));
            return $"IMP{today}{(lastNumber + 1):D3}";
        }

        public async Task<IEnumerable<Order>> GetByUserIdWithDetailsAsync(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT o.*, 
                       a.""Title"" as AddressTitle, a.""City"" as AddressCity, a.""Street"" as AddressStreet,
                       a.""HouseNumber"" as AddressHouseNumber, a.""Apartment"" as AddressApartment
                FROM ""Orders"" o
                LEFT JOIN ""Addresses"" a ON o.""DeliveryAddressId"" = a.""Id""
                WHERE o.""UserId"" = @UserId
                ORDER BY o.""CreatedAt"" DESC";

            return await connection.QueryAsync<Order>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<Order>> GetAllWithDetailsAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT o.*, 
                       u.""FullName"" as UserFullName, u.""Email"" as UserEmail, u.""Phone"" as UserPhone,
                       a.""Title"" as AddressTitle, a.""City"" as AddressCity, a.""Street"" as AddressStreet,
                       a.""HouseNumber"" as AddressHouseNumber, a.""Apartment"" as AddressApartment
                FROM ""Orders"" o
                LEFT JOIN ""Users"" u ON o.""UserId"" = u.""Id""
                LEFT JOIN ""Addresses"" a ON o.""DeliveryAddressId"" = a.""Id""
                ORDER BY o.""CreatedAt"" DESC";

            return await connection.QueryAsync<Order>(sql);
        }
    }
}