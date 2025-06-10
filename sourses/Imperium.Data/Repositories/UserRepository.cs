using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Users")
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM \"Users\" WHERE \"Email\" = @Email AND \"DeletedAt\" IS NULL";
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User?> GetByPhoneAsync(string phone)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM \"Users\" WHERE \"Phone\" = @Phone AND \"DeletedAt\" IS NULL";
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Phone = phone });
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT COUNT(*) FROM \"Users\" WHERE \"Email\" = @Email AND \"DeletedAt\" IS NULL";
            var count = await connection.QuerySingleAsync<int>(sql, new { Email = email });
            return count > 0;
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT COUNT(*) FROM \"Users\" WHERE \"Phone\" = @Phone AND \"DeletedAt\" IS NULL";
            var count = await connection.QuerySingleAsync<int>(sql, new { Phone = phone });
            return count > 0;
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM \"Users\" WHERE \"DeletedAt\" IS NULL ORDER BY \"CreatedAt\" DESC";
            return await connection.QueryAsync<User>(sql);
        }

        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM \"Users\" WHERE \"DeletedAt\" IS NULL ORDER BY \"CreatedAt\" DESC";
            return await connection.QueryAsync<User>(sql);
        }
    }
}