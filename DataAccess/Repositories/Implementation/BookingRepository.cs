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
                .Include(b => b.User)
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

        /// <summary>
        /// Atomic payment confirmation - no entity tracking, no concurrency exceptions.
        /// Mirrors the NestJS `updateMany` pattern.
        /// </summary>
        public async Task<bool> ConfirmPayment(Guid bookingId, decimal amount, string transactionId)
        {
            // 1. Atomically update Booking status only if it's still Pending
            var affected = await _context.Bookings
                .Where(b => b.Id == bookingId && b.Status == BookingStatus.Pending)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, BookingStatus.Paid));

            if (affected == 0) return false; // already paid or not found

            // 2. Upsert Payment row
            var existing = await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);

            if (existing == null)
            {
                _context.Payments.Add(new Payment
                {
                    BookingId = bookingId,
                    Amount = amount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = "SePay",
                    TransactionId = transactionId
                });
            }
            else
            {
                existing.TransactionId = transactionId;
                existing.PaymentDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
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
