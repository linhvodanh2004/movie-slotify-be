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
                throw new NotFoundException("Không tìm thấy rạp.");

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
            var normalizedName = NormalizeRequiredText(request.Name, "Tên rạp", 100);
            var normalizedAddress = NormalizeRequiredText(request.Address, "Địa chỉ", 200);
            var normalizedCity = NormalizeRequiredText(request.City, "Thành phố", 100);

            if (await _cinemaRepository.ExistsByNameAsync(normalizedName.ToUpper()))
                throw new ValidationException("Tên rạp đã tồn tại.");

            var cinema = new Cinema
            {
                Name = normalizedName,
                Address = normalizedAddress,
                City = normalizedCity,
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
                throw new NotFoundException("Không tìm thấy rạp.");

            var normalizedName = NormalizeRequiredText(request.Name, "Tên rạp", 100);
            var normalizedAddress = NormalizeRequiredText(request.Address, "Địa chỉ", 200);
            var normalizedCity = NormalizeRequiredText(request.City, "Thành phố", 100);

            if (await _cinemaRepository.ExistsByNameAsync(normalizedName.ToUpper(), id))
                throw new ValidationException("Tên rạp đã tồn tại.");

            cinema.Name = normalizedName;
            cinema.Address = normalizedAddress;
            cinema.City = normalizedCity;
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
                throw new NotFoundException("Không tìm thấy rạp.");

            if (await _cinemaRepository.HasShowtimesAsync(id))
                throw new BadRequestException("Không thể xóa rạp đang có lịch chiếu.");

            if (await _cinemaRepository.HasAuditoriumsAsync(id))
                throw new BadRequestException("Không thể xóa rạp đang còn phòng chiếu.");

            await _cinemaRepository.DeleteAsync(cinema);
        }

        public async Task ActivateCinema(Guid id)
        {
            var cinema = await _cinemaRepository.GetByIdAsync(id);
            if (cinema == null)
                throw new NotFoundException("Không tìm thấy rạp.");

            cinema.IsActive = true;
            await _cinemaRepository.UpdateAsync(cinema);
        }

        public async Task DeactivateCinema(Guid id)
        {
            var cinema = await _cinemaRepository.GetByIdAsync(id);
            if (cinema == null)
                throw new NotFoundException("Không tìm thấy rạp.");

            cinema.IsActive = false;
            await _cinemaRepository.UpdateAsync(cinema);
        }

        private static string NormalizeRequiredText(string value, string fieldName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException($"{fieldName} là bắt buộc.");

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
                throw new ValidationException($"{fieldName} không được vượt quá {maxLength} ký tự.");

            return normalized;
        }
    }
}
