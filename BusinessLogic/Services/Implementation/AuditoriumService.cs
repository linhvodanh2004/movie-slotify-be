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
    public class AuditoriumService : IAuditoriumService
    {
        private readonly IAuditoriumRepository _auditoriumRepository;
        private readonly ICinemaRepository _cinemaRepository;

        public AuditoriumService(IAuditoriumRepository auditoriumRepository, ICinemaRepository cinemaRepository)
        {
            _auditoriumRepository = auditoriumRepository;
            _cinemaRepository = cinemaRepository;
        }

        public async Task<IEnumerable<AuditoriumResponse>> GetAuditoriumsByCinema(Guid cinemaId, bool includeInactive = false)
        {
            var auditoriums = await _auditoriumRepository.GetByCinemaIdAsync(cinemaId, includeInactive);
            
            return auditoriums.Select(a => new AuditoriumResponse
            {
                Id = a.Id,
                Name = a.Name,
                CinemaId = a.CinemaId,
                CinemaName = a.Cinema?.Name ?? "Unknown",
                IsActive = a.IsActive,
                TotalSeats = a.Seats?.Count ?? 0
            });
        }

        public async Task<AuditoriumResponse> GetAuditoriumById(Guid id)
        {
            var auditorium = await _auditoriumRepository.GetByIdAsync(id);
            if (auditorium == null)
                throw new Exception("Auditorium not found");

            return new AuditoriumResponse
            {
                Id = auditorium.Id,
                Name = auditorium.Name,
                CinemaId = auditorium.CinemaId,
                CinemaName = auditorium.Cinema?.Name ?? "Unknown",
                IsActive = auditorium.IsActive,
                TotalSeats = auditorium.Seats?.Count ?? 0
            };
        }

        public async Task<AuditoriumResponse> AddAuditorium(AuditoriumRequest request)
        {
            var cinema = await _cinemaRepository.GetByIdAsync(request.CinemaId);
            if (cinema == null)
                throw new Exception("Cinema not found");

            var auditorium = new Auditorium
            {
                Name = request.Name,
                CinemaId = request.CinemaId,
                IsActive = request.IsActive
            };

            var added = await _auditoriumRepository.AddAsync(auditorium);
            
            return new AuditoriumResponse
            {
                Id = added.Id,
                Name = added.Name,
                CinemaId = added.CinemaId,
                CinemaName = cinema.Name,
                IsActive = added.IsActive,
                TotalSeats = 0
            };
        }

        public async Task<AuditoriumResponse> UpdateAuditorium(Guid id, AuditoriumRequest request)
        {
            var auditorium = await _auditoriumRepository.GetByIdAsync(id);
            if (auditorium == null)
                throw new Exception("Auditorium not found");

            var cinema = await _cinemaRepository.GetByIdAsync(request.CinemaId);
            if (cinema == null)
                throw new Exception("Cinema not found");

            auditorium.Name = request.Name;
            auditorium.CinemaId = request.CinemaId;
            auditorium.IsActive = request.IsActive;

            await _auditoriumRepository.UpdateAsync(auditorium);

            return new AuditoriumResponse
            {
                Id = auditorium.Id,
                Name = auditorium.Name,
                CinemaId = auditorium.CinemaId,
                CinemaName = cinema.Name,
                IsActive = auditorium.IsActive,
                TotalSeats = auditorium.Seats?.Count ?? 0
            };
        }

        public async Task DeleteAuditorium(Guid id)
        {
            var auditorium = await _auditoriumRepository.GetByIdAsync(id);
            if (auditorium == null)
                throw new Exception("Auditorium not found");

            await _auditoriumRepository.DeleteAsync(auditorium);
        }

        public async Task ActivateAuditorium(Guid id)
        {
            var auditorium = await _auditoriumRepository.GetByIdAsync(id);
            if (auditorium == null)
                throw new Exception("Auditorium not found");

            auditorium.IsActive = true;
            await _auditoriumRepository.UpdateAsync(auditorium);
        }

        public async Task DeactivateAuditorium(Guid id)
        {
            var auditorium = await _auditoriumRepository.GetByIdAsync(id);
            if (auditorium == null)
                throw new Exception("Auditorium not found");

            auditorium.IsActive = false;
            await _auditoriumRepository.UpdateAsync(auditorium);
        }
    }
}
