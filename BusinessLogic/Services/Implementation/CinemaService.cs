using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;
using DataAccess.Entities;
using DataAccess.Repositories;

namespace BusinessLogic.Services.Implementation
{
    public class CinemaService : ICinemaService
    {
        private readonly ICinemaRepository _cinemaRepository;
        private readonly IMapper _mapper;

        public CinemaService(ICinemaRepository cinemaRepository, IMapper mapper)
        {
            _cinemaRepository = cinemaRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CinemaResponse>> GetAllCinemas(bool includeInactive = false)
        {
            var cinemas = await _cinemaRepository.GetAllAsync(includeInactive);
            
            // Map manually since AutoMapper might not count Auditoriums automatically
            return cinemas.Select(c => new CinemaResponse
            {
                Id = c.Id,
                Name = c.Name,
                Address = c.Address,
                City = c.City,
                IsActive = c.IsActive,
                NumberOfAuditoriums = c.Auditoriums?.Count ?? 0
            });
        }

        public async Task<CinemaResponse> GetCinemaById(Guid id)
        {
            var cinema = await _cinemaRepository.GetByIdAsync(id);
            if (cinema == null)
                throw new Exception("Cinema not found");

            return new CinemaResponse
            {
                Id = cinema.Id,
                Name = cinema.Name,
                Address = cinema.Address,
                City = cinema.City,
                IsActive = cinema.IsActive,
                NumberOfAuditoriums = cinema.Auditoriums?.Count ?? 0
            };
        }

        public async Task<CinemaResponse> AddCinema(CinemaRequest request)
        {
            // Simple mapping
            var cinema = new Cinema
            {
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                IsActive = request.IsActive
            };

            var addedCinema = await _cinemaRepository.AddAsync(cinema);
            
            return new CinemaResponse
            {
                Id = addedCinema.Id,
                Name = addedCinema.Name,
                Address = addedCinema.Address,
                City = addedCinema.City,
                IsActive = addedCinema.IsActive,
                NumberOfAuditoriums = 0
            };
        }

        public async Task<CinemaResponse> UpdateCinema(Guid id, CinemaRequest request)
        {
            var cinema = await _cinemaRepository.GetByIdAsync(id);
            if (cinema == null)
                throw new Exception("Cinema not found");

            cinema.Name = request.Name;
            cinema.Address = request.Address;
            cinema.City = request.City;
            cinema.IsActive = request.IsActive;

            await _cinemaRepository.UpdateAsync(cinema);

            return new CinemaResponse
            {
                Id = cinema.Id,
                Name = cinema.Name,
                Address = cinema.Address,
                City = cinema.City,
                IsActive = cinema.IsActive,
                NumberOfAuditoriums = cinema.Auditoriums?.Count ?? 0
            };
        }

        public async Task DeleteCinema(Guid id)
        {
            var cinema = await _cinemaRepository.GetByIdAsync(id);
            if (cinema == null)
                throw new Exception("Cinema not found");

            await _cinemaRepository.DeleteAsync(cinema);
        }

        public async Task ActivateCinema(Guid id)
        {
            var cinema = await _cinemaRepository.GetByIdAsync(id);
            if (cinema == null)
                throw new Exception("Cinema not found");

            cinema.IsActive = true;
            await _cinemaRepository.UpdateAsync(cinema);
        }

        public async Task DeactivateCinema(Guid id)
        {
            var cinema = await _cinemaRepository.GetByIdAsync(id);
            if (cinema == null)
                throw new Exception("Cinema not found");

            cinema.IsActive = false;
            await _cinemaRepository.UpdateAsync(cinema);
        }
    }
}
