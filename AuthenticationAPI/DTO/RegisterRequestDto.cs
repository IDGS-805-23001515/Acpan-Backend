using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.DTO
{
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(200)]
        public string NombreCompleto { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
