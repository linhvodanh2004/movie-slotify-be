using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Entities;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.BackgroundJobs
{
    /// <summary>
    /// Cron job chạy mỗi 5 phút, xóa các booking Pending quá 15 phút.
    /// </summary>
    public class BookingExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingExpiryService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan ExpiryTime = TimeSpan.FromMinutes(15);

        public BookingExpiryService(IServiceScopeFactory scopeFactory, ILogger<BookingExpiryService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingExpiryService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(Interval, stoppingToken);

                try
                {
                    await ExpireOldBookingsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while expiring bookings.");
                }
            }
        }

        private async Task ExpireOldBookingsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cutoff = DateTime.UtcNow - ExpiryTime;

            // Xóa toàn bộ Ticket liên quan đến booking sắp bị xóa (tránh FK violation)
            var expiredBookingIds = await db.Bookings
                .Where(b => b.Status == BookingStatus.Pending && b.BookingDate < cutoff)
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);

            if (expiredBookingIds.Count == 0) return;

            // Xóa tickets trước (FK constraint)
            await db.Tickets
                .Where(t => expiredBookingIds.Contains(t.BookingId))
                .ExecuteDeleteAsync(cancellationToken);

            // Xóa booking
            var deleted = await db.Bookings
                .Where(b => expiredBookingIds.Contains(b.Id))
                .ExecuteDeleteAsync(cancellationToken);

            _logger.LogInformation("BookingExpiryService: Deleted {Count} expired pending bookings.", deleted);
        }
    }
}
