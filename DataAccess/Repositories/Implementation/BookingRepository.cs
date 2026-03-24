using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Entities;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementation
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Booking> GetBookingById(Guid id)
        {
            return await _context.Bookings
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Seat)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Auditorium)
                        .ThenInclude(a => a.Cinema)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Booking> GetBookingForPayment(Guid id)
        {
            return await _context.Bookings
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Booking> GetBookingByTransactionId(string transactionId)
        {
            return await _context.Bookings
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Payment.TransactionId == transactionId);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUser(Guid userId)
        {
            return await _context.Bookings
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Movie)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<Booking> AddBooking(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task UpdateBooking(Booking booking)
        {
            if (_context.Entry(booking).State == EntityState.Detached)
            {
                _context.Bookings.Update(booking);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByShowtime(Guid showtimeId)
        {
            return await _context.Tickets
                .Where(t => t.Booking.ShowtimeId == showtimeId && 
                           (t.Booking.Status == BookingStatus.Paid || t.Booking.Status == BookingStatus.Confirmed))
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetPendingBookings()
        {
            // Only get bookings from the last 24 hours to keep it efficient
            var yesterday = DateTime.UtcNow.AddDays(-1);
            return await _context.Bookings
                .Where(b => b.Status == BookingStatus.Pending && b.BookingDate > yesterday)
                .ToListAsync();
        }
    }
}
