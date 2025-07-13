using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories.Order
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
                SELECT * FROM `Orders` 
                WHERE `UserId` = @UserId 
                ORDER BY `CreatedAt` DESC";

            return await connection.QueryAsync<Order>(sql, new { UserId = userId });
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Orders` 
                WHERE `OrderNumber` = @OrderNumber";

            return await connection.QuerySingleOrDefaultAsync<Order>(sql, new { OrderNumber = orderNumber });
        }

        public async Task<Order?> GetWithDetailsAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT o.*,
                       u.`FullName` as UserFullName, u.`Email` as UserEmail,
                       a.`Title` as AddressTitle, a.`City` as AddressCity, a.`Street` as AddressStreet
                FROM `Orders` o
                LEFT JOIN `Users` u ON o.`UserId` = u.`Id`
                LEFT JOIN `Addresses` a ON o.`DeliveryAddressId` = a.`Id`
                WHERE o.`Id` = @Id";

            return await connection.QuerySingleOrDefaultAsync<Order>(sql, new { Id = id });
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"IMP{today}";

            var sql = @"
                SELECT `OrderNumber` FROM `Orders` 
                WHERE `OrderNumber` LIKE CONCAT(@Prefix, '%')
                ORDER BY `OrderNumber` DESC 
                LIMIT 1";

            var lastOrderNumber = await connection.QuerySingleOrDefaultAsync<string>(sql, new { Prefix = prefix });

            if (string.IsNullOrEmpty(lastOrderNumber))
            {
                return $"{prefix}001";
            }

            var lastNumber = int.Parse(lastOrderNumber.Substring(prefix.Length));
            return $"{prefix}{lastNumber + 1:D3}";
        }

        public async Task<IEnumerable<Order>> GetByUserIdWithDetailsAsync(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT o.*,
                       u.`FullName` as UserFullName, u.`Email` as UserEmail,
                       a.`Title` as AddressTitle, a.`City` as AddressCity, a.`Street` as AddressStreet
                FROM `Orders` o
                LEFT JOIN `Users` u ON o.`UserId` = u.`Id`
                LEFT JOIN `Addresses` a ON o.`DeliveryAddressId` = a.`Id`
                WHERE o.`UserId` = @UserId
                ORDER BY o.`CreatedAt` DESC";

            return await connection.QueryAsync<Order>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<Order>> GetAllWithDetailsAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT o.*,
                       u.`FullName` as UserFullName, u.`Email` as UserEmail,
                       a.`Title` as AddressTitle, a.`City` as AddressCity, a.`Street` as AddressStreet
                FROM `Orders` o
                LEFT JOIN `Users` u ON o.`UserId` = u.`Id`
                LEFT JOIN `Addresses` a ON o.`DeliveryAddressId` = a.`Id`
                ORDER BY o.`CreatedAt` DESC";

            return await connection.QueryAsync<Order>(sql);
        }
    }
}
