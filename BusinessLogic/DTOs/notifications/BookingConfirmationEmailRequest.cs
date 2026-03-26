using System;
using System.Collections.Generic;

namespace BusinessLogic.DTOs.Notifications
{
    public class BookingConfirmationEmailRequest
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string BookingId { get; set; } = string.Empty;
        public string BookingCode { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public string? MoviePosterUrl { get; set; }
        public string? MovieGenre { get; set; }
        public int DurationMinutes { get; set; }
        public string CinemaName { get; set; } = string.Empty;
        public string? CinemaAddress { get; set; }
        public string AuditoriumName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public List<BookingConfirmationTicketItem> Tickets { get; set; } = new();
    }

    public class BookingConfirmationTicketItem
    {
        public string SeatLabel { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
