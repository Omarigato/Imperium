using Imperium.Core.Models;
using Imperium.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Data.Repositories
{
    public interface ILogRepository : IBaseRepository<Log>
    {
        Task<IEnumerable<Log>> GetByLevelAsync(string level, int page = 1, int pageSize = 50);
        Task<IEnumerable<Log>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 50);
        Task<IEnumerable<Log>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int page = 1, int pageSize = 50);
        Task<bool> DeleteOldLogsAsync(DateTime cutoffDate);
        Task<int> GetCountByLevelAsync(string level);
    }
}