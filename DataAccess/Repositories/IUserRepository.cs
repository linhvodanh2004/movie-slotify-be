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
        Task AddUser(User user);
        Task<User?> GetUserByUsername(string username);
    }
}
