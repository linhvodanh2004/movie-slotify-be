using System;
using System.Collections.Generic;

namespace DataAccess.Entities
{
    public class Cinema
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        
        // Navigation property
        public ICollection<Auditorium> Auditoriums { get; set; }
    }
}
