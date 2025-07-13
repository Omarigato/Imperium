using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Address
{
    /// <summary>
    /// DTO для создания адреса
    /// </summary>
    public class CreateAddressDto
    {
        [StringLength(100)]
        public string? Title { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string HouseNumber { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Apartment { get; set; }

        public string? Notes { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}
