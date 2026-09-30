namespace HotelBooking.Web.Models;

public class AdminRoomsViewModel
{
    public Room NewRoom { get; set; } = new();
    public IReadOnlyList<Room> Rooms { get; set; } = [];
}