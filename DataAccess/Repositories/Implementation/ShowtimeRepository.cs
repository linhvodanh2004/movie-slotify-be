using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataAccess.Entities;
using DataAccess.Persistence;

namespace DataAccess.Repositories.Implementation
{
    public class ShowtimeRepository : IShowtimeRepository
    {
        private readonly AppDbContext _context;

        public ShowtimeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Showtime>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Auditorium)
                .ThenInclude(a => a.Cinema)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(s => s.StartTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(s => s.StartTime <= toDate.Value);
            }

            return await query.OrderBy(s => s.StartTime).ToListAsync();
        }

        public async Task<IEnumerable<Showtime>> GetByMovieIdAsync(Guid movieId)
        {
            return await _context.Showtimes
                .Where(s => s.MovieId == movieId && s.StartTime >= DateTime.UtcNow)
                .Include(s => s.Auditorium)
                .ThenInclude(a => a.Cinema)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Showtime>> GetByCinemaIdAsync(Guid cinemaId)
        {
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Auditorium)
                .ThenInclude(a => a.Cinema)
                .Where(s => s.Auditorium.CinemaId == cinemaId && s.StartTime >= DateTime.UtcNow)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<Showtime> GetByIdAsync(Guid id)
        {
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Auditorium)
                .ThenInclude(a => a.Cinema)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Showtime> AddAsync(Showtime showtime)
        {
            await _context.Showtimes.AddAsync(showtime);
            await _context.SaveChangesAsync();
            return showtime;
        }

        public async Task UpdateAsync(Showtime showtime)
        {
            _context.Showtimes.Update(showtime);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Showtime showtime)
        {
            _context.Showtimes.Remove(showtime);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasConflictAsync(Guid auditoriumId, DateTime startTime, DateTime endTime, Guid? excludeShowtimeId = null)
        {
            var query = _context.Showtimes
                .Where(s => s.AuditoriumId == auditoriumId && 
                            ((startTime >= s.StartTime && startTime < s.EndTime) || 
                             (endTime > s.StartTime && endTime <= s.EndTime) ||
                             (startTime <= s.StartTime && endTime >= s.EndTime)));

            if (excludeShowtimeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeShowtimeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
