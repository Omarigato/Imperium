using System;
using System.Collections.Generic;

namespace Imperium.Service.DTOs.Admin
{
    public class LogStatsDto
    {
        public int TotalLogs { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }
        public int DebugCount { get; set; }
        public DateTime? LastErrorDate { get; set; }
        public List<UserActivityDto> MostActiveUsers { get; set; } = new();
    }
}
