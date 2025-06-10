using System;

namespace Imperium.Service.DTOs.Admin
{
    public class UserActivityDto
    {
        public Guid UserId { get; set; }
        public int LogCount { get; set; }
    }
}
