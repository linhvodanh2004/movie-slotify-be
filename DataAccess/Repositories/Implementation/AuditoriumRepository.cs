using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataAccess.Entities;
using DataAccess.Persistence;

namespace DataAccess.Repositories.Implementation
{
    public class AuditoriumRepository : IAuditoriumRepository
    {
        private readonly AppDbContext _context;

        public AuditoriumRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Auditorium>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Auditoriums
                .Include(a => a.Cinema)
                .Include(a => a.Seats)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(a => a.IsActive);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Auditorium>> GetByCinemaIdAsync(Guid cinemaId, bool includeInactive = false)
        {
            var query = _context.Auditoriums
                .Where(a => a.CinemaId == cinemaId)
                .Include(a => a.Cinema)
                .Include(a => a.Seats)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(a => a.IsActive);
            }

            return await query.ToListAsync();
        }

        public async Task<Auditorium> GetByIdAsync(Guid id)
        {
            return await _context.Auditoriums
                .Include(a => a.Cinema)
                .Include(a => a.Seats)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Auditorium> AddAsync(Auditorium auditorium)
        {
            await _context.Auditoriums.AddAsync(auditorium);
            await _context.SaveChangesAsync();
            return auditorium;
        }

        public async Task UpdateAsync(Auditorium auditorium)
        {
            _context.Auditoriums.Update(auditorium);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Auditorium auditorium)
        {
            _context.Auditoriums.Remove(auditorium);
            await _context.SaveChangesAsync();
        }
    }
}
