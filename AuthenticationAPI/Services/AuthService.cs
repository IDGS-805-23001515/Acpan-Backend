using AuthenticationAPI.DTO;
using AuthenticationAPI.interfaces;
using AuthenticationAPI.Models; // <-- Importamos tu carpeta de modelos
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationAPI.Services
{
    public class AuthService : IAuthService
    {
        // Cambiamos IdentityUser por tu ApplicationUser
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            var userExists = await _userManager.FindByEmailAsync(dto.Correo);
            if (userExists != null)
                return new AuthResponseDto { Success = false, Message = "El correo ya está registrado." };

            string tempPassword = GenerateRandomPassword();

            // Usamos tu clase ApplicationUser
            var newUser = new ApplicationUser
            {
                UserName = dto.Correo,
                Email = dto.Correo
            };

            var result = await _userManager.CreateAsync(newUser, tempPassword);

            if (!result.Succeeded)
                return new AuthResponseDto { Success = false, Message = "Error al crear el usuario." };

            if (await _roleManager.RoleExistsAsync(dto.Rol))
            {
                await _userManager.AddToRoleAsync(newUser, dto.Rol);
            }

            return new AuthResponseDto
            {
                Success = true,
                Message = $"Usuario registrado exitosamente. Contraseña temporal: {tempPassword}",
                Token = null,
                RefreshToken = null
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Correo);
            if (user == null)
            {
                return new AuthResponseDto { Success = false, Message = "Credenciales incorrectas." };
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
            {
                return new AuthResponseDto { Success = false, Message = "Credenciales incorrectas." };
            }

            // <-- AGREGAR ESTA VALIDACIÓN DE ESTATUS / BLOQUEO -->
            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now)
            {
                return new AuthResponseDto { Success = false, Message = "Tu cuenta se encuentra inactiva. Contacta al administrador." };
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = CreateToken(authClaims);
            var expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationInMinutes"]));

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login exitoso",
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration,
                RefreshToken = GenerateRefreshToken()
            };
        }

        public async Task LogoutAsync(string userId)
        {
            await Task.CompletedTask;
        }

        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationInMinutes"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private string GenerateRandomPassword()
        {
            var random = new Random();
            int randomNumber = random.Next(1000, 9999);
            return $"Acpan{randomNumber}!Temp";
        }
    }
}