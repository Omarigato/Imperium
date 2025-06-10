using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Verification
{
    public class VerifyCodeRequest
    {
        [Required]
        [StringLength(255)]
        public string Contact { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
    }
}
