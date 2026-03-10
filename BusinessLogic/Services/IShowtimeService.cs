using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;

namespace BusinessLogic.Services
{
    public interface IShowtimeService
    {
        Task<IEnumerable<ShowtimeResponse>> GetAllShowtimes(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<ShowtimeResponse>> GetShowtimesByMovie(Guid movieId);
        Task<IEnumerable<ShowtimeResponse>> GetShowtimesByCinema(Guid cinemaId);
        Task<ShowtimeResponse> GetShowtimeById(Guid id);
        Task<ShowtimeResponse> AddShowtime(ShowtimeRequest request);
        Task<ShowtimeResponse> UpdateShowtime(Guid id, ShowtimeRequest request);
        Task DeleteShowtime(Guid id);
    }
}
