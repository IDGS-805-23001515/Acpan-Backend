using AuthenticationAPI.DTO;

namespace AuthenticationAPI.interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync (RegisterRequestDto dto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
        Task LogoutAsync(string userId);
    }
}
