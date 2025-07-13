using Dapper;
using System;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Order
{
    public class OrderRepository : BaseRepository<Core.Models.Order>, IOrderRepository
    {
        public OrderRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Orders")
        {
        }

        public async Task Insert(Core.Models.Order order)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `Orders` (
                    `Id`, `OrderNumber`, `ClientId`, `DeliveryAddressId`, `Status`,
                    `TotalAmount`, `DeliveryFee`, `Notes`, `AdminNotes`,
                    `CreateDate`, `UpdateDate`)
                VALUES (
                    @Id, @OrderNumber, @ClientId, @DeliveryAddressId, @Status,
                    @TotalAmount, @DeliveryFee, @Notes, @AdminNotes,
                    @CreateDate, @UpdateDate)", order);
        }

        public async Task Update(Core.Models.Order order)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `Orders`
                SET 
                    `OrderNumber` = @OrderNumber,
                    `ClientId` = @ClientId,
                    `DeliveryAddressId` = @DeliveryAddressId,
                    `Status` = @Status,
                    `TotalAmount` = @TotalAmount,
                    `DeliveryFee` = @DeliveryFee,
                    `Notes` = @Notes,
                    `AdminNotes` = @AdminNotes,
                    `UpdateDate` = @UpdateDate,
                    `DeleteDate` = @DeleteDate
                WHERE `Id` = @Id", order);
        }

        public async Task<IEnumerable<Core.Models.Order>> GetByClientIdAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Orders` 
                WHERE `ClientId` = @ClientId AND `DeleteDate` IS NULL
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Order>(sql, new { ClientId = clientId });
        }

        public async Task<Core.Models.Order?> GetByOrderNumberAsync(string orderNumber)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Orders` 
                WHERE `OrderNumber` = @OrderNumber AND `DeleteDate` IS NULL";

            return await connection.QuerySingleOrDefaultAsync<Core.Models.Order>(sql, new { OrderNumber = orderNumber });
        }

        public async Task<IEnumerable<Core.Models.Order>> GetByStatusAsync(string status)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Orders` 
                WHERE `Status` = @Status AND `DeleteDate` IS NULL
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Order>(sql, new { Status = status });
        }

        public async Task<bool> UpdateStatusAsync(Guid orderId, string status)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                UPDATE `Orders` 
                SET `Status` = @Status, `UpdateDate` = @UpdateDate 
                WHERE `Id` = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = orderId,
                Status = status,
                UpdateDate = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }

        public async Task<Core.Models.Order?> GetWithDetailsAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT o.*,
                       c.`FullName` as ClientFullName, c.`Phone` as ClientPhone,
                       a.`Title` as AddressTitle, a.`City` as AddressCity, a.`Street` as AddressStreet
                FROM `Orders` o
                LEFT JOIN `Clients` c ON o.`ClientId` = c.`Id`
                LEFT JOIN `Addresses` a ON o.`DeliveryAddressId` = a.`Id`
                WHERE o.`Id` = @Id AND o.`DeleteDate` IS NULL";

            return await connection.QuerySingleOrDefaultAsync<Core.Models.Order>(sql, new { Id = id });
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

        public async Task<IEnumerable<Core.Models.Order>> GetByClientIdWithDetailsAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT o.*,
                       c.`FullName` as ClientFullName, c.`Phone` as ClientPhone,
                       a.`Title` as AddressTitle, a.`City` as AddressCity, a.`Street` as AddressStreet
                FROM `Orders` o
                LEFT JOIN `Clients` c ON o.`ClientId` = c.`Id`
                LEFT JOIN `Addresses` a ON o.`DeliveryAddressId` = a.`Id`
                WHERE o.`ClientId` = @ClientId AND o.`DeleteDate` IS NULL
                ORDER BY o.`CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Order>(sql, new { ClientId = clientId });
        }

        public async Task<IEnumerable<Core.Models.Order>> GetAllWithDetailsAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT o.*,
                       c.`FullName` as ClientFullName, c.`Phone` as ClientPhone,
                       a.`Title` as AddressTitle, a.`City` as AddressCity, a.`Street` as AddressStreet
                FROM `Orders` o
                LEFT JOIN `Clients` c ON o.`ClientId` = c.`Id`
                LEFT JOIN `Addresses` a ON o.`DeliveryAddressId` = a.`Id`
                WHERE o.`DeleteDate` IS NULL
                ORDER BY o.`CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Order>(sql);
        }

        public override async Task<IEnumerable<Core.Models.Order>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Orders` WHERE `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.Order>(sql);
        }
    }
}