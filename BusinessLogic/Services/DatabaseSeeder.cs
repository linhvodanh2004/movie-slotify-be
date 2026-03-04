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
                        PosterUrl = "https://cdn.betacinemas.vn/media/console/catalog/product/m/a/maiii_1.jpg",
                        TrailerUrl = "https://www.youtube.com/watch?v=kYv_W5ZlV8w",
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
                        PosterUrl = "https://cdn.betacinemas.vn/media/console/catalog/product/p/o/poster_-_dao_pho_piano_1.jpg",
                        TrailerUrl = "https://www.youtube.com/watch?v=Fj-yZ212Gbw",
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
                        PosterUrl = "https://cdn.betacinemas.vn/media/console/catalog/product/g/l/glcb.jpg",
                        TrailerUrl = "https://www.youtube.com/watch?v=e_0bEaH6kE0",
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
                        PosterUrl = "https://cdn.betacinemas.vn/media/console/catalog/product/q/c/qc.jpg",
                        TrailerUrl = "https://www.youtube.com/watch?v=zR27HnIQcwM",
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
                        PosterUrl = "https://cdn.betacinemas.vn/media/console/catalog/product/l/m/lm7-mdk.jpg",
                        TrailerUrl = "https://www.youtube.com/watch?v=6hJzO3O9sF0",
                        IsActive = true
                    }
                };

                await _context.Movies.AddRangeAsync(movies);
            }

            await _context.SaveChangesAsync();
        }
    }
}
