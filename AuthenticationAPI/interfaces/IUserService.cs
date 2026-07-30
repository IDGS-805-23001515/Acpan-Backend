using AuthenticationAPI.DTO;

namespace AuthenticationAPI.interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(string id);
        Task<bool> DeleteAsync(string id); 
        Task<bool> AssignRoleAsync(string id, string role);
    }
}
