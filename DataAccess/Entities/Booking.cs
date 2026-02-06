using System;
using System.Collections.Generic;

namespace DataAccess.Entities
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Paid
    }
    public class Booking
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        
        // Status: Pending, Confirmed, Cancelled, Paid
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid ShowtimeId { get; set; }
        public Showtime Showtime { get; set; }

        public ICollection<Ticket> Tickets { get; set; }
        public Payment Payment { get; set; }
    }
}
