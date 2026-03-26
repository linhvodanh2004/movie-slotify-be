using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;
using BusinessLogic.Exceptions;
using DataAccess.Entities;
using DataAccess.Repositories;

namespace BusinessLogic.Services.Implementation
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly IShowtimeRepository _showtimeRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IAuditoriumRepository _auditoriumRepository;

        public ShowtimeService(
            IShowtimeRepository showtimeRepository, 
            IMovieRepository movieRepository, 
            IAuditoriumRepository auditoriumRepository)
        {
            _showtimeRepository = showtimeRepository;
            _movieRepository = movieRepository;
            _auditoriumRepository = auditoriumRepository;
        }

        public async Task<IEnumerable<ShowtimeResponse>> GetAllShowtimes(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var showtimes = await _showtimeRepository.GetAllAsync(fromDate, toDate);
            return showtimes.Select(MapToResponse);
        }

        public async Task<IEnumerable<ShowtimeResponse>> GetShowtimesByMovie(Guid movieId)
        {
            var showtimes = await _showtimeRepository.GetByMovieIdAsync(movieId);
            return showtimes.Select(MapToResponse);
        }

        public async Task<IEnumerable<ShowtimeResponse>> GetShowtimesByCinema(Guid cinemaId)
        {
            var showtimes = await _showtimeRepository.GetByCinemaIdAsync(cinemaId);
            return showtimes.Select(MapToResponse);
        }

        public async Task<ShowtimeResponse> GetShowtimeById(Guid id)
        {
            var showtime = await _showtimeRepository.GetByIdAsync(id);
            if (showtime == null)
                throw new NotFoundException("Không tìm thấy lịch chiếu.");

            return MapToResponse(showtime);
        }

        public async Task<ShowtimeResponse> AddShowtime(ShowtimeRequest request)
        {
            ValidateShowtimeRequest(request);

            var movie = await _movieRepository.GetMovieById(request.MovieId);
            if (movie == null) throw new NotFoundException("Không tìm thấy phim.");

            var auditorium = await _auditoriumRepository.GetByIdAsync(request.AuditoriumId);
            if (auditorium == null) throw new NotFoundException("Không tìm thấy phòng chiếu.");

            // Auto-calculate EndTime = StartTime + DurationMinutes + 10 min buffer
            var endTime = request.StartTime.AddMinutes(movie.DurationMinutes + 10);

            var hasConflict = await _showtimeRepository.HasConflictAsync(request.AuditoriumId, request.StartTime, endTime);
            if (hasConflict)
                throw new ValidationException("Phòng chiếu này đã có lịch chiếu trong khoảng thời gian này.");

            var showtime = new Showtime
            {
                StartTime = request.StartTime,
                EndTime = endTime,
                StandardPrice = request.StandardPrice,
                VipPrice = request.VipPrice,
                CouplePrice = request.CouplePrice,
                MovieId = request.MovieId,
                AuditoriumId = request.AuditoriumId
            };

            var added = await _showtimeRepository.AddAsync(showtime);
            
            // Reload with relations
            return await GetShowtimeById(added.Id);
        }

        public async Task<ShowtimeResponse> UpdateShowtime(Guid id, ShowtimeRequest request)
        {
            ValidateShowtimeRequest(request);

            var showtime = await _showtimeRepository.GetByIdAsync(id);
            if (showtime == null)
                throw new NotFoundException("Không tìm thấy lịch chiếu.");

            var movie = await _movieRepository.GetMovieById(request.MovieId);
            if (movie == null) throw new NotFoundException("Không tìm thấy phim.");
            
            var auditorium = await _auditoriumRepository.GetByIdAsync(request.AuditoriumId);
            if (auditorium == null) throw new NotFoundException("Không tìm thấy phòng chiếu.");

            // Auto-calculate EndTime = StartTime + DurationMinutes + 10 min buffer
            var endTime = request.StartTime.AddMinutes(movie.DurationMinutes + 10);

            var hasConflict = await _showtimeRepository.HasConflictAsync(request.AuditoriumId, request.StartTime, endTime, id);
            if (hasConflict)
                throw new ValidationException("Phòng chiếu này đã có lịch chiếu trong khoảng thời gian này.");

            showtime.StartTime = request.StartTime;
            showtime.EndTime = endTime;
            showtime.StandardPrice = request.StandardPrice;
            showtime.VipPrice = request.VipPrice;
            showtime.CouplePrice = request.CouplePrice;
            showtime.MovieId = request.MovieId;
            showtime.AuditoriumId = request.AuditoriumId;

            await _showtimeRepository.UpdateAsync(showtime);
            
            return await GetShowtimeById(id);
        }

        public async Task DeleteShowtime(Guid id)
        {
            var showtime = await _showtimeRepository.GetByIdAsync(id);
            if (showtime == null)
                throw new NotFoundException("Không tìm thấy lịch chiếu.");

            if (await _showtimeRepository.HasBookingsAsync(id))
                throw new BadRequestException("Không thể xóa lịch chiếu đã có người đặt vé.");

            await _showtimeRepository.DeleteAsync(showtime);
        }

        private ShowtimeResponse MapToResponse(Showtime s)
        {
            return new ShowtimeResponse
            {
                Id = s.Id,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                StandardPrice = s.StandardPrice,
                VipPrice = s.VipPrice,
                CouplePrice = s.CouplePrice,
                MovieId = s.MovieId,
                MovieTitle = s.Movie?.Title ?? "Unknown",
                MoviePosterUrl = s.Movie?.PosterUrl,
                AuditoriumId = s.AuditoriumId,
                AuditoriumName = s.Auditorium?.Name ?? "Unknown",
                CinemaId = s.Auditorium?.CinemaId ?? Guid.Empty,
                CinemaName = s.Auditorium?.Cinema?.Name ?? "Unknown"
            };
        }

        private static void ValidateShowtimeRequest(ShowtimeRequest request)
        {
            if (request == null)
                throw new ValidationException("Dữ liệu lịch chiếu không hợp lệ.");

            if (request.MovieId == Guid.Empty)
                throw new ValidationException("Phim là bắt buộc.");

            if (request.AuditoriumId == Guid.Empty)
                throw new ValidationException("Phòng chiếu là bắt buộc.");

            if (request.StartTime == default)
                throw new ValidationException("Thời gian bắt đầu là bắt buộc.");

            if (request.StandardPrice < 0 || request.VipPrice < 0 || request.CouplePrice < 0)
                throw new ValidationException("Giá vé không được nhỏ hơn 0.");

            if (request.VipPrice < request.StandardPrice)
                throw new ValidationException("Giá VIP không được nhỏ hơn giá Standard.");

            if (request.CouplePrice < request.StandardPrice)
                throw new ValidationException("Giá Couple không được nhỏ hơn giá Standard.");
        }
    }
}
