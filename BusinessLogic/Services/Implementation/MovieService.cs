using System;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;
using BusinessLogic.Exceptions;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic.Services.Implementation
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public MovieService(IMovieRepository movieRepository, IMapper mapper, IImageService imageService)
        {
            _movieRepository = movieRepository;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task<IEnumerable<MovieResponse>> GetAllMovies(bool activeOnly = false)
        {
            var movies = await _movieRepository.GetAllMovies(activeOnly);
            return _mapper.Map<IEnumerable<MovieResponse>>(movies);
        }
        public async Task<MovieResponse> GetMovieById(Guid id)
        {
            var movie = await _movieRepository.GetMovieById(id);
            return _mapper.Map<MovieResponse>(movie);
        }
        public async Task<MovieResponse> AddMovie(MovieRequest request)
        {
            ValidateMovieRequest(request);
            var movie = _mapper.Map<Movie>(request);
            await _movieRepository.AddMovie(movie);
            return _mapper.Map<MovieResponse>(movie);
        }
        public async Task<MovieResponse> UpdateMovie(Guid id, MovieRequest request)
        {
            ValidateMovieRequest(request);
            var movie = await _movieRepository.GetMovieById(id);
            if (movie == null) throw new BadRequestException("Không tìm thấy phim.");

            if (!string.IsNullOrEmpty(movie.PosterUrl) && 
                movie.PosterUrl != request.PosterUrl && 
                movie.PosterUrl.Contains("res.cloudinary.com"))
            {
                await _imageService.DeleteImageAsync(movie.PosterUrl);
            }

            _mapper.Map(request, movie);
            await _movieRepository.UpdateMovie(movie);
            return _mapper.Map<MovieResponse>(movie);
        }
        public async Task DeleteMovie(Guid id)
        {
            var movie = await _movieRepository.GetMovieById(id);
            if (movie == null) throw new BadRequestException("Không tìm thấy phim.");

            if (await _movieRepository.HasShowtimesAsync(id))
                throw new BadRequestException("Không thể xóa phim đang có lịch chiếu.");

            if (!string.IsNullOrEmpty(movie.PosterUrl) && movie.PosterUrl.Contains("res.cloudinary.com"))
            {
                await _imageService.DeleteImageAsync(movie.PosterUrl);
            }

            await _movieRepository.DeleteMovie(movie);
        }

        public async Task ActivateMovie(Guid id)
        {
            await _movieRepository.ChangeMovieStatus(id, true);
        }

        public async Task DeactivateMovie(Guid id)
        {
            await _movieRepository.ChangeMovieStatus(id, false);
        }

        public async Task<IEnumerable<MovieResponse>> SearchMovies(string title, string genre)
        {
            var movies = await _movieRepository.GetAllMovies(true);
            var query = movies.AsEnumerable();

            if (!string.IsNullOrEmpty(title))
                query = query.Where(m => m.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(genre))
                query = query.Where(m => m.Genre.Contains(genre, StringComparison.OrdinalIgnoreCase));

            return _mapper.Map<IEnumerable<MovieResponse>>(query);
        }

        public async Task<IEnumerable<MovieResponse>> GetNowShowingMovies()
        {
            var movies = await _movieRepository.GetAllMovies(true);
            // In a real app, we'd check if there are active showtimes within a range.
            // For now, let's say movies released in the last 30 days are "Now Showing".
            var nowShowing = movies.Where(m => m.ReleaseDate <= DateTime.UtcNow && m.ReleaseDate >= DateTime.UtcNow.AddDays(-30));
            return _mapper.Map<IEnumerable<MovieResponse>>(nowShowing);
        }

        public async Task<IEnumerable<MovieResponse>> GetComingSoonMovies()
        {
            var movies = await _movieRepository.GetAllMovies(true);
            var comingSoon = movies.Where(m => m.ReleaseDate > DateTime.UtcNow);
            return _mapper.Map<IEnumerable<MovieResponse>>(comingSoon);
        }

        private static void ValidateMovieRequest(MovieRequest request)
        {
            if (request == null)
                throw new ValidationException("Dữ liệu phim không hợp lệ.");

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ValidationException("Tên phim là bắt buộc.");
            request.Title = request.Title.Trim();

            if (string.IsNullOrWhiteSpace(request.Director))
                throw new ValidationException("Đạo diễn là bắt buộc.");
            request.Director = request.Director.Trim();

            if (string.IsNullOrWhiteSpace(request.Cast))
                throw new ValidationException("Diễn viên là bắt buộc.");
            request.Cast = request.Cast.Trim();

            if (string.IsNullOrWhiteSpace(request.Genre))
                throw new ValidationException("Thể loại là bắt buộc.");
            request.Genre = request.Genre.Trim();

            if (request.DurationMinutes <= 0)
                throw new ValidationException("Thời lượng phim phải lớn hơn 0.");

            if (request.ReleaseDate == default)
                throw new ValidationException("Ngày khởi chiếu không hợp lệ.");

            if (string.IsNullOrWhiteSpace(request.PosterUrl))
                throw new ValidationException("Poster là bắt buộc.");
            request.PosterUrl = request.PosterUrl.Trim();

            if (!string.IsNullOrWhiteSpace(request.TrailerUrl))
                request.TrailerUrl = request.TrailerUrl.Trim();
        }
    }
}
