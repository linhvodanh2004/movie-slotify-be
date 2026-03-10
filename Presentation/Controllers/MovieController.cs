using BusinessLogic.Services;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;
using BusinessLogic.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMovies([FromQuery] bool activeOnly = false)
        {
            var movies = await _movieService.GetAllMovies(activeOnly);
            return Ok(new ApiResponse<IEnumerable<MovieResponse>>(movies, "Movies retrieved successfully"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovieById(Guid id)
        {
            var movie = await _movieService.GetMovieById(id);
            return Ok(new ApiResponse<MovieResponse>(movie, "Movie retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AddMovie(MovieRequest request)
        {
            var movie = await _movieService.AddMovie(request);
            return Ok(new ApiResponse<MovieResponse>(movie, "Movie added successfully"));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateMovie(Guid id, MovieRequest request)
        {
            var movie = await _movieService.UpdateMovie(id, request);
            return Ok(new ApiResponse<MovieResponse>(movie, "Movie updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteMovie(Guid id)
        {
            await _movieService.DeleteMovie(id);
            return Ok(new ApiResponse<object>(null, "Movie deleted successfully"));
        }

        [HttpPut("{id}/activate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ActivateMovie(Guid id)
        {
            await _movieService.ActivateMovie(id);
            return Ok(new ApiResponse<object>(null, "Movie activated successfully"));
        }

        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeactivateMovie(Guid id)
        {
            await _movieService.DeactivateMovie(id);
            return Ok(new ApiResponse<object>(null, "Movie deactivated successfully"));
        }
    }
}
