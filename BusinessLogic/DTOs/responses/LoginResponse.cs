using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.responses
{
    public class LoginResponse
    {
        public string Token { get; set; }
        
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string RefreshToken { get; set; }
        public UserResponse User { get; set; }
    }
}
