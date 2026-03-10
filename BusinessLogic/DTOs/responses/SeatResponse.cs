using System;
using DataAccess.Entities;

namespace BusinessLogic.DTOs.responses
{
    public class SeatResponse
    {
        public Guid Id { get; set; }
        public string Row { get; set; }
        public int Number { get; set; }
        public string SeatName { get; set; } // e.g., "A1"
        public string Type { get; set; } // Standard, VIP, Couple
        public Guid AuditoriumId { get; set; }
        public bool IsActive { get; set; }
    }
}
