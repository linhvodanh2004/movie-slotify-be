using System;

namespace BusinessLogic.DTOs.responses
{
    public class ShowtimeResponse
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public decimal StandardPrice { get; set; }
        public decimal VipPrice { get; set; }
        public decimal CouplePrice { get; set; }

        public Guid MovieId { get; set; }
        public string MovieTitle { get; set; }
        public string MoviePosterUrl { get; set; }

        public Guid AuditoriumId { get; set; }
        public string AuditoriumName { get; set; }

        public Guid CinemaId { get; set; }
        public string CinemaName { get; set; }
    }
}
