using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.requests
{
    public class ShowtimeRequest
    {
        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        [Range(0, 10000000)]
        public decimal StandardPrice { get; set; }

        [Required]
        [Range(0, 10000000)]
        public decimal VipPrice { get; set; }

        [Required]
        [Range(0, 10000000)]
        public decimal CouplePrice { get; set; }

        [Required]
        public Guid MovieId { get; set; }

        [Required]
        public Guid AuditoriumId { get; set; }
    }
}

