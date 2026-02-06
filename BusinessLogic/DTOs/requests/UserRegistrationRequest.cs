using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.requests
{
    public class UserRegistrationRequest
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string FullName { get; set; }

        
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
