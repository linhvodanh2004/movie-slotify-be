using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.requests
{
    public class AuditoriumRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        public Guid CinemaId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
