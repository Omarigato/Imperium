using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.Admin
{
    public class AdminUserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminUsers { get; set; }
        public int ManagerUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int UsersCreatedThisMonth { get; set; }
        public string? LastCreatedUser { get; set; }
    }
}
