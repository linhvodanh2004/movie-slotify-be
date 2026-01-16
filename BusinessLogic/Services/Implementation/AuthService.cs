using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace BusinessLogic.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        
        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }
        public async Task<UserResponse> Register(UserRegistrationRequest request)
        {
            if (await _userRepository.IsEmailExists(request.Email))
            {
                throw new Exception("User with this email already exists.");
            }
            if (await _userRepository.IsUsernameExists(request.Username))
            {
                throw new Exception("User with this username already exists.");
            }
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };
            await _userRepository.AddUser(user);
            return new UserResponse
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<LoginResponse> Login(UserLoginRequest request)
        {
            var user = await _userRepository.GetUserByUsername(request.Username);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                throw new Exception("Sai tài khoản hoặc mật khẩu");
            }

            var token = GenerateJwtToken(user);
            
            return new LoginResponse
            {
                Token = token,
                User = new UserResponse
                {
                    Id = user.Id.ToString(),
                    Username = user.Username,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "User")
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
