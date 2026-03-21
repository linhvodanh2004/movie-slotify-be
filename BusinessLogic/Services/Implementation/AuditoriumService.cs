using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Exceptions;
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
                throw new NotFoundException("Không tìm thấy phòng chiếu.");

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
                throw new NotFoundException("Không tìm thấy rạp.");

            var normalizedName = NormalizeName(request.Name);
            if (await _auditoriumRepository.ExistsByNameAsync(request.CinemaId, normalizedName.ToUpper()))
                throw new ValidationException("Tên phòng đã tồn tại trong rạp này.");

            var auditorium = new Auditorium
            {
                Name = normalizedName,
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
                throw new NotFoundException("Không tìm thấy phòng chiếu.");

            var cinema = await _cinemaRepository.GetByIdAsync(request.CinemaId);
            if (cinema == null)
                throw new NotFoundException("Không tìm thấy rạp.");

            var normalizedName = NormalizeName(request.Name);
            if (await _auditoriumRepository.ExistsByNameAsync(request.CinemaId, normalizedName.ToUpper(), id))
                throw new ValidationException("Tên phòng đã tồn tại trong rạp này.");

            auditorium.Name = normalizedName;
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
                throw new NotFoundException("Không tìm thấy phòng chiếu.");

            if (await _auditoriumRepository.HasShowtimesAsync(id))
                throw new BadRequestException("Không thể xóa phòng đang có lịch chiếu.");

            if (await _auditoriumRepository.HasSeatsAsync(id))
                throw new BadRequestException("Không thể xóa phòng đang còn ghế.");

            await _auditoriumRepository.DeleteAsync(auditorium);
        }

        public async Task ActivateAuditorium(Guid id)
        {
            var auditorium = await _auditoriumRepository.GetByIdAsync(id);
            if (auditorium == null)
                throw new NotFoundException("Không tìm thấy phòng chiếu.");

            auditorium.IsActive = true;
            await _auditoriumRepository.UpdateAsync(auditorium);
        }

        public async Task DeactivateAuditorium(Guid id)
        {
            var auditorium = await _auditoriumRepository.GetByIdAsync(id);
            if (auditorium == null)
                throw new NotFoundException("Không tìm thấy phòng chiếu.");

            auditorium.IsActive = false;
            await _auditoriumRepository.UpdateAsync(auditorium);
        }

        private static string NormalizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException("Tên phòng là bắt buộc.");

            var normalized = value.Trim();
            if (normalized.Length > 50)
                throw new ValidationException("Tên phòng không được vượt quá 50 ký tự.");

            return normalized;
        }
    }
}
