using AutoMapper;
using BusinessLogic.DTOs.responses;
using DataAccess.Repositories;

namespace BusinessLogic.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsers();
            return _mapper.Map<IEnumerable<UserResponse>>(users);
        }

        public async Task<UserResponse> ChangeUserRoleAsync(Guid userId, string newRole)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Simple validation for roles, could be expanded
            var validRoles = new[] { "USER", "ADMIN" };
            if (!validRoles.Contains(newRole.ToUpper()))
            {
                throw new Exception("Invalid role specified");
            }

            user.Role = newRole.ToUpper();
            await _userRepository.UpdateUser(user);

            return _mapper.Map<UserResponse>(user);
        }
    }
}
