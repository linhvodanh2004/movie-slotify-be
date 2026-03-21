using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Entities;

namespace DataAccess.Repositories
{
    public interface ICinemaRepository
    {
        Task<IEnumerable<Cinema>> GetAllAsync(bool includeInactive = false);
        Task<Cinema> GetByIdAsync(Guid id);
        Task<bool> ExistsByNameAsync(string normalizedName, Guid? excludeId = null);
        Task<bool> HasAuditoriumsAsync(Guid cinemaId);
        Task<bool> HasShowtimesAsync(Guid cinemaId);
        Task<Cinema> AddAsync(Cinema cinema);
        Task UpdateAsync(Cinema cinema);
        Task DeleteAsync(Cinema cinema);
    }
}
