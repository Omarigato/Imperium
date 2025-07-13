using Dapper;
using System;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.Verification
{
    public class VerificationRepository : BaseRepository<Core.Models.Verification>, IVerificationRepository
    {
        public VerificationRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Verifications")
        {
        }

        public async Task Insert(Core.Models.Verification verification)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `Verifications` (
                    `Id`, `Phone`, `OtpCode`, `ExpiresAt`, `AttemptCount`, 
                    `UsedDate`, `CreateDate`)
                VALUES (
                    @Id, @Phone, @OtpCode, @ExpiresAt, @AttemptCount,
                    @UsedDate, @CreateDate)", verification);
        }

        public async Task Update(Core.Models.Verification verification)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `Verifications`
                SET 
                    `Phone` = @Phone,
                    `OtpCode` = @OtpCode,
                    `ExpiresAt` = @ExpiresAt,
                    `AttemptCount` = @AttemptCount,
                    `UsedDate` = @UsedDate
                WHERE `Id` = @Id", verification);
        }

        public async Task<IEnumerable<Core.Models.Verification>> GetByClientIdAsync(Guid clientId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Verifications` 
                WHERE `ClientId` = @ClientId 
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Verification>(sql, new { ClientId = clientId });
        }

        public async Task<Core.Models.Verification?> GetActiveByClientAndContactAsync(Guid clientId, string contact, string type)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Verifications` 
                WHERE `ClientId` = @ClientId 
                AND `Contact` = @Contact 
                AND `Type` = @Type 
                AND `ExpiresAt` > @Now 
                AND `UsedDate` IS NULL
                ORDER BY `CreateDate` DESC
                LIMIT 1";

            return await connection.QuerySingleOrDefaultAsync<Core.Models.Verification>(sql, new
            {
                ClientId = clientId,
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

        public async Task<IEnumerable<Core.Models.Verification>> GetByContactAndTypeAsync(string contact, string type)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `Verifications` 
                WHERE `Contact` = @Contact AND `Type` = @Type 
                ORDER BY `CreateDate` DESC";

            return await connection.QueryAsync<Core.Models.Verification>(sql, new { Contact = contact, Type = type });
        }

        public async Task<bool> MarkAsUsedAsync(Guid verificationId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                UPDATE `Verifications` 
                SET `UsedDate` = @UsedDate 
                WHERE `Id` = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = verificationId,
                UsedDate = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }
    }
}