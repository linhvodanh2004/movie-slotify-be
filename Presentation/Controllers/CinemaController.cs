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
    public class CinemaController : ControllerBase
    {
        private readonly ICinemaService _cinemaService;

        public CinemaController(ICinemaService cinemaService)
        {
            _cinemaService = cinemaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCinemas([FromQuery] bool includeInactive = false)
        {
            var cinemas = await _cinemaService.GetAllCinemas(includeInactive);
            return Ok(new ApiResponse<IEnumerable<CinemaResponse>>(cinemas, "Cinemas retrieved successfully"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCinemaById(Guid id)
        {
            var cinema = await _cinemaService.GetCinemaById(id);
            return Ok(new ApiResponse<CinemaResponse>(cinema, "Cinema retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AddCinema(CinemaRequest request)
        {
            var cinema = await _cinemaService.AddCinema(request);
            return Ok(new ApiResponse<CinemaResponse>(cinema, "Cinema added successfully"));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateCinema(Guid id, CinemaRequest request)
        {
            var cinema = await _cinemaService.UpdateCinema(id, request);
            return Ok(new ApiResponse<CinemaResponse>(cinema, "Cinema updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteCinema(Guid id)
        {
            await _cinemaService.DeleteCinema(id);
            return Ok(new ApiResponse<object>(null, "Cinema deleted successfully"));
        }

        [HttpPut("{id}/activate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ActivateCinema(Guid id)
        {
            await _cinemaService.ActivateCinema(id);
            return Ok(new ApiResponse<object>(null, "Cinema activated successfully"));
        }

        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeactivateCinema(Guid id)
        {
            await _cinemaService.DeactivateCinema(id);
            return Ok(new ApiResponse<object>(null, "Cinema deactivated successfully"));
        }
    }
}
