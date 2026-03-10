using System;
using System.ComponentModel.DataAnnotations;
using DataAccess.Entities;

namespace BusinessLogic.DTOs.requests
{
    public class SeatRequest
    {
        [Required]
        [MaxLength(5)]
        public string Row { get; set; }

        [Required]
        [Range(1, 100)]
        public int Number { get; set; }

        [Required]
        public SeatType Type { get; set; }

        [Required]
        public Guid AuditoriumId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
