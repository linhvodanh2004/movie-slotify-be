using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Entities;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementation
{
    public class MovieRepository : IMovieRepository
    {
        private readonly AppDbContext _context;
        public MovieRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddMovie(Movie movie)
        {
            await _context.Movies.AddAsync(movie);
            await _context.SaveChangesAsync();
        }

        public async Task<Movie?> GetMovieById(Guid id)
        {
            return await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Movie>> GetAllMovies(bool activeOnly = false)
        {
            if (activeOnly)
            {
                return await _context.Movies.ToListAsync(); // Relies on the global query filter
            }

            return await _context.Movies.IgnoreQueryFilters().ToListAsync();
        }

        public async Task UpdateMovie(Movie movie)
        {
            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMovie(Movie movie)
        {
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeMovieStatus(Guid id, bool isActive)
        {
            var movie = await _context.Movies.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == id);
            if (movie != null)
            {
                movie.IsActive = isActive;
                _context.Movies.Update(movie);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasShowtimesAsync(Guid movieId)
        {
            return await _context.Showtimes.IgnoreQueryFilters().AnyAsync(s => s.MovieId == movieId);
        }
    }
}
