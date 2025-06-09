using System;

namespace Imperium.Service.DTOs.Admin
{
    public class SystemInfoDto
    {
        public DateTime ServerTime { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public int ProcessorCount { get; set; }
        public long WorkingSet { get; set; }
        public string Version { get; set; } = string.Empty;
        public TimeSpan Uptime { get; set; }
    }
}
