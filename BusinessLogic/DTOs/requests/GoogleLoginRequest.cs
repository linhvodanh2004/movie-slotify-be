using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.requests
{
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; }
    }
}
