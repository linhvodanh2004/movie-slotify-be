using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Entities;

namespace DataAccess.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking> GetBookingById(Guid id);
        Task<Booking> GetBookingByTransactionId(string transactionId);
        Task<IEnumerable<Booking>> GetBookingsByUser(Guid userId);
        Task<Booking> AddBooking(Booking booking);
        Task UpdateBooking(Booking booking);
        Task<IEnumerable<Ticket>> GetTicketsByShowtime(Guid showtimeId);
        Task<IEnumerable<Booking>> GetPendingBookings();
    }
}
