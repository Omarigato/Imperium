using Imperium.Service.DTOs.Address;
using System;
using System.Collections.Generic;

namespace Imperium.Service.DTOs.Client
{
    /// <summary>
    /// DTO информации о клиенте
    /// </summary>
    public class ClientInfoDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsPhoneVerified { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public List<AddressDto> Addresses { get; set; } = new();
        public ClientStatsDto? Stats { get; set; }
    }
}
