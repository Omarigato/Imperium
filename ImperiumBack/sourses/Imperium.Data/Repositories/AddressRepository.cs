using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class AddressRepository : BaseRepository<Address>, IAddressRepository
    {
        public AddressRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Addresses")
        {
        }

        public async Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Addresses` 
                WHERE `UserId` = @UserId 
                ORDER BY `IsDefault` DESC, `CreatedAt` DESC";

            return await connection.QueryAsync<Address>(sql, new { UserId = userId });
        }

        public async Task<Address?> GetDefaultByUserIdAsync(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Addresses` 
                WHERE `UserId` = @UserId AND `IsDefault` = true";

            return await connection.QuerySingleOrDefaultAsync<Address>(sql, new { UserId = userId });
        }

        public async Task<bool> SetDefaultAddressAsync(Guid userId, Guid addressId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Снимаем флаг `IsDefault` со всех адресов
                await UnsetDefaultAddressesAsync(userId, connection, transaction);

                // Устанавливаем нужный адрес как default
                var sql = @"
                    UPDATE `Addresses` 
                    SET `IsDefault` = true 
                    WHERE `Id` = @AddressId AND `UserId` = @UserId";

                var rowsAffected = await connection.ExecuteAsync(sql, new { AddressId = addressId, UserId = userId }, transaction);

                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> UnsetDefaultAddressesAsync(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            return await UnsetDefaultAddressesAsync(userId, connection, null);
        }

        private async Task<bool> UnsetDefaultAddressesAsync(Guid userId, System.Data.IDbConnection connection, System.Data.IDbTransaction? transaction)
        {
            var sql = @"
                UPDATE `Addresses` 
                SET `IsDefault` = false 
                WHERE `UserId` = @UserId";

            var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId }, transaction);
            return rowsAffected >= 0;
        }
    }
}
