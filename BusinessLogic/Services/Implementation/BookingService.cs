using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.DTOs.Notifications;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;
using BusinessLogic.Exceptions;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Implementation
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IShowtimeRepository _showtimeRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepository,
            IShowtimeRepository showtimeRepository,
            ISeatRepository seatRepository,
            IEmailService emailService,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _showtimeRepository = showtimeRepository;
            _seatRepository = seatRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IEnumerable<SeatAvailabilityResponse>> GetSeatAvailability(Guid showtimeId)
        {
            var showtime = await _showtimeRepository.GetByIdAsync(showtimeId);
            if (showtime == null) throw new BadRequestException("Khong tim thay suat chieu.");
            EnsureShowtimeStillBookable(showtime);

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
            if (showtime == null) throw new BadRequestException("Khong tim thay suat chieu.");
            EnsureShowtimeStillBookable(showtime);

            var seats = new List<Seat>();
            foreach (var seatId in request.SeatIds)
            {
                var seat = await _seatRepository.GetByIdAsync(seatId);
                if (seat == null || seat.AuditoriumId != showtime.AuditoriumId)
                {
                    throw new BadRequestException($"Ghe {seatId} khong hop le.");
                }

                seats.Add(seat);
            }

            var bookedTickets = await _bookingRepository.GetTicketsByShowtime(request.ShowtimeId);
            var bookedSeatIds = bookedTickets.Select(t => t.SeatId).ToHashSet();
            if (request.SeatIds.Any(id => bookedSeatIds.Contains(id)))
            {
                throw new BadRequestException("Mot hoac nhieu ghe da duoc dat.");
            }

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
            if (booking == null) throw new BadRequestException("Khong tim thay don dat ve.");

            return new BookingResponse
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
        }

        public async Task<IEnumerable<BookingResponse>> GetUserBookings(Guid userId)
        {
            var bookings = await _bookingRepository.GetBookingsByUser(userId);
            var results = new List<BookingResponse>();

            foreach (var booking in bookings)
            {
                results.Add(await GetBookingDetails(booking.Id));
            }

            return results;
        }

        public async Task ProcessPayment(string transactionId, decimal amount, string content)
        {
            var bookingId = await ResolveBookingIdAsync(transactionId, content);
            var booking = await _bookingRepository.GetBookingForPayment(bookingId);

            if (booking == null)
            {
                throw new BadRequestException("Khong tim thay don hang.");
            }

            if (booking.TotalAmount != amount)
            {
                throw new BadRequestException("So tien thanh toan khong khop.");
            }

            var confirmed = await _bookingRepository.ConfirmPayment(bookingId, amount, transactionId);
            if (!confirmed)
            {
                var currentBooking = await _bookingRepository.GetBookingForPayment(bookingId);
                if (currentBooking?.Status is BookingStatus.Paid or BookingStatus.Confirmed)
                {
                    _logger.LogInformation(
                        "Duplicate payment webhook for booking {BookingId} with transaction {TransactionId}. Re-sending confirmation email.",
                        bookingId,
                        transactionId);
                    await SendBookingConfirmationEmailAsync(bookingId);
                    return;
                }

                throw new BadRequestException("Don hang khong o trang thai Pending hoac da duoc thanh toan truoc do.");
            }

            await SendBookingConfirmationEmailAsync(bookingId);
        }

        public async Task SendBookingConfirmationEmailForBooking(Guid bookingId)
        {
            var booking = await _bookingRepository.GetBookingById(bookingId);
            if (booking == null)
                throw new NotFoundException("Khong tim thay don dat ve.");

            if (booking.Status is not (BookingStatus.Paid or BookingStatus.Confirmed))
                throw new BadRequestException("Chi gui email xac nhan cho don da thanh toan thanh cong.");

            if (booking.User == null || string.IsNullOrWhiteSpace(booking.User.Email))
                throw new BadRequestException("Don dat ve chua co email nguoi nhan.");

            var emailRequest = BuildBookingConfirmationEmailRequest(booking);
            await _emailService.SendBookingConfirmationEmailAsync(emailRequest);
        }

        public async Task SendBookingConfirmationEmailForUser(Guid userId, Guid bookingId)
        {
            var booking = await _bookingRepository.GetBookingById(bookingId);
            if (booking == null)
                throw new NotFoundException("Khong tim thay don dat ve.");

            if (booking.UserId != userId)
                throw new BadRequestException("Ban khong co quyen thao tac voi don dat ve nay.");

            if (booking.Status is not (BookingStatus.Paid or BookingStatus.Confirmed))
                throw new BadRequestException("Chi gui email xac nhan cho don da thanh toan thanh cong.");

            if (booking.User == null || string.IsNullOrWhiteSpace(booking.User.Email))
                throw new BadRequestException("Don dat ve chua co email nguoi nhan.");

            var emailRequest = BuildBookingConfirmationEmailRequest(booking);
            await _emailService.SendBookingConfirmationEmailAsync(emailRequest);
        }

        private async Task<Guid> ResolveBookingIdAsync(string transactionId, string content)
        {
            var bookingIdStr = string.Empty;
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
                bookingIdStr = content;
            }

            bookingIdStr = bookingIdStr.Replace("-", "");

            if (Guid.TryParse(bookingIdStr, out var bookingId))
            {
                return bookingId;
            }

            if (bookingIdStr.Length >= 20)
            {
                var allPending = await _bookingRepository.GetPendingBookings();
                var matched = allPending.FirstOrDefault(
                    booking => booking.Id.ToString("N").StartsWith(bookingIdStr, StringComparison.OrdinalIgnoreCase));

                if (matched != null)
                {
                    return matched.Id;
                }

                var bookingByTransaction = await _bookingRepository.GetBookingByTransactionId(transactionId);
                if (bookingByTransaction != null)
                {
                    return bookingByTransaction.Id;
                }
            }

            throw new BadRequestException("Khong the xac dinh don hang tu noi dung thanh toan.");
        }

        private async Task SendBookingConfirmationEmailAsync(Guid bookingId)
        {
            try
            {
                var booking = await _bookingRepository.GetBookingById(bookingId);
                if (booking?.User == null || string.IsNullOrWhiteSpace(booking.User.Email))
                {
                    _logger.LogWarning(
                        "Payment confirmed for booking {BookingId}, but user email is missing so no confirmation email was sent.",
                        bookingId);
                    return;
                }

                if (booking.Status is not (BookingStatus.Paid or BookingStatus.Confirmed))
                {
                    _logger.LogInformation(
                        "Skip booking confirmation email for booking {BookingId} because status is {Status}.",
                        bookingId,
                        booking.Status);
                    return;
                }

                var emailRequest = BuildBookingConfirmationEmailRequest(booking);
                await _emailService.SendBookingConfirmationEmailAsync(emailRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Payment was confirmed for booking {BookingId}, but sending the booking confirmation email failed.",
                    bookingId);
            }
        }

        private static BookingConfirmationEmailRequest BuildBookingConfirmationEmailRequest(Booking booking)
        {
            var cinema = booking.Showtime.Auditorium.Cinema;
            var cinemaAddress = string.Join(
                ", ",
                new[] { cinema.Address, cinema.City }.Where(value => !string.IsNullOrWhiteSpace(value)));

            return new BookingConfirmationEmailRequest
            {
                RecipientEmail = booking.User.Email,
                RecipientName = FirstNonEmpty(booking.User.FullName, booking.User.Username, booking.User.Email),
                BookingId = booking.Id.ToString(),
                BookingCode = booking.Id.ToString("N")[..8].ToUpperInvariant(),
                MovieTitle = booking.Showtime.Movie.Title,
                MoviePosterUrl = booking.Showtime.Movie.PosterUrl,
                MovieGenre = booking.Showtime.Movie.Genre,
                DurationMinutes = booking.Showtime.Movie.DurationMinutes,
                CinemaName = cinema.Name,
                CinemaAddress = cinemaAddress,
                AuditoriumName = booking.Showtime.Auditorium.Name,
                StartTime = booking.Showtime.StartTime,
                EndTime = booking.Showtime.EndTime,
                TotalAmount = booking.TotalAmount,
                PaymentMethod = booking.Payment?.PaymentMethod ?? "SePay",
                TransactionId = booking.Payment?.TransactionId ?? string.Empty,
                Tickets = booking.Tickets
                    .OrderBy(ticket => ticket.Seat.Row)
                    .ThenBy(ticket => ticket.Seat.Number)
                    .Select(ticket => new BookingConfirmationTicketItem
                    {
                        SeatLabel = $"{ticket.Seat.Row}{ticket.Seat.Number}",
                        SeatType = GetSeatTypeLabel(ticket.Seat.Type),
                        Price = ticket.Price
                    })
                    .ToList()
            };
        }

        private static string GetSeatTypeLabel(SeatType seatType)
        {
            return seatType switch
            {
                SeatType.VIP => "VIP",
                SeatType.Couple => "Couple",
                _ => "Standard"
            };
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
        }

        private static decimal GetSeatPrice(Showtime showtime, SeatType type)
        {
            return type switch
            {
                SeatType.VIP => showtime.VipPrice,
                SeatType.Couple => showtime.CouplePrice,
                _ => showtime.StandardPrice
            };
        }

        private static void EnsureShowtimeStillBookable(Showtime showtime)
        {
            if (showtime.EndTime <= DateTime.UtcNow)
            {
                throw new BadRequestException("Suat chieu da ket thuc, khong the dat ve.");
            }
        }
    }
}
