using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using Dapper;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class PasswordResetRepository : BaseRepository<PasswordReset>, IPasswordResetRepository
    {
        public PasswordResetRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "PasswordResets")
        {
        }

        public async Task<PasswordReset?> GetActiveByTokenAsync(string resetToken)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `PasswordResets` 
                WHERE `ResetToken` = @ResetToken 
                AND `ExpiresAt` > @Now 
                AND `IsUsed` = false";

            return await connection.QuerySingleOrDefaultAsync<PasswordReset>(sql, new
            {
                ResetToken = resetToken,
                Now = DateTime.UtcNow
            });
        }

        public async Task<PasswordReset?> GetActiveByEmailAsync(string email)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM `PasswordResets` 
                WHERE `Email` = @Email 
                AND `ExpiresAt` > @Now 
                AND `IsUsed` = false
                ORDER BY `CreatedAt` DESC
                LIMIT 1";

            return await connection.QuerySingleOrDefaultAsync<PasswordReset>(sql, new
            {
                Email = email,
                Now = DateTime.UtcNow
            });
        }

        public async Task<bool> MarkAsUsedAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                UPDATE `PasswordResets` 
                SET `IsUsed` = true, `UsedAt` = @UsedAt 
                WHERE `Id` = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                UsedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteExpiredAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                DELETE FROM `PasswordResets` 
                WHERE `ExpiresAt` < @Now OR `IsUsed` = true";

            var rowsAffected = await connection.ExecuteAsync(sql, new { Now = DateTime.UtcNow });
            return rowsAffected > 0;
        }
    }
}
