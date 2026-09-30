namespace HotelBooking.Web.Models;

public class AdminDashboardViewModel
{
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int PendingBookings { get; set; }
    public decimal Revenue { get; set; }
    public IReadOnlyList<Booking> RecentBookings { get; set; } = [];
}