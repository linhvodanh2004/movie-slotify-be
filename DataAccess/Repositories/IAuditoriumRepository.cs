using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Entities;

namespace DataAccess.Repositories
{
    public interface IAuditoriumRepository
    {
        Task<IEnumerable<Auditorium>> GetAllAsync(bool includeInactive = false);
        Task<IEnumerable<Auditorium>> GetByCinemaIdAsync(Guid cinemaId, bool includeInactive = false);
        Task<Auditorium> GetByIdAsync(Guid id);
        Task<bool> ExistsByNameAsync(Guid cinemaId, string normalizedName, Guid? excludeId = null);
        Task<bool> HasSeatsAsync(Guid auditoriumId);
        Task<bool> HasShowtimesAsync(Guid auditoriumId);
        Task<Auditorium> AddAsync(Auditorium auditorium);
        Task UpdateAsync(Auditorium auditorium);
        Task DeleteAsync(Auditorium auditorium);
    }
}
