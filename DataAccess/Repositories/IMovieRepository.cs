using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Entities;

namespace DataAccess.Repositories
{
    public interface IMovieRepository
    {
        Task<Movie?> GetMovieById(Guid id);
        Task<IEnumerable<Movie>> GetAllMovies(bool activeOnly = false);
        Task AddMovie(Movie movie);
        Task UpdateMovie(Movie movie);
        Task DeleteMovie(Movie movie);
        Task ChangeMovieStatus(Guid id, bool isActive);
        Task<bool> HasShowtimesAsync(Guid movieId);
    }
}
