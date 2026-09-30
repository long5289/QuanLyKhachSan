namespace HotelBooking.Web.Models;

public class RoomSearchResultViewModel
{
    public RoomSearchViewModel Search { get; set; } = new();
    public IReadOnlyList<RoomSearchItemViewModel> Rooms { get; set; } = [];
}

public class RoomSearchItemViewModel
{
    public Room Room { get; set; } = null!;
    public Review? RecentReview { get; set; }
}