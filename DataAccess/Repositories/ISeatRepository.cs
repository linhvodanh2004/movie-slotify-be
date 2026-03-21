using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Entities;

namespace DataAccess.Repositories
{
    public interface ISeatRepository
    {
        Task<IEnumerable<Seat>> GetByAuditoriumIdAsync(Guid auditoriumId, bool includeInactive = false);
        Task<Seat> GetByIdAsync(Guid id);
        Task<bool> ExistsAsync(Guid auditoriumId, string row, int number, Guid? excludeId = null);
        Task<bool> HasTicketsAsync(Guid seatId);
        Task<Seat> AddAsync(Seat seat);
        Task AddRangeAsync(IEnumerable<Seat> seats);
        Task UpdateAsync(Seat seat);
        Task DeleteAsync(Seat seat);
    }
}
