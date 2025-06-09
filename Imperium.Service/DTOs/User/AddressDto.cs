using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.User
{
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
        public DateTime CreatedAt { get; set; }
    }
}