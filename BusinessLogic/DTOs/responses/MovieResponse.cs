using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.responses
{
    public class MovieResponse
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Director { get; set; }
        public string? Cast { get; set; }
        public string? Genre { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string? PosterUrl { get; set; }
        public string? TrailerUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
