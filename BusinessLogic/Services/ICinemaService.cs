using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;

namespace BusinessLogic.Services
{
    public interface ICinemaService
    {
        Task<IEnumerable<CinemaResponse>> GetAllCinemas(bool includeInactive = false);
        Task<CinemaResponse> GetCinemaById(Guid id);
        Task<CinemaResponse> AddCinema(CinemaRequest request);
        Task<CinemaResponse> UpdateCinema(Guid id, CinemaRequest request);
        Task DeleteCinema(Guid id);
        Task ActivateCinema(Guid id);
        Task DeactivateCinema(Guid id);
    }
}
