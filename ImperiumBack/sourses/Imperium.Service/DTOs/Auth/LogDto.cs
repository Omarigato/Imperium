using System;

namespace Imperium.Service.DTOs.Auth
{
    public class LogDto
    {
        public Guid Id { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public Guid? UserId { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
