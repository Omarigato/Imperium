using Dapper;
using System;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Client
{
    public class ClientRepository : BaseRepository<Core.Models.Client>, IClientRepository
    {
        public ClientRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Clients")
        {
        }

        public async Task Insert(Core.Models.Client client)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `Clients` (
                    `Id`, `FullName`, `Phone`, `IsPhoneVerified`, 
                    `CreateDate`, `UpdateDate`)
                VALUES (
                    @Id, @FullName, @Phone, @IsPhoneVerified,
                    @CreateDate, @UpdateDate)", client);
        }

        public async Task Update(Core.Models.Client client)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `Clients`
                SET 
                    `FullName` = @FullName,
                    `Phone` = @Phone,
                    `IsPhoneVerified` = @IsPhoneVerified,
                    `UpdateDate` = @UpdateDate,
                    `DeleteDate` = @DeleteDate
                WHERE `Id` = @Id", client);
        }

        public async Task<Core.Models.Client?> GetByPhoneAsync(string phone)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Clients` WHERE `Phone` = @Phone AND `DeleteDate` IS NULL";
            return await connection.QuerySingleOrDefaultAsync<Core.Models.Client>(sql, new { Phone = phone });
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT COUNT(*) FROM `Clients` WHERE `Phone` = @Phone AND `DeleteDate` IS NULL";
            var count = await connection.QuerySingleAsync<int>(sql, new { Phone = phone });
            return count > 0;
        }

        public async Task<IEnumerable<Core.Models.Client>> GetActiveClientsAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Clients` WHERE `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.Client>(sql);
        }

        public async Task<IEnumerable<Core.Models.Client>> GetVerifiedClientsAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Clients` WHERE `IsPhoneVerified` = true AND `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.Client>(sql);
        }

        public async Task<bool> VerifyPhoneAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "UPDATE `Clients` SET `IsPhoneVerified` = true, `UpdateDate` = @UpdateDate WHERE `Id` = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = clientId, UpdateDate = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public override async Task<IEnumerable<Core.Models.Client>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Clients` WHERE `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.Client>(sql);
        }
    }
}