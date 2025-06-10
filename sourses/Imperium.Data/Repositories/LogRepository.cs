using Dapper;
using Imperium.Core.Models;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public class LogRepository : BaseRepository<Log>, ILogRepository
    {
        public LogRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Logs")
        {
        }

        public async Task<IEnumerable<Log>> GetByLevelAsync(string level, int page = 1, int pageSize = 50)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var offset = (page - 1) * pageSize;
            var sql = @"
                SELECT * FROM ""Logs"" 
                WHERE ""Level"" = @Level 
                ORDER BY ""CreatedAt"" DESC 
                LIMIT @PageSize OFFSET @Offset";

            return await connection.QueryAsync<Log>(sql, new { Level = level, PageSize = pageSize, Offset = offset });
        }

        public async Task<IEnumerable<Log>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 50)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var offset = (page - 1) * pageSize;
            var sql = @"
                SELECT * FROM ""Logs"" 
                WHERE ""UserId"" = @UserId 
                ORDER BY ""CreatedAt"" DESC 
                LIMIT @PageSize OFFSET @Offset";

            return await connection.QueryAsync<Log>(sql, new { UserId = userId, PageSize = pageSize, Offset = offset });
        }

        public async Task<IEnumerable<Log>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int page = 1, int pageSize = 50)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var offset = (page - 1) * pageSize;
            var sql = @"
                SELECT * FROM ""Logs"" 
                WHERE ""CreatedAt"" >= @FromDate AND ""CreatedAt"" <= @ToDate 
                ORDER BY ""CreatedAt"" DESC 
                LIMIT @PageSize OFFSET @Offset";

            return await connection.QueryAsync<Log>(sql, new
            {
                FromDate = fromDate,
                ToDate = toDate,
                PageSize = pageSize,
                Offset = offset
            });
        }

        public async Task<bool> DeleteOldLogsAsync(DateTime cutoffDate)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                DELETE FROM ""Logs"" 
                WHERE ""CreatedAt"" < @CutoffDate";

            var rowsAffected = await connection.ExecuteAsync(sql, new { CutoffDate = cutoffDate });
            return rowsAffected > 0;
        }

        public async Task<int> GetCountByLevelAsync(string level)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT COUNT(*) FROM ""Logs"" 
                WHERE ""Level"" = @Level";

            return await connection.QuerySingleAsync<int>(sql, new { Level = level });
        }

        public override async Task<IEnumerable<Log>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var sql = @"
                SELECT * FROM ""Logs"" 
                ORDER BY ""CreatedAt"" DESC 
                LIMIT 1000"; // Ограничиваем количество для производительности

            return await connection.QueryAsync<Log>(sql);
        }
    }
}