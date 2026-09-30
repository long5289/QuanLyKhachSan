namespace HotelBooking.Web.Models;

public class HomeViewModel
{
    public RoomSearchViewModel Search { get; set; } = new();
    public IReadOnlyList<Room> FeaturedRooms { get; set; } = [];
    public IReadOnlyList<Service> Services { get; set; } = [];
    public IReadOnlyList<Review> Reviews { get; set; } = [];
}