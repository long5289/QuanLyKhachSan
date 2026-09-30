namespace HotelBooking.Web.Models;

public class RoomCalendarViewModel
{
    public DateTime Month { get; set; }
    public IReadOnlyList<DateTime> Days { get; set; } = [];
    public IReadOnlyList<RoomCalendarEntryViewModel> Rooms { get; set; } = [];
}

public class RoomCalendarEntryViewModel
{
    public Room Room { get; set; } = null!;
    public Review? RecentReview { get; set; }
    public IReadOnlyList<RoomCalendarDayViewModel> Days { get; set; } = [];
    public IReadOnlyList<RoomCalendarDayViewModel?> CalendarSlots { get; set; } = [];
}

public class RoomCalendarDayViewModel
{
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}