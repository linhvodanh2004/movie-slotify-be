using BusinessLogic.DTOs.responses;

namespace BusinessLogic.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
        Task<UserResponse> ChangeUserRoleAsync(Guid userId, string newRole);
    }
}
