using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;
using BusinessLogic.Services;
using BusinessLogic.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowtimeController : ControllerBase
    {
        private readonly IShowtimeService _showtimeService;

        public ShowtimeController(IShowtimeService showtimeService)
        {
            _showtimeService = showtimeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllShowtimes([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var showtimes = await _showtimeService.GetAllShowtimes(fromDate, toDate);
            return Ok(new ApiResponse<IEnumerable<ShowtimeResponse>>(showtimes, "Showtimes retrieved successfully"));
        }

        [HttpGet("movie/{movieId}")]
        public async Task<IActionResult> GetShowtimesByMovie(Guid movieId)
        {
            var showtimes = await _showtimeService.GetShowtimesByMovie(movieId);
            return Ok(new ApiResponse<IEnumerable<ShowtimeResponse>>(showtimes, "Showtimes retrieved successfully"));
        }

        [HttpGet("cinema/{cinemaId}")]
        public async Task<IActionResult> GetShowtimesByCinema(Guid cinemaId)
        {
            var showtimes = await _showtimeService.GetShowtimesByCinema(cinemaId);
            return Ok(new ApiResponse<IEnumerable<ShowtimeResponse>>(showtimes, "Showtimes retrieved successfully"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShowtimeById(Guid id)
        {
            var showtime = await _showtimeService.GetShowtimeById(id);
            return Ok(new ApiResponse<ShowtimeResponse>(showtime, "Showtime retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AddShowtime(ShowtimeRequest request)
        {
            var showtime = await _showtimeService.AddShowtime(request);
            return Ok(new ApiResponse<ShowtimeResponse>(showtime, "Showtime added successfully"));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateShowtime(Guid id, ShowtimeRequest request)
        {
            var showtime = await _showtimeService.UpdateShowtime(id, request);
            return Ok(new ApiResponse<ShowtimeResponse>(showtime, "Showtime updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteShowtime(Guid id)
        {
            await _showtimeService.DeleteShowtime(id);
            return Ok(new ApiResponse<object>(null, "Showtime deleted successfully"));
        }
    }
}
