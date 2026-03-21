using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataAccess.Entities;
using DataAccess.Persistence;

namespace DataAccess.Repositories.Implementation
{
    public class CinemaRepository : ICinemaRepository
    {
        private readonly AppDbContext _context;

        public CinemaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cinema>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Cinemas.Include(c => c.Auditoriums).AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            return await query.ToListAsync();
        }

        public async Task<Cinema> GetByIdAsync(Guid id)
        {
            return await _context.Cinemas
                .Include(c => c.Auditoriums)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> ExistsByNameAsync(string normalizedName, Guid? excludeId = null)
        {
            return await _context.Cinemas
                .IgnoreQueryFilters()
                .AnyAsync(c =>
                    (excludeId == null || c.Id != excludeId.Value) &&
                    c.Name.Trim().ToUpper() == normalizedName);
        }

        public async Task<bool> HasAuditoriumsAsync(Guid cinemaId)
        {
            return await _context.Auditoriums
                .IgnoreQueryFilters()
                .AnyAsync(a => a.CinemaId == cinemaId);
        }

        public async Task<bool> HasShowtimesAsync(Guid cinemaId)
        {
            return await _context.Showtimes
                .IgnoreQueryFilters()
                .Include(s => s.Auditorium)
                .AnyAsync(s => s.Auditorium.CinemaId == cinemaId);
        }

        public async Task<Cinema> AddAsync(Cinema cinema)
        {
            await _context.Cinemas.AddAsync(cinema);
            await _context.SaveChangesAsync();
            return cinema;
        }

        public async Task UpdateAsync(Cinema cinema)
        {
            _context.Cinemas.Update(cinema);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Cinema cinema)
        {
            _context.Cinemas.Remove(cinema);
            await _context.SaveChangesAsync();
        }
    }
}
