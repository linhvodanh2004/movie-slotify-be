using System;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;
using BusinessLogic.Exceptions;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, IMapper mapper)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<UserResponse> Register(UserRegistrationRequest request)
        {
            if (await _userRepository.IsEmailExists(request.Email))
            {
                throw new BadRequestException("User with this email already exists.");
            }
            if (await _userRepository.IsUsernameExists(request.Username))
            {
                throw new BadRequestException("User with this username already exists.");
            }

            var user = _mapper.Map<User>(request);
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            
            await _userRepository.AddUser(user);
            
            return _mapper.Map<UserResponse>(user);
        }

        public async Task<LoginResponse> Login(UserLoginRequest request, string ipAddress)
        {
            var user = await _userRepository.GetUserByUsername(request.Username);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                throw new UnauthorizedException("Sai tài khoản hoặc mật khẩu");
            }

            var token = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            refreshToken.UserId = user.Id;
            refreshToken.CreatedByIp = ipAddress;
            
            await _userRepository.AddRefreshToken(refreshToken);
            
            var response = _mapper.Map<UserResponse>(user);
            
            return new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken.Token,
                User = response
            };
        }

        public async Task<LoginResponse> RefreshToken(string token, string ipAddress)
        {
            var user = await _userRepository.GetUserByRefreshToken(token);
            if (user == null) throw new UnauthorizedException("Token không hợp lệ");

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
            {
               throw new UnauthorizedException("Token không hợp lệ");
            }

            // Revoke current refresh token
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReasonRevoked = "Đã thay thế bằng token mới";
            
            // Generate new tokens
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            newRefreshToken.UserId = user.Id;
            newRefreshToken.CreatedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            
            // Save changes
            await _userRepository.UpdateRefreshToken(refreshToken);
            await _userRepository.AddRefreshToken(newRefreshToken);

            var jwtToken = _tokenService.GenerateAccessToken(user);

            return new LoginResponse
            {
                Token = jwtToken,
                RefreshToken = newRefreshToken.Token,
                User = _mapper.Map<UserResponse>(user)
            };
        }

        public async Task RevokeToken(string token, string ipAddress)
        {
             var user = await _userRepository.GetUserByRefreshToken(token);
             if (user == null) throw new BadRequestException("Token không hợp lệ");
             
             var refreshToken = user.RefreshTokens.Single(x => x.Token == token);
             
             if (!refreshToken.IsActive) throw new BadRequestException("Token không hợp lệ");

             refreshToken.Revoked = DateTime.UtcNow;
             refreshToken.RevokedByIp = ipAddress;
             refreshToken.ReasonRevoked = "Đã thu hồi token";
             
             await _userRepository.UpdateRefreshToken(refreshToken);
        }
    }
}
