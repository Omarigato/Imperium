using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Auth
{
    public class GoogleAuthDto
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}
