using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.DTOs.requests;
using BusinessLogic.DTOs.responses;
using DataAccess.Entities;
using DataAccess.Repositories;

namespace BusinessLogic.Services.Implementation
{
    public class SeatService : ISeatService
    {
        private readonly ISeatRepository _seatRepository;
        private readonly IAuditoriumRepository _auditoriumRepository;

        public SeatService(ISeatRepository seatRepository, IAuditoriumRepository auditoriumRepository)
        {
            _seatRepository = seatRepository;
            _auditoriumRepository = auditoriumRepository;
        }

        public async Task<IEnumerable<SeatResponse>> GetSeatsByAuditorium(Guid auditoriumId, bool includeInactive = false)
        {
            var seats = await _seatRepository.GetByAuditoriumIdAsync(auditoriumId, includeInactive);
            
            return seats.Select(s => new SeatResponse
            {
                Id = s.Id,
                Row = s.Row,
                Number = s.Number,
                SeatName = $"{s.Row}{s.Number}",
                Type = s.Type.ToString(),
                AuditoriumId = s.AuditoriumId,
                IsActive = s.IsActive
            });
        }

        public async Task<SeatResponse> GetSeatById(Guid id)
        {
            var seat = await _seatRepository.GetByIdAsync(id);
            if (seat == null)
                throw new Exception("Seat not found");

            return new SeatResponse
            {
                Id = seat.Id,
                Row = seat.Row,
                Number = seat.Number,
                SeatName = $"{seat.Row}{seat.Number}",
                Type = seat.Type.ToString(),
                AuditoriumId = seat.AuditoriumId,
                IsActive = seat.IsActive
            };
        }

        public async Task<SeatResponse> AddSeat(SeatRequest request)
        {
            var auditorium = await _auditoriumRepository.GetByIdAsync(request.AuditoriumId);
            if (auditorium == null)
                throw new Exception("Auditorium not found");

            var seat = new Seat
            {
                Row = request.Row,
                Number = request.Number,
                Type = request.Type,
                AuditoriumId = request.AuditoriumId,
                IsActive = request.IsActive
            };

            var added = await _seatRepository.AddAsync(seat);
            
            return new SeatResponse
            {
                Id = added.Id,
                Row = added.Row,
                Number = added.Number,
                SeatName = $"{added.Row}{added.Number}",
                Type = added.Type.ToString(),
                AuditoriumId = added.AuditoriumId,
                IsActive = added.IsActive
            };
        }

        public async Task<IEnumerable<SeatResponse>> AddSeatsBulk(IEnumerable<SeatRequest> requests)
        {
            var seatsToAdd = new List<Seat>();
            // Assume all requests are for the same auditorium for efficiency check
            var auditoriumId = requests.FirstOrDefault()?.AuditoriumId;
            
            if (auditoriumId.HasValue)
            {
                var auditorium = await _auditoriumRepository.GetByIdAsync(auditoriumId.Value);
                if (auditorium == null) throw new Exception("Auditorium not found");
            }

            foreach (var req in requests)
            {
                seatsToAdd.Add(new Seat
                {
                    Row = req.Row,
                    Number = req.Number,
                    Type = req.Type,
                    AuditoriumId = req.AuditoriumId,
                    IsActive = req.IsActive
                });
            }

            if (seatsToAdd.Any())
            {
                await _seatRepository.AddRangeAsync(seatsToAdd);
            }

            return seatsToAdd.Select(s => new SeatResponse
            {
                Id = s.Id,
                Row = s.Row,
                Number = s.Number,
                SeatName = $"{s.Row}{s.Number}",
                Type = s.Type.ToString(),
                AuditoriumId = s.AuditoriumId,
                IsActive = s.IsActive
            });
        }

        public async Task<SeatResponse> UpdateSeat(Guid id, SeatRequest request)
        {
            var seat = await _seatRepository.GetByIdAsync(id);
            if (seat == null)
                throw new Exception("Seat not found");

            var auditorium = await _auditoriumRepository.GetByIdAsync(request.AuditoriumId);
            if (auditorium == null)
                throw new Exception("Auditorium not found");

            seat.Row = request.Row;
            seat.Number = request.Number;
            seat.Type = request.Type;
            seat.AuditoriumId = request.AuditoriumId;
            seat.IsActive = request.IsActive;

            await _seatRepository.UpdateAsync(seat);

            return new SeatResponse
            {
                Id = seat.Id,
                Row = seat.Row,
                Number = seat.Number,
                SeatName = $"{seat.Row}{seat.Number}",
                Type = seat.Type.ToString(),
                AuditoriumId = seat.AuditoriumId,
                IsActive = seat.IsActive
            };
        }

        public async Task DeleteSeat(Guid id)
        {
            var seat = await _seatRepository.GetByIdAsync(id);
            if (seat == null)
                throw new Exception("Seat not found");

            await _seatRepository.DeleteAsync(seat);
        }

        public async Task ActivateSeat(Guid id)
        {
            var seat = await _seatRepository.GetByIdAsync(id);
            if (seat == null)
                throw new Exception("Seat not found");

            seat.IsActive = true;
            await _seatRepository.UpdateAsync(seat);
        }

        public async Task DeactivateSeat(Guid id)
        {
            var seat = await _seatRepository.GetByIdAsync(id);
            if (seat == null)
                throw new Exception("Seat not found");

            seat.IsActive = false;
            await _seatRepository.UpdateAsync(seat);
        }
    }
}
