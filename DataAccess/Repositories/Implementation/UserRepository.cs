using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Entities;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddUser(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsEmailExists(string email)
        {
            bool IsEmailExists = await _context.Users.AnyAsync(u => u.Email == email);
            return IsEmailExists;
        }

        public async Task<bool> IsUsernameExists(string username)
        {
            bool isUsernameExists = await _context.Users.AnyAsync(u => u.Username == username);
            return isUsernameExists;
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
