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
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, IMapper mapper, IConfiguration configuration, IEmailService emailService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
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

        public async Task<LoginResponse> GoogleLogin(GoogleLoginRequest request, string ipAddress)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Google:ClientId"] }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            }
            catch (Exception)
            {
                throw new UnauthorizedException("Invalid Google token.");
            }

            var user = await _userRepository.GetUserByUsername(payload.Email); // Assuming email is username
            
            if (user == null)
            {
                // Register new user
                user = new User
                {
                    Email = payload.Email,
                    Username = payload.Email, // Ensure username is populated
                    FullName = payload.Name,
                    AvatarUrl = payload.Picture,
                    IsActive = true,
                    Role = "USER",
                    Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()) // Random password
                };
                await _userRepository.AddUser(user);
            }
            else if (string.IsNullOrEmpty(user.AvatarUrl))
            {
                // Optionally update avatar if missing
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
        public async Task<UserResponse> GetMe(Guid userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null) throw new BadRequestException("User not found.");

            return _mapper.Map<UserResponse>(user);
        }

        public async Task<UserResponse> UpdateProfile(Guid userId, UpdateProfileRequest request)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null) throw new BadRequestException("User not found.");

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            
            if (!string.IsNullOrEmpty(request.AvatarUrl))
            {
                user.AvatarUrl = request.AvatarUrl;
            }

            await _userRepository.UpdateUser(user);

            return _mapper.Map<UserResponse>(user);
        }

        public async Task ChangePassword(Guid userId, ChangePasswordRequest request)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null) throw new BadRequestException("User not found.");

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
            {
                throw new BadRequestException("Mật khẩu cũ không chính xác.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepository.UpdateUser(user);
        }

        public async Task ForgotPassword(ForgotPasswordRequest request)
        {
            var user = await _userRepository.GetUserByEmail(request.Email);
            if (user == null)
            {
                return; // Prevent email enumeration attacks
            }

            var token = Guid.NewGuid().ToString("N");
            user.ResetPasswordToken = token;
            user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(15);
            await _userRepository.UpdateUser(user);

            var resetLink = $"http://localhost:3000/reset-password?token={token}";
            var emailBody = $@"
                <p>Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng bấm vào đường dẫn bên dưới để thiết lập mật khẩu mới.</p>
                <p><a href='{resetLink}'>Tạo mật khẩu mới</a></p>
                <p>Đường dẫn có hiệu lực trong vòng 15 phút.</p>
            ";

            await _emailService.SendEmailAsync(user.Email, "MovieSlotify: Đặt lại mật khẩu", emailBody);
        }

        public async Task ResetPassword(ResetPasswordRequest request)
        {
            var user = await _userRepository.GetUserByResetToken(request.Token);
            
            if (user == null || user.ResetPasswordExpiry == null || user.ResetPasswordExpiry < DateTime.UtcNow)
            {
                throw new BadRequestException("Token đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;

            await _userRepository.UpdateUser(user);
        }
    }
}
