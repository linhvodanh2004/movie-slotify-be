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
    public class AuditoriumController : ControllerBase
    {
        private readonly IAuditoriumService _auditoriumService;

        public AuditoriumController(IAuditoriumService auditoriumService)
        {
            _auditoriumService = auditoriumService;
        }

        [HttpGet("cinema/{cinemaId}")]
        public async Task<IActionResult> GetAuditoriumsByCinema(Guid cinemaId, [FromQuery] bool includeInactive = false)
        {
            var auditoriums = await _auditoriumService.GetAuditoriumsByCinema(cinemaId, includeInactive);
            return Ok(new ApiResponse<IEnumerable<AuditoriumResponse>>(auditoriums, "Auditoriums retrieved successfully"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuditoriumById(Guid id)
        {
            var auditorium = await _auditoriumService.GetAuditoriumById(id);
            return Ok(new ApiResponse<AuditoriumResponse>(auditorium, "Auditorium retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AddAuditorium(AuditoriumRequest request)
        {
            var auditorium = await _auditoriumService.AddAuditorium(request);
            return Ok(new ApiResponse<AuditoriumResponse>(auditorium, "Auditorium added successfully"));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateAuditorium(Guid id, AuditoriumRequest request)
        {
            var auditorium = await _auditoriumService.UpdateAuditorium(id, request);
            return Ok(new ApiResponse<AuditoriumResponse>(auditorium, "Auditorium updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteAuditorium(Guid id)
        {
            await _auditoriumService.DeleteAuditorium(id);
            return Ok(new ApiResponse<object>(null, "Auditorium deleted successfully"));
        }

        [HttpPut("{id}/activate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ActivateAuditorium(Guid id)
        {
            await _auditoriumService.ActivateAuditorium(id);
            return Ok(new ApiResponse<object>(null, "Auditorium activated successfully"));
        }

        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeactivateAuditorium(Guid id)
        {
            await _auditoriumService.DeactivateAuditorium(id);
            return Ok(new ApiResponse<object>(null, "Auditorium deactivated successfully"));
        }
    }
}
