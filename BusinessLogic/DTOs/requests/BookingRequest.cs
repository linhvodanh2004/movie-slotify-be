using System;
using System.Collections.Generic;

namespace BusinessLogic.DTOs.requests
{
    public class BookingRequest
    {
        public Guid ShowtimeId { get; set; }
        public List<Guid> SeatIds { get; set; }
        public string PaymentMethod { get; set; } = "SePay";
    }
}
