using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;

namespace BusinessLogic.Services
{
    public interface IAuthService
    {
        Task<UserResponse> Register(UserRegistrationRequest request);
        Task<LoginResponse> Login(UserLoginRequest request);
    }
}
