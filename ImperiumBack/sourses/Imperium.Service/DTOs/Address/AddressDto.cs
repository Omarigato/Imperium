using System;

namespace Imperium.Service.DTOs.Address
{
    /// <summary>
    /// DTO адреса
    /// </summary>
    public class AddressDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string HouseNumber { get; set; } = string.Empty;
        public string? Apartment { get; set; }
        public string? Notes { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreateDate { get; set; }

        public string FullAddress => $"{City}, {Street}, {HouseNumber}" +
                                   (!string.IsNullOrEmpty(Apartment) ? $", кв. {Apartment}" : "");
    }
}
