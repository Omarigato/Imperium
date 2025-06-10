using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class VerificationRepository : BaseRepository<Verification>, IVerificationRepository
    {
        public VerificationRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Verifications")
        {
        }

        public async Task<IEnumerable<Verification>> GetByUserIdAsync(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Verifications` 
                WHERE `UserId` = @UserId 
                ORDER BY `CreatedAt` DESC";

            return await connection.QueryAsync<Verification>(sql, new { UserId = userId });
        }

        public async Task<Verification?> GetActiveByUserAndContactAsync(Guid userId, string contact, string type)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Verifications` 
                WHERE `UserId` = @UserId 
                AND `Contact` = @Contact 
                AND `Type` = @Type 
                AND `ExpiresAt` > @Now 
                AND `IsVerified` = false
                ORDER BY `CreatedAt` DESC
                LIMIT 1";

            return await connection.QuerySingleOrDefaultAsync<Verification>(sql, new
            {
                UserId = userId,
                Contact = contact,
                Type = type,
                Now = DateTime.UtcNow
            });
        }

        public async Task<bool> DeleteExpiredAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                DELETE FROM `Verifications` 
                WHERE `ExpiresAt` < @Now";

            var rowsAffected = await connection.ExecuteAsync(sql, new { Now = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Verification>> GetByContactAndTypeAsync(string contact, string type)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Verifications` 
                WHERE `Contact` = @Contact AND `Type` = @Type 
                ORDER BY `CreatedAt` DESC";

            return await connection.QueryAsync<Verification>(sql, new { Contact = contact, Type = type });
        }
    }
}
