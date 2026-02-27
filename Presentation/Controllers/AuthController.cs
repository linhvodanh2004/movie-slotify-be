using BusinessLogic.Services;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;
using BusinessLogic.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationRequest request)
        {
            var userResponse = await _authService.Register(request);
            return Ok(new ApiResponse<UserResponse>(userResponse, "Registration successful"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var loginResponse = await _authService.Login(request, IpAddress());
            SetTokenCookie(loginResponse.RefreshToken);
            loginResponse.RefreshToken = null; // Don't return in body
            return Ok(new ApiResponse<LoginResponse>(loginResponse, "Login successful"));
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var loginResponse = await _authService.GoogleLogin(request, IpAddress());
            SetTokenCookie(loginResponse.RefreshToken);
            loginResponse.RefreshToken = null; // Hide from body to avoid leak since it's HttpOnly
            return Ok(new ApiResponse<LoginResponse>(loginResponse, "Google login successful"));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new ApiResponse<object>(false, "Token is required", 400));

            var response = await _authService.RefreshToken(refreshToken, IpAddress());
            SetTokenCookie(response.RefreshToken);
            response.RefreshToken = null;
            return Ok(new ApiResponse<LoginResponse>(response, "Token refreshed"));
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new ApiResponse<object>(false, "Token is required", 400));

            await _authService.RevokeToken(refreshToken, IpAddress());
            Response.Cookies.Delete("refreshToken");
            return Ok(new ApiResponse<object>(null, "Token revoked"));
        }

        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized(new ApiResponse<object>(null, "Invalid token claims."));

            var user = await _authService.GetMe(userId);
            return Ok(new ApiResponse<UserResponse>(user, "User details fetched successfully."));
        }

        [HttpPut("profile")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized(new ApiResponse<object>(null, "Invalid token claims."));

            var user = await _authService.UpdateProfile(userId, request);
            return Ok(new ApiResponse<UserResponse>(user, "Profile updated successfully."));
        }

        [HttpPost("change-password")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized(new ApiResponse<object>(null, "Invalid token claims."));

            await _authService.ChangePassword(userId, request);
            return Ok(new ApiResponse<object>(null, "Password changed successfully."));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            await _authService.ForgotPassword(request);
            return Ok(new ApiResponse<object>(null, "If the email is registered, a reset link has been sent."));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            await _authService.ResetPassword(request);
            return Ok(new ApiResponse<object>(null, "Password reset successfully."));
        }

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.None,
                Secure = true
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string IpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }
    }
}
