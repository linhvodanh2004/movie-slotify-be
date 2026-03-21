using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataAccess.Entities;
using DataAccess.Persistence;

namespace DataAccess.Repositories.Implementation
{
    public class SeatRepository : ISeatRepository
    {
        private readonly AppDbContext _context;

        public SeatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Seat>> GetByAuditoriumIdAsync(Guid auditoriumId, bool includeInactive = false)
        {
            var query = _context.Seats
                .Where(s => s.AuditoriumId == auditoriumId)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive);
            }

            return await query.OrderBy(s => s.Row).ThenBy(s => s.Number).ToListAsync();
        }

        public async Task<Seat> GetByIdAsync(Guid id)
        {
            return await _context.Seats
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> ExistsAsync(Guid auditoriumId, string row, int number, Guid? excludeId = null)
        {
            var normalizedRow = row.Trim().ToUpper();

            return await _context.Seats
                .IgnoreQueryFilters()
                .AnyAsync(s =>
                    s.AuditoriumId == auditoriumId &&
                    s.Row.Trim().ToUpper() == normalizedRow &&
                    s.Number == number &&
                    (excludeId == null || s.Id != excludeId.Value));
        }

        public async Task<bool> HasTicketsAsync(Guid seatId)
        {
            return await _context.Tickets
                .IgnoreQueryFilters()
                .AnyAsync(t => t.SeatId == seatId);
        }

        public async Task<Seat> AddAsync(Seat seat)
        {
            await _context.Seats.AddAsync(seat);
            await _context.SaveChangesAsync();
            return seat;
        }

        public async Task AddRangeAsync(IEnumerable<Seat> seats)
        {
            await _context.Seats.AddRangeAsync(seats);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Seat seat)
        {
            _context.Seats.Update(seat);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Seat seat)
        {
            _context.Seats.Remove(seat);
            await _context.SaveChangesAsync();
        }
    }
}
