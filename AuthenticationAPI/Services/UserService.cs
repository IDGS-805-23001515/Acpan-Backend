using AuthenticationAPI.DTO;
using AuthenticationAPI.interfaces;
using AuthenticationAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new List<UserDto>();
        foreach (var user in users)
        {
            result.Add(new UserDto
            {
                Id = user.Id,
                NombreCompleto = user.NombreCompleto,
                Email = user.Email!,
                FechaRegistro = user.FechaRegistro,
                Roles = await _userManager.GetRolesAsync(user)
            });
        }
        return result;
    }
    public async Task<UserDto?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return null;
        return new UserDto
        {
            Id = user.Id,
            NombreCompleto = user.NombreCompleto,
            Email = user.Email!,
            FechaRegistro = user.FechaRegistro,
            Roles = await _userManager.GetRolesAsync(user)
        };
    }
    public async Task<bool> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return false;
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
    public async Task<bool> AssignRoleAsync(string id, string role)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return false;
        if (!await _userManager.IsInRoleAsync(user, role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }
        return true;
    }
}