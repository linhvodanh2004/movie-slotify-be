using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Entities;

namespace DataAccess.Repositories
{
    public interface IShowtimeRepository
    {
        Task<IEnumerable<Showtime>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<Showtime>> GetByMovieIdAsync(Guid movieId);
        Task<IEnumerable<Showtime>> GetByCinemaIdAsync(Guid cinemaId);
        Task<Showtime> GetByIdAsync(Guid id);
        Task<Showtime> AddAsync(Showtime showtime);
        Task UpdateAsync(Showtime showtime);
        Task DeleteAsync(Showtime showtime);
        Task<bool> HasConflictAsync(Guid auditoriumId, DateTime startTime, DateTime endTime, Guid? excludeShowtimeId = null);
    }
}
