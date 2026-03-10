using DataAccess.Entities;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class DatabaseSeeder
    {
        private readonly AppDbContext _context;

        public DatabaseSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Seed Admin User
            if (!await _context.Users.AnyAsync(u => u.Role == "ADMIN"))
            {
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@slotify.com",
                    FullName = "System Admin",
                    PhoneNumber = "0123456789",
                    Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Role = "ADMIN",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(adminUser);
            }

            // Seed Movies
            if (!await _context.Movies.AnyAsync())
            {
                var movies = new List<Movie>
                {
                    new Movie
                    {
                        Id = Guid.NewGuid(),
                        Title = "Mai",
                        Description = "Câu chuyện về cuộc đời của một người phụ nữ tên Mai, với những góc khuất trong cuộc sống và tình yêu.",
                        Director = "Trấn Thành",
                        Cast = "Phương Anh Đào, Tuấn Trần, Trấn Thành, Hồng Đào",
                        Genre = "Tâm Lý, Tình Cảm",
                        DurationMinutes = 131,
                        ReleaseDate = new DateTime(2024, 2, 10),
                        PosterUrl = "https://cdn-i.doisongphapluat.com.vn/media/trieu-phuong-linh/2024/02/20/sau-10-ngay-cong-chieu-phim-mai-bat-ngo-tung-poster-dac-biet.jpg",
                        TrailerUrl = "https://youtu.be/HXWRTGbhb4U?si=-5WRHJxFt5pA6wn0",
                        IsActive = true
                    },
                    new Movie
                    {
                        Id = Guid.NewGuid(),
                        Title = "Đào, Phở và Piano",
                        Description = "Bối cảnh phim lấy cảm hứng từ cuộc chiến 60 ngày đêm bảo vệ Hà Nội cuối năm 1946, đầu năm 1947.",
                        Director = "Phi Tiến Sơn",
                        Cast = "Doãn Quốc Đam, Cao Thùy Linh, Trần Lực",
                        Genre = "Chiến Tranh, Tâm Lý",
                        DurationMinutes = 100,
                        ReleaseDate = new DateTime(2024, 2, 10),
                        PosterUrl = "https://upload.wikimedia.org/wikipedia/vi/2/29/%C3%81p_ph%C3%ADch_%C4%90%C3%A0o%2C_ph%E1%BB%9F_v%C3%A0_piano.jpg",
                        TrailerUrl = "https://youtu.be/qn1t_biQigc?si=hS0t05v5y41rfv_L",
                        IsActive = true
                    },
                    new Movie
                    {
                        Id = Guid.NewGuid(),
                        Title = "Gặp Lại Chị Bầu",
                        Description = "Anh thanh niên tên Phúc vô tình quay trở lại quá khứ năm 1997 và gặp gỡ những người bạn mới tại xóm trọ bà Lê.",
                        Director = "Nhất Trung",
                        Cast = "Anh Tú, Diệu Nhi, Ngọc Phước, Quốc Khánh",
                        Genre = "Hài, Tình Cảm",
                        DurationMinutes = 114,
                        ReleaseDate = new DateTime(2024, 2, 10),
                        PosterUrl = "https://cdn.galaxycine.vn/media/2024/2/6/gap-lai-chi-bau-500_1707203931098.jpg",
                        TrailerUrl = "https://youtu.be/_sJ0rRhTK84?si=5SBbBw7UnbA1S88q",
                        IsActive = true
                    },
                    new Movie
                    {
                        Id = Guid.NewGuid(),
                        Title = "Quỷ Cẩu",
                        Description = "Gia đình làm nghề giết mổ chó phải đối mặt với những hiện tượng kỳ lạ và đáng sợ sau cái chết đột ngột của người cha.",
                        Director = "Lưu Thành Luân",
                        Cast = "Quang Tuấn, Nam Thư, NSND Kim Xuân",
                        Genre = "Kinh Dị",
                        DurationMinutes = 108,
                        ReleaseDate = new DateTime(2023, 12, 29),
                        PosterUrl = "https://upload.wikimedia.org/wikipedia/vi/c/c3/Qu%E1%BB%B7_C%E1%BA%A9u_poster.jpg",
                        TrailerUrl = "https://youtu.be/t4LVt_L9jWM?si=wJ7ZBwTasNp5Cbk1",
                        IsActive = true
                    },
                    new Movie
                    {
                        Id = Guid.NewGuid(),
                        Title = "Lật Mặt 7: Một Điều Ước",
                        Description = "Một câu chuyện gia đình đầy cảm động xoay quanh người mẹ già và những người con trưởng thành.",
                        Director = "Lý Hải",
                        Cast = "Thanh Hiền, Trương Minh Cường, Đinh Y Nhung",
                        Genre = "Gia Đình, Tâm Lý",
                        DurationMinutes = 138,
                        ReleaseDate = new DateTime(2024, 4, 26),
                        PosterUrl = "https://upload.wikimedia.org/wikipedia/vi/d/d4/%C3%81p_ph%C3%ADch_ch%C3%ADnh_th%E1%BB%A9c_L%E1%BA%ADt_m%E1%BA%B7t_7.jpg",
                        TrailerUrl = "https://youtu.be/d1ZHdosjNX8?si=eZ5BRC4F60TijW-8",
                        IsActive = true
                    }
                };

                await _context.Movies.AddRangeAsync(movies);
            }

            await _context.SaveChangesAsync();
        }
    }
}
