using System;

namespace BusinessLogic.DTOs.responses
{
    public class CinemaResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public bool IsActive { get; set; }
        public int NumberOfAuditoriums { get; set; }
    }
}
