using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;

namespace BusinessLogic.Services
{
    public interface IAuditoriumService
    {
        Task<IEnumerable<AuditoriumResponse>> GetAuditoriumsByCinema(Guid cinemaId, bool includeInactive = false);
        Task<AuditoriumResponse> GetAuditoriumById(Guid id);
        Task<AuditoriumResponse> AddAuditorium(AuditoriumRequest request);
        Task<AuditoriumResponse> UpdateAuditorium(Guid id, AuditoriumRequest request);
        Task DeleteAuditorium(Guid id);
        Task ActivateAuditorium(Guid id);
        Task DeactivateAuditorium(Guid id);
    }
}
