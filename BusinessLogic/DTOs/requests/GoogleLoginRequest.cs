using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.requests
{
    public class GoogleLoginRequest
    {
        [Required(ErrorMessage = "IdToken không được để trống")]
        public string IdToken { get; set; }
    }
}
