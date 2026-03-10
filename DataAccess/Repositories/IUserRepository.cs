using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Entities;

namespace DataAccess.Repositories
{
    public interface IUserRepository
    {
        Task<bool> IsEmailExists(string email);
        Task<bool> IsUsernameExists(string username);
        Task<IEnumerable<User>> GetAllUsers();
        Task AddUser(User user);
        Task<User?> GetUserByUsername(string username);
        Task<User?> GetUserById(Guid id);
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserByRefreshToken(string token);
        Task<User?> GetUserByResetToken(string resetToken);
        Task AddRefreshToken(RefreshToken refreshToken);
        Task UpdateRefreshToken(RefreshToken refreshToken);
        Task UpdateUser(User user);
    }
}
