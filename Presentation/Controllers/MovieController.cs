using BusinessLogic.Services;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;
using BusinessLogic.Wrappers;
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
        public async Task<IActionResult> GetAllMovies()
        {
            var movies = await _movieService.GetAllMovies();
            return Ok(new ApiResponse<IEnumerable<MovieResponse>>(movies, "Movies retrieved successfully"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovieById(Guid id)
        {
            var movie = await _movieService.GetMovieById(id);
            return Ok(new ApiResponse<MovieResponse>(movie, "Movie retrieved successfully"));
        }

        [HttpPost]
        public async Task<IActionResult> AddMovie(MovieRequest request)
        {
            var movie = await _movieService.AddMovie(request);
            return Ok(new ApiResponse<MovieResponse>(movie, "Movie added successfully"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMovie(Guid id, MovieRequest request)
        {
            var movie = await _movieService.UpdateMovie(id, request);
            return Ok(new ApiResponse<MovieResponse>(movie, "Movie updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(Guid id)
        {
            await _movieService.DeleteMovie(id);
            return Ok(new ApiResponse<object>(null, "Movie deleted successfully"));
        }
    }
}
