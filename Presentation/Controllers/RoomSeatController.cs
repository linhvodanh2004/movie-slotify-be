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
    public class RoomSeatController : ControllerBase
    {
        private readonly ISeatService _seatService;

        public RoomSeatController(ISeatService seatService)
        {
            _seatService = seatService;
        }

        [HttpGet("auditorium/{auditoriumId}")]
        public async Task<IActionResult> GetSeatsByAuditorium(Guid auditoriumId, [FromQuery] bool includeInactive = false)
        {
            var seats = await _seatService.GetSeatsByAuditorium(auditoriumId, includeInactive);
            return Ok(new ApiResponse<IEnumerable<SeatResponse>>(seats, "Seats retrieved successfully"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSeatById(Guid id)
        {
            var seat = await _seatService.GetSeatById(id);
            return Ok(new ApiResponse<SeatResponse>(seat, "Seat retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AddSeat(SeatRequest request)
        {
            var seat = await _seatService.AddSeat(request);
            return Ok(new ApiResponse<SeatResponse>(seat, "Seat added successfully"));
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AddSeatsBulk(IEnumerable<SeatRequest> requests)
        {
            var seats = await _seatService.AddSeatsBulk(requests);
            return Ok(new ApiResponse<IEnumerable<SeatResponse>>(seats, "Seats added successfully"));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateSeat(Guid id, SeatRequest request)
        {
            var seat = await _seatService.UpdateSeat(id, request);
            return Ok(new ApiResponse<SeatResponse>(seat, "Seat updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteSeat(Guid id)
        {
            await _seatService.DeleteSeat(id);
            return Ok(new ApiResponse<object>(null, "Seat deleted successfully"));
        }

        [HttpPut("{id}/activate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ActivateSeat(Guid id)
        {
            await _seatService.ActivateSeat(id);
            return Ok(new ApiResponse<object>(null, "Seat activated successfully"));
        }

        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeactivateSeat(Guid id)
        {
            await _seatService.DeactivateSeat(id);
            return Ok(new ApiResponse<object>(null, "Seat deactivated successfully"));
        }
    }
}
