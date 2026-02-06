using System;

namespace DataAccess.Entities
{
    public class Showtime
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public decimal StandardPrice { get; set; }
        public decimal VipPrice { get; set; }
        public decimal CouplePrice { get; set; }

        public Guid MovieId { get; set; }
        public Movie Movie { get; set; }

        public Guid AuditoriumId { get; set; }
        public Auditorium Auditorium { get; set; }
    }
}
