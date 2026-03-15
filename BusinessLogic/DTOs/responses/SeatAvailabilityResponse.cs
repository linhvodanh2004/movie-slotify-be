using System;

namespace BusinessLogic.DTOs.responses
{
    public class SeatAvailabilityResponse
    {
        public Guid SeatId { get; set; }
        public string Row { get; set; }
        public int Number { get; set; }
        public string Type { get; set; }
        public bool IsAvailable { get; set; }
        public decimal Price { get; set; }
    }
}
