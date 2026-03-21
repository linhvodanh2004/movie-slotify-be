using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Exceptions;
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
                throw new NotFoundException("Không tìm thấy ghế.");

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
            ValidateSeatRequest(request);

            var auditorium = await _auditoriumRepository.GetByIdAsync(request.AuditoriumId);
            if (auditorium == null)
                throw new NotFoundException("Không tìm thấy phòng chiếu.");

            var normalizedRow = NormalizeRow(request.Row);
            if (await _seatRepository.ExistsAsync(request.AuditoriumId, normalizedRow, request.Number))
                throw new ValidationException($"Ghế {normalizedRow}{request.Number} đã tồn tại trong phòng này.");

            var seat = new Seat
            {
                Row = normalizedRow,
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
            if (requests == null)
                throw new ValidationException("Danh sách ghế không hợp lệ.");

            var requestList = requests.ToList();
            if (!requestList.Any())
                throw new ValidationException("Danh sách ghế không được để trống.");

            foreach (var req in requestList)
            {
                ValidateSeatRequest(req);
            }

            var distinctAuditoriumIds = requestList.Select(r => r.AuditoriumId).Distinct().ToList();
            if (distinctAuditoriumIds.Count != 1)
                throw new ValidationException("Chỉ được thêm nhiều ghế cho cùng một phòng chiếu.");

            var auditoriumId = distinctAuditoriumIds[0];
            var auditorium = await _auditoriumRepository.GetByIdAsync(auditoriumId);
            if (auditorium == null)
                throw new NotFoundException("Không tìm thấy phòng chiếu.");

            var duplicateKeys = requestList
                .GroupBy(r => $"{NormalizeRow(r.Row)}-{r.Number}")
                .Where(g => g.Count() > 1)
                .Select(g => g.Key.Replace("-", string.Empty))
                .ToList();

            if (duplicateKeys.Any())
                throw new ValidationException($"Danh sách ghế bị trùng: {string.Join(", ", duplicateKeys)}.");

            var seatsToAdd = new List<Seat>();
            foreach (var req in requestList)
            {
                var normalizedRow = NormalizeRow(req.Row);
                if (await _seatRepository.ExistsAsync(req.AuditoriumId, normalizedRow, req.Number))
                    throw new ValidationException($"Ghế {normalizedRow}{req.Number} đã tồn tại trong phòng này.");

                seatsToAdd.Add(new Seat
                {
                    Row = normalizedRow,
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
            ValidateSeatRequest(request);

            var seat = await _seatRepository.GetByIdAsync(id);
            if (seat == null)
                throw new NotFoundException("Không tìm thấy ghế.");

            var auditorium = await _auditoriumRepository.GetByIdAsync(request.AuditoriumId);
            if (auditorium == null)
                throw new NotFoundException("Không tìm thấy phòng chiếu.");

            var normalizedRow = NormalizeRow(request.Row);
            if (await _seatRepository.ExistsAsync(request.AuditoriumId, normalizedRow, request.Number, id))
                throw new ValidationException($"Ghế {normalizedRow}{request.Number} đã tồn tại trong phòng này.");

            seat.Row = normalizedRow;
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
                throw new NotFoundException("Không tìm thấy ghế.");

            if (await _seatRepository.HasTicketsAsync(id))
                throw new BadRequestException("Không thể xóa ghế đã phát sinh vé.");

            await _seatRepository.DeleteAsync(seat);
        }

        public async Task ActivateSeat(Guid id)
        {
            var seat = await _seatRepository.GetByIdAsync(id);
            if (seat == null)
                throw new NotFoundException("Không tìm thấy ghế.");

            seat.IsActive = true;
            await _seatRepository.UpdateAsync(seat);
        }

        public async Task DeactivateSeat(Guid id)
        {
            var seat = await _seatRepository.GetByIdAsync(id);
            if (seat == null)
                throw new NotFoundException("Không tìm thấy ghế.");

            seat.IsActive = false;
            await _seatRepository.UpdateAsync(seat);
        }

        private static void ValidateSeatRequest(SeatRequest request)
        {
            if (request == null)
                throw new ValidationException("Dữ liệu ghế không hợp lệ.");

            NormalizeRow(request.Row);

            if (request.Number < 1 || request.Number > 100)
                throw new ValidationException("Số ghế phải nằm trong khoảng từ 1 đến 100.");

            if (!Enum.IsDefined(typeof(SeatType), request.Type))
                throw new ValidationException("Loại ghế không hợp lệ.");

            if (request.AuditoriumId == Guid.Empty)
                throw new ValidationException("Phòng chiếu là bắt buộc.");
        }

        private static string NormalizeRow(string row)
        {
            if (string.IsNullOrWhiteSpace(row))
                throw new ValidationException("Hàng ghế là bắt buộc.");

            var normalized = row.Trim().ToUpper();
            if (normalized.Length > 5)
                throw new ValidationException("Mã hàng ghế không được vượt quá 5 ký tự.");

            if (normalized.Any(c => !char.IsLetterOrDigit(c)))
                throw new ValidationException("Mã hàng ghế chỉ được chứa chữ cái và số.");

            return normalized;
        }
    }
}
