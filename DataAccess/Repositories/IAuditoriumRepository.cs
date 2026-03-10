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
        Task<Auditorium> AddAsync(Auditorium auditorium);
        Task UpdateAsync(Auditorium auditorium);
        Task DeleteAsync(Auditorium auditorium);
    }
}
