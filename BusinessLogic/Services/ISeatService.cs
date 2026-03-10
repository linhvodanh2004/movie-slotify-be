using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;

namespace BusinessLogic.Services
{
    public interface ISeatService
    {
        Task<IEnumerable<SeatResponse>> GetSeatsByAuditorium(Guid auditoriumId, bool includeInactive = false);
        Task<SeatResponse> GetSeatById(Guid id);
        Task<SeatResponse> AddSeat(SeatRequest request);
        Task<IEnumerable<SeatResponse>> AddSeatsBulk(IEnumerable<SeatRequest> requests);
        Task<SeatResponse> UpdateSeat(Guid id, SeatRequest request);
        Task DeleteSeat(Guid id);
        Task ActivateSeat(Guid id);
        Task DeactivateSeat(Guid id);
    }
}
