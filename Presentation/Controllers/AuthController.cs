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
