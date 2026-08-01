using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.DTO
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Roles { get; set; } = string.Empty;
    }
}