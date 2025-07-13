using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories.Address
{
    public class AddressRepository : BaseRepository<Address>, IAddressRepository
    {
        public AddressRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Addresses")
        {
        }

        public async Task<IEnumerable<Address>> GetByClientIdAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Addresses` 
                WHERE `ClientId` = @ClientID
                ORDER BY `IsDefault` DESC, `CreateDate` DESC";

            return await connection.QueryAsync<Address>(sql, new { ClientId = clientId });
        }

        public async Task<Address?> GetDefaultByClientIdAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Addresses` 
                WHERE `ClientId` = @ClientId AND `IsDefault` = true";

            return await connection.QuerySingleOrDefaultAsync<Address>(sql, new { ClientId = clientId });
        }

        public async Task<bool> SetDefaultAddressAsync(Guid clientId, Guid addressId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Снимаем флаг `IsDefault` со всех адресов
                await UnsetDefaultAddressesAsync(clientId, connection, transaction);

                // Устанавливаем нужный адрес как default
                var sql = @"
                    UPDATE `Addresses` 
                    SET `IsDefault` = true 
                    WHERE `Id` = @AddressId AND `ClientId` = @CLientId";

                var rowsAffected = await connection.ExecuteAsync(sql, new { AddressId = addressId, UserId = clientId }, transaction);

                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> UnsetDefaultAddressesAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            return await UnsetDefaultAddressesAsync(clientId, connection, null);
        }

        private async Task<bool> UnsetDefaultAddressesAsync(Guid clientId, System.Data.IDbConnection connection, System.Data.IDbTransaction? transaction)
        {
            var sql = @"
                UPDATE `Addresses` 
                SET `IsDefault` = false 
                WHERE `ClientId` = @ClientId";

            var rowsAffected = await connection.ExecuteAsync(sql, new { ClientId = clientId }, transaction);
            return rowsAffected >= 0;
        }
    }
}
