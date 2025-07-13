using Dapper;
using System;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Address
{
    public class AddressRepository : BaseRepository<Core.Models.Address>, IAddressRepository
    {
        public AddressRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Addresses")
        {
        }

        public async Task Insert(Core.Models.Address address)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `Addresses` (
                    `Id`, `ClientId`, `Title`, `City`, `Street`, `HouseNumber`,
                    `Apartment`, `Notes`, `IsDefault`, `CreateDate`)
                VALUES (
                    @Id, @ClientId, @Title, @City, @Street, @HouseNumber,
                    @Apartment, @Notes, @IsDefault, @CreateDate)", address);
        }

        public async Task Update(Core.Models.Address address)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `Addresses`
                SET 
                    `ClientId` = @ClientId,
                    `Title` = @Title,
                    `City` = @City,
                    `Street` = @Street,
                    `HouseNumber` = @HouseNumber,
                    `Apartment` = @Apartment,
                    `Notes` = @Notes,
                    `IsDefault` = @IsDefault,
                    `DeleteDate` = @DeleteDate
                WHERE `Id` = @Id", address);
        }

        public async Task<IEnumerable<Core.Models.Address>> GetByClientIdAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Addresses` 
                WHERE `ClientId` = @ClientId AND `DeleteDate` IS NULL
                ORDER BY `IsDefault` DESC, `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Address>(sql, new { ClientId = clientId });
        }

        public async Task<Core.Models.Address?> GetDefaultByClientIdAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Addresses` 
                WHERE `ClientId` = @ClientId AND `IsDefault` = true AND `DeleteDate` IS NULL";

            return await connection.QuerySingleOrDefaultAsync<Core.Models.Address>(sql, new { ClientId = clientId });
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
                    WHERE `Id` = @AddressId AND `ClientId` = @ClientId";

                var rowsAffected = await connection.ExecuteAsync(sql, new { AddressId = addressId, ClientId = clientId }, transaction);

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

        public override async Task<IEnumerable<Core.Models.Address>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Addresses` WHERE `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.Address>(sql);
        }
    }
}