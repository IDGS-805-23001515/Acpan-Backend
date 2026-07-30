using AuthenticationAPI.DTO;
using AuthenticationAPI.interfaces;
using AuthenticationAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        public AuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }
        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "El correo ya existe."
                };
            }
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                NombreCompleto = dto.NombreCompleto,
                FechaRegistro = DateTime.UtcNow
            };
            var result =
                await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(",", result.Errors.Select(e => e.Description))
                };
            }
            await _userManager.AddToRoleAsync(user, "Usuario");
            return new AuthResponseDto
            {
                Success = true,
                Message = "Usuario registrado correctamente."
            };
        }
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user =
                await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Credenciales inválidas."
                };
            }
            var valid =
                await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Credenciales inválidas."
                };
            }
            var token =
                await _tokenService.CreateTokenAsync(user);
            var refresh =
                _tokenService.GenerateRefreshToken();
            user.RefreshToken = refresh;
            user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);
            return new AuthResponseDto
            {
                Success = true,
                Message = "Login correcto.",
                Token = token,
                RefreshToken = refresh,
                Expiration = DateTime.UtcNow.AddMinutes(60)
            };
        }

        public async Task LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            return;

            user.RefreshToken = null;  
            user.RefreshTokenExpiration = null;
            await _userManager.UpdateAsync(user);
        }
    }
}