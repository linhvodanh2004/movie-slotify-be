using System.Collections.Generic;

namespace BusinessLogic.DTOs.responses
{
    public class AdminDashboardResponse
    {
        public int TicketsSoldToday { get; set; }
        public int PaidBookingsTodayCount { get; set; }
        public decimal RevenueToday { get; set; }

        public List<BookingResponse> RecentPaidBookings { get; set; } = new();
    }
}

