using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Verification
{
    public class SendCodeRequest
    {
        [Required]
        [StringLength(255)]
        public string Contact { get; set; } = string.Empty;
    }
}
