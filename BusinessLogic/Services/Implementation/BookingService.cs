using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;
using BusinessLogic.Exceptions;
using DataAccess.Entities;
using DataAccess.Repositories;

namespace BusinessLogic.Services.Implementation
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IShowtimeRepository _showtimeRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IMapper _mapper;

        public BookingService(
            IBookingRepository bookingRepository,
            IShowtimeRepository showtimeRepository,
            ISeatRepository seatRepository,
            IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _showtimeRepository = showtimeRepository;
            _seatRepository = seatRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SeatAvailabilityResponse>> GetSeatAvailability(Guid showtimeId)
        {
            var showtime = await _showtimeRepository.GetByIdAsync(showtimeId);
            if (showtime == null) throw new BadRequestException("Không tìm thấy suất chiếu.");

            var allSeats = await _seatRepository.GetByAuditoriumIdAsync(showtime.AuditoriumId, false);
            var bookedTickets = await _bookingRepository.GetTicketsByShowtime(showtimeId);
            var bookedSeatIds = bookedTickets.Select(t => t.SeatId).ToHashSet();

            return allSeats.Select(s => new SeatAvailabilityResponse
            {
                SeatId = s.Id,
                Row = s.Row,
                Number = s.Number,
                Type = s.Type.ToString(),
                IsAvailable = !bookedSeatIds.Contains(s.Id),
                Price = GetSeatPrice(showtime, s.Type)
            });
        }

        public async Task<BookingResponse> CreateBooking(Guid userId, BookingRequest request)
        {
            var showtime = await _showtimeRepository.GetByIdAsync(request.ShowtimeId);
            if (showtime == null) throw new BadRequestException("Không tìm thấy suất chiếu.");

            var seats = new List<Seat>();
            foreach (var seatId in request.SeatIds)
            {
                var seat = await _seatRepository.GetByIdAsync(seatId);
                if (seat == null || seat.AuditoriumId != showtime.AuditoriumId)
                    throw new BadRequestException($"Ghế {seatId} không hợp lệ.");
                seats.Add(seat);
            }

            // Check availability again
            var bookedTickets = await _bookingRepository.GetTicketsByShowtime(request.ShowtimeId);
            var bookedSeatIds = bookedTickets.Select(t => t.SeatId).ToHashSet();
            if (request.SeatIds.Any(id => bookedSeatIds.Contains(id)))
                throw new BadRequestException("Một hoặc nhiều ghế đã được đặt.");

            var booking = new Booking
            {
                UserId = userId,
                ShowtimeId = request.ShowtimeId,
                Status = BookingStatus.Pending,
                BookingDate = DateTime.UtcNow,
                Tickets = seats.Select(s => new Ticket
                {
                    SeatId = s.Id,
                    Price = GetSeatPrice(showtime, s.Type)
                }).ToList()
            };

            booking.TotalAmount = booking.Tickets.Sum(t => t.Price);

            await _bookingRepository.AddBooking(booking);
            return await GetBookingDetails(booking.Id);
        }

        public async Task<BookingResponse> GetBookingDetails(Guid bookingId)
        {
            var booking = await _bookingRepository.GetBookingById(bookingId);
            if (booking == null) throw new BadRequestException("Không tìm thấy đơn đặt vé.");

            var response = new BookingResponse
            {
                Id = booking.Id,
                BookingDate = booking.BookingDate,
                TotalAmount = booking.TotalAmount,
                Status = booking.Status.ToString(),
                MovieTitle = booking.Showtime.Movie.Title,
                CinemaName = booking.Showtime.Auditorium.Cinema.Name,
                AuditoriumName = booking.Showtime.Auditorium.Name,
                StartTime = booking.Showtime.StartTime,
                Tickets = booking.Tickets.Select(t => new TicketResponse
                {
                    Id = t.Id,
                    SeatRow = t.Seat.Row,
                    SeatNumber = t.Seat.Number,
                    SeatType = t.Seat.Type.ToString(),
                    Price = t.Price
                }).ToList()
            };

            return response;
        }

        public async Task<IEnumerable<BookingResponse>> GetUserBookings(Guid userId)
        {
            var bookings = await _bookingRepository.GetBookingsByUser(userId);
            // We'd ideally map this more efficiently
            var results = new List<BookingResponse>();
            foreach (var b in bookings)
            {
                results.Add(await GetBookingDetails(b.Id));
            }
            return results;
        }

        public async Task ProcessPayment(string transactionId, decimal amount, string content)
        {
            // SePay webhook often uses content (like booking code) to identify the booking
            // Expecting content format: "slotify_ok_[BookingId]"
            string bookingIdStr = "";
            var prefixes = new[] { "slotify", "slotifyok", "slotify_ok_", "slotify-ok-", "sok_", "sok" };
            foreach (var prefix in prefixes)
            {
                if (content.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    bookingIdStr = content.Substring(prefix.Length);
                    break;
                }
            }

            if (string.IsNullOrEmpty(bookingIdStr))
            {
                // If no prefix found, the whole content might be the ID
                bookingIdStr = content;
            }

            // Remove any dashes if present (robustness for dash mismatch)
            bookingIdStr = bookingIdStr.Replace("-", "");

            if (!Guid.TryParse(bookingIdStr, out Guid bookingId))
            {
                // Fuzzy match fallback for truncated IDs (usually 40 char limit at banks)
                // e.g., "slotify_ok_efd942a35c744cffaea0d8e17602642" (truncated last char)
                if (bookingIdStr.Length >= 20)
                {
                    var allPending = await _bookingRepository.GetPendingBookings();
                    var matched = allPending.FirstOrDefault(b => 
                        b.Id.ToString("N").StartsWith(bookingIdStr, StringComparison.OrdinalIgnoreCase));
                    
                    if (matched != null)
                    {
                        bookingId = matched.Id;
                    }
                    else
                    {
                        // Try searching by transaction ID if that's how it's linked
                        var bookingByTx = await _bookingRepository.GetBookingByTransactionId(transactionId);
                        if (bookingByTx != null)
                            bookingId = bookingByTx.Id;
                        else
                            throw new BadRequestException("Không thể xác định đơn hàng từ nội dung thanh toán.");
                    }
                }
                else
                {
                    throw new BadRequestException("Không thể xác định đơn hàng từ nội dung thanh toán.");
                }
            }

            var booking = await _bookingRepository.GetBookingForPayment(bookingId);
            if (booking == null) throw new BadRequestException("Không tìm thấy đơn hàng.");

            if (booking.TotalAmount != amount)
                throw new BadRequestException("Số tiền thanh toán không khớp.");

            // Atomic update: no change tracker, no concurrency exception (mirrors NestJS updateMany pattern)
            var confirmed = await _bookingRepository.ConfirmPayment(bookingId, amount, transactionId);
            if (!confirmed)
                throw new BadRequestException("Đơn hàng không ở trạng thái Pending hoặc đã được thanh toán trước đó.");
        }

        private decimal GetSeatPrice(Showtime showtime, SeatType type)
        {
            return type switch
            {
                SeatType.VIP => showtime.VipPrice,
                SeatType.Couple => showtime.CouplePrice,
                _ => showtime.StandardPrice
            };
        }
    }
}
