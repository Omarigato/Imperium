using Dapper;
using System.Threading.Tasks;
using Imperium.Data.Connections;
using System.Collections.Generic;
using Imperium.Data.Repositories.Base;

namespace Imperium.Data.Repositories.User
{
    public class UserRepository : BaseRepository<Core.Models.User>, IUserRepository
    {
        public UserRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Users")
        {
        }

        public async Task Insert(Core.Models.User user)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO `Users` (
                    `Id`, `FullName`, `Phone`, `Login`, `Password`, `Role`, 
                    `AuthorId`, `CreateDate`)
                VALUES (
                    @Id, @FullName, @Phone, @Login, @Password, @Role,
                    @AuthorId, @CreateDate)", user);
        }

        public async Task Update(Core.Models.User user)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                UPDATE `Users`
                SET 
                    `FullName` = @FullName,
                    `Phone` = @Phone,
                    `Login` = @Login,
                    `Password` = @Password,
                    `Role` = @Role,
                    `DeleteDate` = @DeleteDate
                WHERE `Id` = @Id", user);
        }

        public async Task<Core.Models.User?> GetByLoginAsync(string login)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Users` WHERE `Login` = @Login AND `DeleteDate` IS NULL";
            return await connection.QuerySingleOrDefaultAsync<Core.Models.User>(sql, new { Login = login });
        }

        public async Task<Core.Models.User?> GetByPhoneAsync(string phone)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Users` WHERE `Phone` = @Phone AND `DeleteDate` IS NULL";
            return await connection.QuerySingleOrDefaultAsync<Core.Models.User>(sql, new { Phone = phone });
        }

        public async Task<bool> LoginExistsAsync(string login)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT COUNT(*) FROM `Users` WHERE `Login` = @Login AND `DeleteDate` IS NULL";
            var count = await connection.QuerySingleAsync<int>(sql, new { Login = login });
            return count > 0;
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT COUNT(*) FROM `Users` WHERE `Phone` = @Phone AND `DeleteDate` IS NULL";
            var count = await connection.QuerySingleAsync<int>(sql, new { Phone = phone });
            return count > 0;
        }

        public async Task<IEnumerable<Core.Models.User>> GetActiveUsersAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Users` WHERE `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.User>(sql);
        }

        public async Task<IEnumerable<Core.Models.User>> GetByRoleAsync(string role)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Users` WHERE `Role` = @Role AND `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.User>(sql, new { Role = role });
        }

        public override async Task<IEnumerable<Core.Models.User>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = "SELECT * FROM `Users` WHERE `DeleteDate` IS NULL ORDER BY `CreateDate` DESC";
            return await connection.QueryAsync<Core.Models.User>(sql);
        }
    }
}