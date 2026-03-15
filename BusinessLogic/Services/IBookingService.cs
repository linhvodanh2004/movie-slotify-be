using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;

namespace BusinessLogic.Services
{
    public interface IBookingService
    {
        Task<IEnumerable<SeatAvailabilityResponse>> GetSeatAvailability(Guid showtimeId);
        Task<BookingResponse> CreateBooking(Guid userId, BookingRequest request);
        Task<BookingResponse> GetBookingDetails(Guid bookingId);
        Task<IEnumerable<BookingResponse>> GetUserBookings(Guid userId);
        Task ProcessPayment(string transactionId, decimal amount, string content);
    }
}
