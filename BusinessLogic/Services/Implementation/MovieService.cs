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

        public MovieService(IMovieRepository movieRepository, IMapper mapper)
        {
            _movieRepository = movieRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MovieResponse>> GetAllMovies()
        {
            var movies = await _movieRepository.GetAllMovies();
            return _mapper.Map<IEnumerable<MovieResponse>>(movies);
        }
        public async Task<MovieResponse> GetMovieById(Guid id)
        {
            var movie = await _movieRepository.GetMovieById(id);
            return _mapper.Map<MovieResponse>(movie);
        }
        public async Task<MovieResponse> AddMovie(MovieRequest request)
        {
            var movie = _mapper.Map<Movie>(request);
            await _movieRepository.AddMovie(movie);
            return _mapper.Map<MovieResponse>(movie);
        }
        public async Task<MovieResponse> UpdateMovie(Guid id, MovieRequest request)
        {
            var movie = await _movieRepository.GetMovieById(id);
            if (movie == null) throw new BadRequestException("Không tìm thấy phim.");
            _mapper.Map(request, movie);
            await _movieRepository.UpdateMovie(movie);
            return _mapper.Map<MovieResponse>(movie);
        }
        public async Task DeleteMovie(Guid id)
        {
            var movie = await _movieRepository.GetMovieById(id);
            if (movie == null) throw new BadRequestException("Không tìm thấy phim.");
            await _movieRepository.DeleteMovie(movie);
        }

    }
}
