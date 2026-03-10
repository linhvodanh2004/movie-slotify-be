using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.requests
{
    public class CinemaRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(200)]
        public string Address { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
