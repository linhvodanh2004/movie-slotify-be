using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;

namespace BusinessLogic.Services
{
    public interface IMovieService
    {
        Task<MovieResponse> AddMovie(MovieRequest request);
        Task<MovieResponse> UpdateMovie(Guid id, MovieRequest request);
        Task<MovieResponse> GetMovieById(Guid id);
        Task<IEnumerable<MovieResponse>> GetAllMovies();
        Task DeleteMovie(Guid id);
        Task ActivateMovie(Guid id);
        Task DeactivateMovie(Guid id);
    }
}
