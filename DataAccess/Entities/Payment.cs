using System;

namespace DataAccess.Entities
{
    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }

        public Guid BookingId { get; set; }
        public Booking Booking { get; set; }
    }
}
