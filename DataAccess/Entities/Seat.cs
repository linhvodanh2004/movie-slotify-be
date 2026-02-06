using System;

namespace DataAccess.Entities
{
    public enum SeatType
    {
        Standard,
        VIP,
        Couple
    }

    public class Seat
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Row { get; set; }
        public int Number { get; set; }
        public SeatType Type { get; set; }
        
        public Guid AuditoriumId { get; set; }
        public Auditorium Auditorium { get; set; }
    }
}
