namespace HotelBooking.Web.Models;

public class AdminReportViewModel
{
    public decimal ConfirmedRevenue { get; set; }
    public int CompletedBookings { get; set; }
    public int TotalCustomers { get; set; }
    public IReadOnlyList<RoomOccupancyItem> RoomOccupancy { get; set; } = [];
}

public class RoomOccupancyItem
{
    public string RoomName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
}