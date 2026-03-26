using System;
using System.Collections.Generic;
using System.Security.Claims;
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
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("available-seats/{showtimeId}")]
        public async Task<IActionResult> GetAvailableSeats(Guid showtimeId)
        {
            var seats = await _bookingService.GetSeatAvailability(showtimeId);
            return Ok(new ApiResponse<IEnumerable<SeatAvailabilityResponse>>(seats, "Available seats retrieved successfully"));
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateBooking(BookingRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out Guid userId))
                return Unauthorized(new ApiResponse<object>(null, "Người dùng không hợp lệ."));

            var booking = await _bookingService.CreateBooking(userId, request);
            return Ok(new ApiResponse<BookingResponse>(booking, "Đơn đặt vé đã được tạo. Vui lòng thanh toán."));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetBookingDetails(Guid id)
        {
            var booking = await _bookingService.GetBookingDetails(id);
            return Ok(new ApiResponse<BookingResponse>(booking, "Chi tiết đơn hàng."));
        }

        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<IActionResult> GetMyBookings()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out Guid userId))
                return Unauthorized(new ApiResponse<object>(null, "Người dùng không hợp lệ."));

            var bookings = await _bookingService.GetUserBookings(userId);
            return Ok(new ApiResponse<IEnumerable<BookingResponse>>(bookings, "Danh sách đơn hàng."));
        }

        [HttpPost("{id}/send-confirmation-email")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> SendConfirmationEmail(Guid id)
        {
            await _bookingService.SendBookingConfirmationEmailForBooking(id);
            return Ok(new ApiResponse<object>(null, "Đã gửi email xác nhận đặt vé."));
        }

        [HttpPost("{id}/send-my-confirmation-email")]
        [Authorize]
        public async Task<IActionResult> SendMyConfirmationEmail(Guid id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out Guid userId))
                return Unauthorized(new ApiResponse<object>(null, "Người dùng không hợp lệ."));

            await _bookingService.SendBookingConfirmationEmailForUser(userId, id);
            return Ok(new ApiResponse<object>(null, "Đã gửi email xác nhận đặt vé."));
        }
    }
}
