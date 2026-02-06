using System;

namespace DataAccess.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Price { get; set; }

        public Guid BookingId { get; set; }
        public Booking Booking { get; set; }

        public Guid SeatId { get; set; }
        public Seat Seat { get; set; }
    }
}
