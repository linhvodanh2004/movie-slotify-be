using System;
using System.Collections.Generic;

namespace BusinessLogic.DTOs.responses
{
    public class BookingResponse
    {
        public Guid Id { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        
        public string MovieTitle { get; set; }
        public string CinemaName { get; set; }
        public string AuditoriumName { get; set; }
        public DateTime StartTime { get; set; }
        
        public List<TicketResponse> Tickets { get; set; }
    }

    public class TicketResponse
    {
        public Guid Id { get; set; }
        public string SeatRow { get; set; }
        public int SeatNumber { get; set; }
        public string SeatType { get; set; }
        public decimal Price { get; set; }
    }
}
