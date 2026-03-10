using System;

namespace BusinessLogic.DTOs.responses
{
    public class AuditoriumResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CinemaId { get; set; }
        public string CinemaName { get; set; }
        public bool IsActive { get; set; }
        public int TotalSeats { get; set; }
    }
}
