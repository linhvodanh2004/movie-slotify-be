using System;
using System.Collections.Generic;

namespace DataAccess.Entities
{
    public class Auditorium
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        
        public Guid CinemaId { get; set; }
        public Cinema Cinema { get; set; }
        
        public ICollection<Seat> Seats { get; set; }
    }
}
